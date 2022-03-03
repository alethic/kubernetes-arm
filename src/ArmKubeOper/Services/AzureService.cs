using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using ArmKubeOper.Entities;
using ArmKubeOper.Extensions;

using Azure;
using Azure.Core;
using Azure.ResourceManager.Resources;

using Cogito;
using Cogito.Autofac;

using DotnetKubernetesClient;

using HandlebarsDotNet;

using k8s.Models;

using KubeOps.Operator.Events;

using Newtonsoft.Json.Linq;

using Serilog;

namespace ArmKubeOper.Services
{

    /// <summary>
    /// Provides services for interacting with AzureResourceGroup related entities.
    /// </summary>
    [RegisterAs(typeof(AzureService))]
    public class AzureService
    {

        /// <summary>
        /// Concatinates two dictionaries.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        static IDictionary<string, string> Concat(IReadOnlyDictionary<string, string> a, IReadOnlyDictionary<string, string> b)
        {
            var d = new Dictionary<string, string>(a);
            foreach (var kvp in b)
                d[kvp.Key] = kvp.Value;
            return d;
        }

        readonly AzureArmClient arm;
        readonly IKubernetesClient k8s;
        readonly IEventManager evt;
        readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="arm"></param>
        /// <param name="k8s"></param>
        /// <param name="evt"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AzureService(AzureArmClient arm, IKubernetesClient k8s, IEventManager evt, ILogger logger)
        {
            this.arm = arm ?? throw new ArgumentNullException(nameof(arm));
            this.k8s = k8s ?? throw new ArgumentNullException(nameof(k8s));
            this.evt = evt ?? throw new ArgumentNullException(nameof(evt));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region AzureSubscription

        /// <summary>
        /// Tries to retrieve the resource group.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<Subscription> TryGetSubscriptionAsync(string id, CancellationToken cancellationToken)
        {
            var response = await arm.GetSubscriptions().GetIfExistsAsync(id, cancellationToken);
            return response.Value;
        }

        /// <summary>
        /// Retrieves a new status structure for the resource group.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> ReconcileAsync(AzureSubscription entity, CancellationToken cancellationToken)
        {
            try
            {
                if (entity.Status.State == null)
                {
                    entity.Status.State = "Pending";
                    entity.Status.Error = null;
                    await k8s.UpdateStatus(entity);
                }

                var subscription = await ApplyAsync(entity, cancellationToken);
                if (subscription == null)
                {
                    entity.Status.Id = null;
                    entity.Status.SubscriptionId = null;
                    entity.Status.TenantId = null;
                    entity.Status.Name = null;
                    entity.Status.Tags = null;
                    entity.Status.State = null;
                    entity.Status.Error = null;
                    await k8s.UpdateStatus(entity);
                    await evt.PublishAsync(entity, "Missing", "Subscription specified by ID is missing.", EventType.Warning);
                    return false;
                }

                entity.Status.Id = subscription.Id;
                entity.Status.SubscriptionId = subscription.Data.SubscriptionId;
                entity.Status.TenantId = subscription.Data.TenantId;
                entity.Status.Name = subscription.Data.DisplayName;
                entity.Status.Tags = new Dictionary<string, string>(subscription.Data.Tags);
                entity.Status.State = "Success";
                entity.Status.Error = null;
                await k8s.UpdateStatus(entity);
                await evt.PublishAsync(entity, "Success", "Subscription successfully updated.", EventType.Normal);
                return true;
            }
            catch (RequestFailedException e)
            {
                entity.Status.State = "Faulted";
                entity.Status.Error = e.ErrorCode;
                await k8s.UpdateStatus(entity);
                await evt.PublishAsync(entity, "Faulted", "Subscription update faulted.", EventType.Normal);
                throw;
            }
            catch (Exception e)
            {
                entity.Status.State = "Faulted";
                entity.Status.Error = e.Message;
                await k8s.UpdateStatus(entity);
                await evt.PublishAsync(entity, "Faulted", "Subscription update faulted.", EventType.Normal);
                throw;
            }
        }

        /// <summary>
        /// Applies the given subscription specification.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<Subscription> ApplyAsync(AzureSubscription entity, CancellationToken cancellationToken)
        {
            if (entity.Status.Id == null)
            {
                throw new NotImplementedException("Cannot yet manage subscriptions.");
            }
            else
            {
                var subscription = await TryGetSubscriptionAsync(entity.Status.Id, cancellationToken);
                if (subscription != null)
                    subscription = await UpdateSubscriptionAsync(subscription, entity, cancellationToken);

                return subscription;
            }
        }

        /// <summary>
        /// Updates the given resource group in response to the specification.
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<Subscription> UpdateSubscriptionAsync(Subscription subscription, AzureSubscription entity, CancellationToken cancellationToken)
        {
            if (subscription.Data.Tags["k8s-uuid"] != entity.Metadata.Uid)
                throw new AzureServiceException($"Cannot update Azure subscription '{entity.Status.Id}'. Existing subscription does not contain tag indicating ownership by this entity.");

            //var metadata = new Dictionary<string, string>()
            //{
            //    ["k8s-apiversion"] = entity.ApiVersion,
            //    ["k8s-kind"] = entity.Kind,
            //    ["k8s-uuid"] = entity.Metadata.Uid,
            //    ["k8s-namespace"] = entity.Metadata.NamespaceProperty,
            //    ["k8s-name"] = entity.Metadata.Name,
            //};

            return subscription;
        }

        #endregion

        #region AzureResourceGroup

        /// <summary>
        /// Tries to retrieve the resource group.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<ResourceGroup> TryGetResourceGroupAsync(string id, CancellationToken cancellationToken)
        {
            var res = await arm.GetResourceGroup(new ResourceIdentifier(id)).GetAsync(cancellationToken);
            return res.Value;
        }

        /// <summary>
        /// Tries to retrieve the resource group.
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="name"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<ResourceGroup> TryGetResourceGroupAsync(Subscription subscription, string name, CancellationToken cancellationToken)
        {
            var res = await subscription.GetResourceGroups().GetIfExistsAsync(name, cancellationToken);
            return res.Value;
        }

        /// <summary>
        /// Retrieves a new status structure for the resource group.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> ReconcileAsync(AzureResourceGroup entity, CancellationToken cancellationToken)
        {
            try
            {
                if (entity.Status.State == null)
                {
                    entity.Status.State = "Pending";
                    entity.Status.Error = null;
                    await k8s.UpdateStatus(entity);
                }

                var resource = await ApplyAsync(entity, cancellationToken);
                if (resource == null)
                {
                    entity.Status.Id = null;
                    entity.Status.Name = null;
                    entity.Status.Location = null;
                    entity.Status.Tags = null;
                    entity.Status.Properties = null;
                    entity.Status.State = null;
                    entity.Status.Error = null;
                    await k8s.UpdateStatus(entity);
                    await evt.PublishAsync(entity, "Missing", "Resource group specified by ID is missing.", EventType.Warning);
                    return false;
                }

                entity.Status.Id = resource.Id;
                entity.Status.Name = resource.Data.Name;
                entity.Status.Location = resource.Data.Location;
                entity.Status.Tags = new Dictionary<string, string>(resource.Data.Tags);
                entity.Status.Properties = new Dictionary<string, string>() { ["ProvisioningState"] = resource.Data.Properties.ProvisioningState };
                entity.Status.State = "Success";
                entity.Status.Error = null;
                await k8s.UpdateStatus(entity);
                await evt.PublishAsync(entity, "Success", "Resource group successfully updated.", EventType.Normal);
                return true;
            }
            catch (RequestFailedException e)
            {
                entity.Status.State = "Faulted";
                entity.Status.Error = e.ErrorCode;
                await k8s.UpdateStatus(entity);
                await evt.PublishAsync(entity, "Faulted", "Resource group update faulted.", EventType.Normal);
                throw;
            }
            catch (Exception e)
            {
                entity.Status.State = "Faulted";
                entity.Status.Error = e.Message;
                await k8s.UpdateStatus(entity);
                await evt.PublishAsync(entity, "Faulted", "Resource group update faulted.", EventType.Normal);
                throw;
            }
        }

        /// <summary>
        /// Applies the given resource group specification.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<ResourceGroup> ApplyAsync(AzureResourceGroup entity, CancellationToken cancellationToken)
        {
            if (entity.Status.Id == null)
            {
                var subscription = await ResolveAsync(entity.Spec.Subscription, entity.Namespace(), cancellationToken);
                if (subscription == null)
                    throw new AzureServiceException("Failed to resolve subscription.");

                var group = await TryGetResourceGroupAsync(subscription, entity.Spec.Name, cancellationToken);
                if (group == null)
                    group = await CreateResourceGroupAsync(subscription, entity, cancellationToken);
                else
                    group = await UpdateResourceGroupAsync(group, entity, cancellationToken);

                return group;
            }
            else
            {
                var group = await TryGetResourceGroupAsync(entity.Status.Id, cancellationToken);
                if (group != null)
                    group = await UpdateResourceGroupAsync(group, entity, cancellationToken);

                return group;
            }
        }

        /// <summary>
        /// Creates the given resource group with the given specification.
        /// </summary>
        /// <param name="subscription"></param>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<ResourceGroup> CreateResourceGroupAsync(Subscription subscription, AzureResourceGroup entity, CancellationToken cancellationToken)
        {
            logger.Information("Creating Azure resource group: {SubscriptionId} {ResourceGroupName}", entity.Spec.Name);
            var ops = await subscription.GetResourceGroups().CreateOrUpdateAsync(true, entity.Spec.Name, GetUpdateData(entity), cancellationToken);
            return await UpdateResourceGroupAsync(ops.Value, entity, cancellationToken);
        }

        /// <summary>
        /// Updates the given resource group in response to the specification.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<ResourceGroup> UpdateResourceGroupAsync(ResourceGroup group, AzureResourceGroup entity, CancellationToken cancellationToken)
        {
            if (group.Data.Tags["k8s-uuid"] != entity.Metadata.Uid)
                throw new AzureServiceException($"Cannot update Azure resource group '{entity.Status.Id}'. Existing resource does not contain tag indicating ownership by this entity.");

            logger.Information("Updating Azure resource group: {Id}", group.Id);
            var ops = await group.SetTagsAsync(entity.Spec.Tags, cancellationToken);
            return ops.Value;
        }

        /// <summary>
        /// Converts a specification to resource group data.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        ResourceGroupData GetUpdateData(AzureResourceGroup entity)
        {
            var data = new ResourceGroupData(entity.Spec.Location);

            foreach (var kvp in entity.Spec.Tags)
                data.Tags[kvp.Key] = kvp.Value;

            data.Tags["k8s-apiversion"] = entity.ApiVersion;
            data.Tags["k8s-kind"] = entity.Kind;
            data.Tags["k8s-uuid"] = entity.Metadata.Uid;
            data.Tags["k8s-namespace"] = entity.Metadata.NamespaceProperty;
            data.Tags["k8s-name"] = entity.Metadata.Name;

            return data;
        }

        #endregion

        #region AzureResource

        /// <summary>
        /// Tries to retrieve the resource.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<GenericResource> TryGetResourceAsync(string id, CancellationToken cancellationToken)
        {
            var res = await arm.GetGenericResources().GetIfExistsAsync(new ResourceIdentifier(id), cancellationToken);
            return res.Value;
        }

        /// <summary>
        /// Tries to retrieve the resource.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="provider"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<GenericResource> TryGetResourceAsync(ResourceGroup group, string provider, string type, string name, CancellationToken cancellationToken)
        {
            if (group is null)
                throw new ArgumentNullException(nameof(group));
            if (string.IsNullOrEmpty(provider))
                throw new ArgumentException($"'{nameof(provider)}' cannot be null or empty.", nameof(provider));
            if (string.IsNullOrEmpty(type))
                throw new ArgumentException($"'{nameof(type)}' cannot be null or empty.", nameof(type));
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

            var res = await arm.GetGenericResources().GetIfExistsAsync(new ResourceIdentifier($"{group.Id}/providers/{provider}/{type}/{name}"), cancellationToken);
            return res.Value;
        }

        /// <summary>
        /// Attempts to retrieve the nested resource of an existing parent resource.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<GenericResource> TryGetResourceAsync(GenericResource parent, string type, string name, CancellationToken cancellationToken)
        {
            if (parent is null)
                throw new ArgumentNullException(nameof(parent));
            if (string.IsNullOrEmpty(type))
                throw new ArgumentException($"'{nameof(type)}' cannot be null or empty.", nameof(type));
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"'{nameof(name)}' cannot be null or empty.", nameof(name));

            var res = await arm.GetGenericResources().GetIfExistsAsync(new ResourceIdentifier($"{parent.Id}/{type}/{name}"));
            return res.Value;
        }

