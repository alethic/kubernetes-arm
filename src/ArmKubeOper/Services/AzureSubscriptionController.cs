using System;
using System.Threading;
using System.Threading.Tasks;

using ArmKubeOper.Entities;
using ArmKubeOper.Services;

using KubeOps.Operator.Controller;
using KubeOps.Operator.Controller.Results;
using KubeOps.Operator.Rbac;

using Microsoft.Extensions.Logging;

namespace ArmKubeOper.Services
{

    [EntityRbac(typeof(AzureSubscription), Verbs = RbacVerb.All)]
    public class AzureSubscriptionController : IResourceController<AzureSubscription>
    {

        readonly AzureService azure;
        readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="azure"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AzureSubscriptionController(AzureService azure, ILogger logger)
        {
            this.azure = azure ?? throw new ArgumentNullException(nameof(azure));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ResourceControllerResult> ReconcileAsync(AzureSubscription entity)
        {
            if (await azure.ReconcileAsync(entity, CancellationToken.None))
                return null;
            else
                return ResourceControllerResult.RequeueEvent(TimeSpan.FromSeconds(30));
        }

        public Task StatusModifiedAsync(AzureSubscription entity)
        {
            logger.LogInformation("StatusModifiedAsync {Entity}.", entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(AzureSubscription entity)
        {
            logger.LogInformation("DeleteAsync {Entity}.", entity);
            return Task.CompletedTask;
        }

    }

}
