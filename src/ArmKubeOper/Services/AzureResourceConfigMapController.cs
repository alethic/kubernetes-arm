﻿using System;
using System.Threading;
using System.Threading.Tasks;

using ArmKubeOper.Entities;

using KubeOps.Operator.Controller;
using KubeOps.Operator.Controller.Results;
using KubeOps.Operator.Finalizer;
using KubeOps.Operator.Rbac;

using Microsoft.Extensions.Logging;

namespace ArmKubeOper.Services
{

    /// <summary>
    /// Manages the lifecycle of an <see cref="AzureResourceConfigMap"/>.
    /// </summary>
    [EntityRbac(typeof(AzureResource), Verbs = RbacVerb.All)]
    [EntityRbac(typeof(AzureResourceGroup), Verbs = RbacVerb.All)]
    [EntityRbac(typeof(AzureSubscription), Verbs = RbacVerb.All)]
    [EntityRbac(typeof(AzureResourceConfigMap), Verbs = RbacVerb.All)]
    public class AzureResourceConfigMapController : IResourceController<AzureResourceConfigMap>
    {

        readonly AzureService azure;
        readonly IFinalizerManager<AzureResourceConfigMap> finalizer;
        readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="azure"></param>
        /// <param name="finalizer"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AzureResourceConfigMapController(AzureService azure, IFinalizerManager<AzureResourceConfigMap> finalizer, ILogger logger)
        {
            this.azure = azure ?? throw new ArgumentNullException(nameof(azure));
            this.finalizer = finalizer ?? throw new ArgumentNullException(nameof(finalizer));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Reconciles the state of the entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<ResourceControllerResult> ReconcileAsync(AzureResourceConfigMap entity)
        {
            if (await azure.ReconcileAsync(entity, CancellationToken.None))
            {
                await finalizer.RegisterAllFinalizersAsync(entity);
                return null;
            }
            else
                return ResourceControllerResult.RequeueEvent(TimeSpan.FromSeconds(30));
        }

    }

}