        /// <summary>
        /// Tries to retrieve the resource.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="provider"></param>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<GenericResource> TryGetResourceAsync(GenericResource parent, string provider, string type, string name, CancellationToken cancellationToken)
        {
            var res = await arm.GetGenericResources().GetIfExistsAsync(new ResourceIdentifier($"{parent.Id}/providers/{provider}/{type}/{name}"));
            return res.Value;
        }

        /// <summary>
        /// Retrieves a new status structure for the resource group.
        /// </summary>
        /// <param name="entity">sssss</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> ReconcileAsync(AzureResource entity, CancellationToken cancellationToken)
        {
            try
            {
                if (entity.Status.State == null)
                {
                    entity.Status.State = "Pending";
                    entity.Status.Error = null;
                    await k8s.UpdateStatus(entity);
                }

                var resource = await ApplyAsync(entity, cancellationToken);
                if (resource == null)
                {
                    entity.Status.Id = null;
                    entity.Status.Resource = null;
                    entity.Status.State = null;
                    entity.Status.Error = null;
                    await k8s.UpdateStatus(entity);
                    await evt.PublishAsync(entity, "Missing", "Resource specified by ID is missing.", EventType.Warning);
                    return false;
                }

                entity.Status.Id = resource.Id;
                entity.Status.Resource = await GetResourceObjectAsync(resource, cancellationToken);
                entity.Status.State = "Success";
                entity.Status.Error = null;
                await k8s.UpdateStatus(entity);
                await evt.PublishAsync(entity, "Success", "Resource successfully updated.", EventType.Normal);
                return true;
            }
            catch (RequestFailedException e)
            {
                entity.Status.State = "Faulted";
                entity.Status.Error = e.ErrorCode;
                await k8s.UpdateStatus(entity);
                await evt.PublishAsync(entity, "Faulted", "Resource update faulted.", EventType.Normal);
                throw;
            }
            catch (Exception e)
            {
                entity.Status.State = "Faulted";
                entity.Status.Error = e.Message;
                await k8s.UpdateStatus(entity);
                await evt.PublishAsync(entity, "Faulted", "Resource update faulted.", EventType.Normal);
                throw;
            }
        }

        /// <summary>
        /// Applies the resource specification.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<GenericResource> ApplyAsync(AzureResource entity, CancellationToken cancellationToken)
        {
            if (entity.Status.Id == null)
            {
                var group = await ResolveAsync(entity.Spec.ResourceGroup, entity.Namespace(), cancellationToken);
                if (group == null)
                    throw new AzureServiceException("Failed to resolve resource group.");

                var resource = await TryGetResourceAsync(group, entity.Spec.ResourceProvider, entity.Spec.ResourceType, entity.Spec.Name, cancellationToken);
                if (resource == null)
                    resource = await CreateResourceAsync(group, entity, cancellationToken);
                else
                    resource = await UpdateResourceAsync(resource, entity, cancellationToken);

                return resource;
            }
            else
            {
                var resource = await TryGetResourceAsync(entity.Status.Id, cancellationToken);
                if (resource != null)
                    resource = await UpdateResourceAsync(resource, entity, cancellationToken);

                return resource;
            }
        }

        /// <summary>
        /// Creates the given resource with the given specification.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<GenericResource> CreateResourceAsync(ResourceGroup group, AzureResource entity, CancellationToken cancellationToken)
        {
            logger.Information("Creating Azure resource: {ResourceGroupId} {ResourceProvider} {ResourceType} {Name}", group.Id, entity.Spec.ResourceProvider, entity.Spec.ResourceType, entity.Spec.Name);
            var ops = await arm.GetGenericResources().CreateOrUpdateAsync(true, new ResourceIdentifier($"{group.Id}/providers/{entity.Spec.ResourceProvider}/{entity.Spec.ResourceType}/{entity.Spec.Name}"), GetUpdateData(entity), cancellationToken);
            return await UpdateResourceAsync(ops.Value, entity, cancellationToken);
        }

        /// <summary>
        /// Updates the given resource in response to the specification.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<GenericResource> UpdateResourceAsync(GenericResource resource, AzureResource entity, CancellationToken cancellationToken)
        {
            if (resource.Data.Tags["k8s-uuid"] != entity.Metadata.Uid)
                throw new AzureServiceException("Cannot update Azure resource {Id}. Existing resource does not contain tag indicating ownership by this entity.");

            logger.Information("Updating Azure resource: {Id}", resource.Id);
            var ops = await resource.UpdateAsync(true, GetUpdateData(entity), cancellationToken);
            return ops.Value;
        }

        /// <summary>
        /// Converts a specification to resource data.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        GenericResourceData GetUpdateData(AzureResource entity)
        {
            // hacky method of taking the K8s object data and applying a template and returning a new object, converted to a GenericResourceData
            var t = ApplyTemplate(((JsonElement)entity.Spec.Template).Deserialize<JsonNode>(), new JsonObject());
            var d = JsonDocument.Parse(t.ToJsonString());
            var l = typeof(GenericResourceData).GetMethod("DeserializeGenericResourceData", BindingFlags.NonPublic | BindingFlags.Static);
            var r = (GenericResourceData)l.Invoke(null, new object[] { d.RootElement });

            // apply any missing metadata
            r.Tags["k8s-apiversion"] = entity.ApiVersion;
            r.Tags["k8s-kind"] = entity.Kind;
            r.Tags["k8s-uuid"] = entity.Metadata.Uid;
            r.Tags["k8s-namespace"] = entity.Metadata.NamespaceProperty;
            r.Tags["k8s-name"] = entity.Metadata.Name;

            return r;
        }

        /// <summary>
        /// Applies the data to the template to return a final object.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        JsonNode ApplyTemplate(JsonNode template, JsonNode data)
        {
            return template switch
            {
                JsonValue v => ApplyTemplate(v, data),
                JsonObject o => ApplyTemplate(o, data),
                JsonArray a => ApplyTemplate(a, data),
                _ => throw new NotImplementedException(),
            };
        }

        /// <summary>
        /// Applies the data to the template to return a final object.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        JsonValue ApplyTemplate(JsonValue template, JsonNode data)
        {
            return template.GetValueKind() switch
            {
                JsonValueKind.String => ApplyTemplate(template.GetValue<string>(), data),
                _ => template,
            };
        }

        /// <summary>
        /// Applies the data to the template to return a final object.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        JsonValue ApplyTemplate(string template, JsonNode data)
        {
            return JsonValue.Create(Handlebars.Compile(template)(data));
        }

        /// <summary>
        /// Applies the data to the template to return a final object.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        JsonObject ApplyTemplate(JsonObject template, JsonNode data)
        {
            return new JsonObject(template.Select(kvp => new KeyValuePair<string, JsonNode?>(kvp.Key, ApplyTemplate(kvp.Value, data))));
        }

        /// <summary>
        /// Applies the data to the template to return a final object.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        JsonArray ApplyTemplate(JsonArray template, JsonNode data)
        {
            return new JsonArray(template.Select(i => ApplyTemplate(i, data)).ToArray());
        }

        /// <summary>
        /// Converts a generic resource to a status.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<JsonNode> GetResourceObjectAsync(GenericResource source, CancellationToken cancellationToken)
        {
            // sort of a hack to serialize the object
            var l = Type.GetType("Azure.Core.IUtf8JsonSerializable, Azure.ResourceManager").GetMethod("Write");
            using var m = new MemoryStream();
            using var w = new Utf8JsonWriter(m);
            l.Invoke(source.Data, new object[] { w });
            w.Flush();
            m.Position = 0;
            return JsonNode.Parse(m);
        }

        /// <summary>
        /// Ensures the entity is deleted.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(AzureResource entity, CancellationToken cancellationToken)
        {
            try
            {
                logger.Debug("Beginning deletion of AzureResource: {Namespace}/{Name}.", entity.Metadata.NamespaceProperty, entity.Metadata.Name);

                entity.Status.State = "Deleting";
                entity.Status.Error = null;
                await k8s.UpdateStatus(entity);

                if (entity.Status.Id != null)
                {
                    var resource = await TryGetResourceAsync(entity.Status.Id, cancellationToken);
                    if (resource != null)
                    {
                        if (resource.Data.Tags["k8s-uuid"] != entity.Metadata.Uid)
                            throw new AzureServiceException("Unable to delete {Id}. Resource does not contain proper ownership tags and may be part of another system.");

                        logger.Debug("Deleting Azure resource: {Id}.", resource.Id);
                        var response = await resource.DeleteAsync(true, cancellationToken);
                    }
                }

                entity.Status.Id = null;
                entity.Status.Resource = null;
                entity.Status.State = "Deleted";
                entity.Status.Error = null;
                await k8s.UpdateStatus(entity);
                await evt.PublishAsync(entity, "Deleted", "Resource successfully deleted.", EventType.Normal);
                return true;
            }
            catch (RequestFailedException e)
            {
                entity.Status.State = "Faulted";
                entity.Status.Error = e.ErrorCode;
                await k8s.UpdateStatus(entity);
                await evt.PublishAsync(entity, "Faulted", "Resource delete faulted.", EventType.Warning);
                throw;
            }
            catch (Exception e)
            {
                entity.Status.State = "Faulted";
                entity.Status.Error = e.Message;
                await k8s.UpdateStatus(entity);
                await evt.PublishAsync(entity, "Faulted", "Resource delete faulted.", EventType.Warning);
                throw;
            }
        }

        #endregion

        #region Resolve

        /// <summary>
        /// Attempts to follow a <see cref="AzureSubscriptionLink"/> to the resulting subscription.
        /// </summary>
        /// <param name="link"></param>
        /// <param name="defaultNamespace"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Task<Subscription> ResolveAsync(AzureSubscriptionLink link, string defaultNamespace, CancellationToken cancellationToken)
        {
            if (link.Query != null)
                return ResolveQueryAsync(link.Query, defaultNamespace, cancellationToken);
            if (link.Ref != null)
                return ResolveRefAsync(link.Ref, defaultNamespace, cancellationToken);

            throw new AzureServiceException("AzureSubscriptionRef missing find and link.");
        }

        /// <summary>
        /// Attempts to resolve a subscription based on search criteria.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="defaultNamespace"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        Task<Subscription> ResolveQueryAsync(AzureSubscriptionQuery query, string defaultNamespace, CancellationToken cancellationToken)
        {
            if (query.Id != null)
                return TryGetSubscriptionAsync(query.Id, cancellationToken);
            if (query.Name != null)
                throw new NotImplementedException("Cannot find a subscription by name.");

            return null;
        }

        /// <summary>
        /// Attempts to resolve a subscription through another <see cref="AzureSubscription"/> entity.
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="defaultNamespace"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="AzureServiceException"></exception>
        async Task<Subscription> ResolveRefAsync(AzureSubscriptionRef reference, string defaultNamespace, CancellationToken cancellationToken)
        {
            var entity = await k8s.Get<AzureSubscription>(reference.Name, reference.Namespace ?? defaultNamespace);
            if (entity == null)
                throw new AzureServiceException("Failed to locate AzureSubscription object.");
            if (entity.Status.Id == null || entity.Status.State != "Success")
                return null;

            return await arm.GetSubscription(new ResourceIdentifier(entity.Status.Id)).GetAsync(cancellationToken);
        }

        /// <summary>
        /// Attempts to follow a <see cref="AzureResourceGroupLink"/> to the resulting resource group.
        /// </summary>
        /// <param name="link"></param>
        /// <param name="defaultNamespace"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Task<ResourceGroup> ResolveAsync(AzureResourceGroupLink link, string defaultNamespace, CancellationToken cancellationToken)
        {
            if (link.Query != null)
                return ResolveQueryAsync(link.Query, defaultNamespace, cancellationToken);
            if (link.Ref != null)
                return ResolveRefAsync(link.Ref, defaultNamespace, cancellationToken);

            throw new AzureServiceException("AzureResourceGroupRef missing find and link.");
        }

        /// <summary>
        /// Attempts to resolve a resource group based on search criteria.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="defaultNamespace"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="AzureServiceException"></exception>
        async Task<ResourceGroup> ResolveQueryAsync(AzureResourceGroupQuery query, string defaultNamespace, CancellationToken cancellationToken)
        {
            if (query.Subscription == null)
                throw new AzureServiceException("Cannot find a resource group without a subscription reference.");
            if (query.Name == null)
                throw new AzureServiceException("Cannot find a resource group without a name.");
            if (query.Name.Length == 0)
                throw new AzureServiceException("Cannot find a resource group without a name.");

            var subscription = await ResolveAsync(query.Subscription, defaultNamespace, cancellationToken);
            if (subscription == null)
                throw new AzureServiceException("Failed to resolve subscription.");

            return await TryGetResourceGroupAsync(subscription, query.Name, cancellationToken);
        }

        /// <summary>
        /// Attempts to resolve a resource group through another <see cref="AzureResourceGroup"/> entity.
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="defaultNamespace"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async Task<ResourceGroup> ResolveRefAsync(AzureResourceGroupRef reference, string defaultNamespace, CancellationToken cancellationToken)
        {
            var entity = await k8s.Get<AzureResourceGroup>(reference.Name, reference.Namespace ?? defaultNamespace);
            if (entity == null)
                return null;
            if (entity.Status.Id == null || entity.Status.State != "Success")
                return null;

            return await arm.GetResourceGroup(new ResourceIdentifier(entity.Status.Id)).GetAsync(cancellationToken);
        }

        /// <summary>
        /// Attempts to follow a <see cref="AzureResourceLink"/> to the resulting resource.
        /// </summary>
        /// <param name="link"></param>
        /// <param name="defaultNamespace"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public Task<GenericResource> ResolveAsync(AzureResourceLink link, string defaultNamespace, CancellationToken cancellationToken)
        {
            if (link.Query != null)
                return ResolveQueryAsync(link.Query, defaultNamespace, cancellationToken);
            if (link.Ref != null)
                return ResolveRefAsync(link.Ref, defaultNamespace, cancellationToken);

            throw new AzureServiceException("AzureResourceRef missing find and link.");
        }

        /// <summary>
        /// Attempts to resolve a subscription based on search criteria.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="defaultNamespace"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        async Task<GenericResource> ResolveQueryAsync(AzureResourceQuery query, string defaultNamespace, CancellationToken cancellationToken)
        {
            // ref may specify a parent
            if (query.ParentResource != null)
            {
                // stored as object because of recursion
                var parentRef = ((JsonElement)query.ParentResource).Deserialize<AzureResourceLink>();
                if (parentRef == null)
                    throw new AzureServiceException("Could not transform Parent into AzureResourceRef.");

                // recursively resolve parent
                var parent = await ResolveAsync(parentRef, defaultNamespace, cancellationToken);
                if (parent == null)
                    return null;

                if (query.ResourceProvider != null)
                    return await TryGetResourceAsync(parent, query.ResourceProvider, query.ResourceType, query.Name, cancellationToken);
                else
                    return await TryGetResourceAsync(parent, query.ResourceType, query.Name, cancellationToken);
            }

            // ref may then specify a resource group
            if (query.ResourceGroup != null)
            {
                var resourceGroup = await ResolveAsync(query.ResourceGroup, defaultNamespace, cancellationToken);
                if (resourceGroup == null)
                    return null;

                return await TryGetResourceAsync(resourceGroup, query.ResourceProvider, query.ResourceType, query.Name, cancellationToken);
            }

            throw new AzureServiceException("Neither parent nor resourceGroup specified on resource ref find.");
        }

        /// <summary>
        /// Attempts to resolve a subscription through another <see cref="AzureResource"/> entity.
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="defaultNamespace"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="AzureServiceException"></exception>
        async Task<GenericResource> ResolveRefAsync(AzureResourceRef reference, string defaultNamespace, CancellationToken cancellationToken)
        {
            var entity = await k8s.Get<AzureResource>(reference.Name, reference.Namespace ?? defaultNamespace);
            if (entity == null)
                throw new AzureServiceException("Failed to locate AzureResource object.");
            if (entity.Status.Id == null || entity.Status.State != "Success")
                return null;

            return await TryGetResourceAsync(entity.Status.Id, cancellationToken);
        }

        #endregion

        #region AzureResourceSecret

        /// <summary>
        /// Reconciles the state of an <see cref="AzureResourceSecret"/>.
        /// </summary>
        /// <param name="entity">sssss</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> ReconcileAsync(AzureResourceSecret entity, CancellationToken cancellationToken)
        {
            try
            {
                // user specified a refresh interval, check that our next refresh time is not > now
                if (entity.Spec.RefreshInterval != null)
                {
                    var ts = ArmKubeOper.Extensions.TimeSpanExtensions.ParseKubeString(entity.Spec.RefreshInterval);
                    if (entity.Status.RefreshTime != null && entity.Status.RefreshTime + ts > DateTime.UtcNow)
                        return true;
                }

                // resolve the specified resource
                var resource = await ResolveAsync(entity.Spec.Resource, entity.Namespace(), cancellationToken);
                if (resource == null)
                    throw new AzureServiceException("Resource group could not be located.");

                // get or create a new owned secret
                var secret = await k8s.Get<V1Secret>(entity.Spec.Target.Name, entity.Spec.Target.Namespace ?? entity.Namespace());
                if (secret == null)
                {
                    secret = new V1Secret();
                    secret.EnsureMetadata().SetNamespace(entity.Spec.Target.Namespace ?? entity.Namespace());
                    secret.EnsureMetadata().Name = entity.Spec.Target.Name;
                    secret.AddOwnerReference(new V1OwnerReference() { ApiVersion = entity.ApiVersion, Kind = entity.Kind, Name = entity.Metadata.Name, Uid = entity.Metadata.Uid });
                    secret = await k8s.Create(secret);
                }

                // we shouldn't be managing a secret we don't own
                if (secret.IsOwnedBy(entity) == false)
                    throw new AzureServiceException("Secret is not owned by this AzureResource.");

                // an endpoint is specified, invoke the endpoint to obtain the data
                JsonNode data = null;
                if (entity.Spec.Endpoint != null)
                    data = await GetResourceEndpointAsync(resource, entity.Spec.ApiVersion, entity.Spec.Endpoint, cancellationToken);
                else
                    // no endpoint, use resource data itself
                    data = JsonNode.Parse(System.Text.Json.JsonSerializer.Serialize(resource.Data));

                // applies the template to the entity
                ApplyTemplate(entity.Spec.Template, secret, data);

                // update the secret value and status
                await k8s.Update(secret);
                entity.Status.SecretNamespace = secret.Namespace();
                entity.Status.SecretName = secret.Name();
                entity.Status.RefreshTime = DateTime.UtcNow;
                entity.Status.State = "Success";
                entity.Status.Error = null;
                await k8s.UpdateStatus(entity);
                await evt.PublishAsync(entity, "Success", "Resource secret successfully updated.", EventType.Normal);
                return true;
            }
            catch (RequestFailedException e)
            {
                entity.Status.State = "Faulted";
                entity.Status.Error = e.ErrorCode;
                await k8s.UpdateStatus(entity);
                await evt.PublishAsync(entity, "Faulted", "Resource secret operation faulted.", EventType.Normal);
                throw;
            }
            catch (Exception e)
            {
                entity.Status.State = "Faulted";
                entity.Status.Error = e.Message;
                await k8s.UpdateStatus(entity);
                await evt.PublishAsync(entity, "Faulted", "Resource secret operation faulted.", EventType.Normal);
                throw;
            }
        }

        /// <summary>
        /// Gets the JSON data from an endpoint of a resource.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="apiVersion"></param>
        /// <param name="endpoint"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="AzureServiceException"></exception>
        async Task<JsonNode> GetResourceEndpointAsync(GenericResource resource, string apiVersion, AzureResourceEndpoint endpoint, CancellationToken cancellationToken)
        {
            // body might come in as an unstructured object, convert to JSON node
            JsonNode body = null;
            if (endpoint.Body != null)
                body = ((JsonElement)endpoint.Body).Deserialize<JsonNode>();

            // generate and send a new message to retrieve the data
            using var message = CreateEndpointMessage(resource, endpoint.Method, endpoint.Path, body, endpoint.Query, apiVersion);
            await arm.Pipeline.SendAsync(message, cancellationToken);

            // should be successful
            if (message.Response.Status != 200)
                throw new AzureServiceException("HTTP error retrieving secret.");

            // parse results
            using var value = await JsonDocument.ParseAsync(message.Response.ContentStream, default, cancellationToken);
            return value.Deserialize<JsonNode>();
        }

        /// <summary>
        /// Applies the given template object to the entity.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="secret"></param>
        /// <param name="data"></param>
        /// <exception cref="NotImplementedException"></exception>
        void ApplyTemplate(AzureSecretTemplate template, V1Secret secret, JsonNode data)
        {
            // handlebars is happier with an ExpandoObject, and System.Text.Json can't seem to fully deserialize it
            var dataObject = JToken.Parse(data.ToString()).ToObject<ExpandoObject>();

            // apply any type
            if (template.Type != null)
                secret.Type = template.Type;

            // apply any metadata
            if (template.Metadata != null)
            {
                if (template.Metadata.Annotations != null)
                    foreach (var kvp in template.Metadata.Annotations)
                        secret.SetAnnotation(kvp.Key, kvp.Value);
                if (template.Metadata.Labels != null)
                    foreach (var kvp in template.Metadata.Labels)
                        secret.SetLabel(kvp.Key, kvp.Value);
            }

            // apply any templates in data
            if (template.Data != null)
            {
                secret.Data ??= new Dictionary<string, byte[]>();
                foreach (var kvp in template.Data)
                    secret.Data[kvp.Key] = ExecuteTemplate(kvp.Value, dataObject) is string s ? Convert.FromBase64String(s) : null;
            }

            // apply any templates in stringData
            if (template.StringData != null)
            {
                secret.StringData ??= new Dictionary<string, string>();
                foreach (var kvp in template.StringData)
                    secret.StringData[kvp.Key] = ExecuteTemplate(kvp.Value, dataObject);
            }

            // remove any keys that are not present in template
            foreach (var kvp in secret.Data.ToArray())
                if (template.Data?.ContainsKey(kvp.Key) != true && template.StringData.ContainsKey(kvp.Key) != true)
                    secret.Data.Remove(kvp.Key);
        }

        /// <summary>
        /// Executes a template.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        string ExecuteTemplate(string template, object data)
        {
            return Handlebars.Compile(template)(data);
        }

        /// <summary>
        /// Creates a request for executing a List* operation.
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="method"></param>
        /// <param name="path"></param>
        /// <param name="body"></param>
        /// <param name="apiVersion"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        HttpMessage CreateEndpointMessage(GenericResource resource, string method, string path, JsonNode body, IDictionary<string, string> query, string apiVersion)
        {
            var message = arm.Pipeline.CreateMessage();
            var request = message.Request;
            var uri = arm.BaseUri;
            uri = uri.Combine(resource.Id);
            uri = uri.Combine(path);
            uri = uri.AppendQuery("api-version", apiVersion);

            foreach (var kvp in query)
                uri = uri.AppendQuery(kvp.Key, kvp.Value);

            request.Method = RequestMethod.Parse(method);
            request.Uri.Reset(uri);
            request.Headers.Add("Accept", "application/json");
            request.Content = body != null ? RequestContent.Create(body.ToString()) : null;
            return message;
        }

        #endregion

    }

}
