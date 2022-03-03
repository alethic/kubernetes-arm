using System;
using System.Threading;
using System.Threading.Tasks;

using ArmKubeOper.Entities;

using KubeOps.Operator.Finalizer;

using Microsoft.Extensions.Logging;

namespace ArmKubeOper.Services
{

    /// <summary>
    /// Finalizes AzureResources.
    /// </summary>
    public class AzureResourceFinalizer : IResourceFinalizer<AzureResource>
    {

        readonly AzureService azure;
        readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="azure"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AzureResourceFinalizer(AzureService azure, ILogger logger)
        {
            this.azure = azure ?? throw new ArgumentNullException(nameof(azure));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Finalizes the entity before removal.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="AzureServiceException"></exception>
        public async Task FinalizeAsync(AzureResource entity)
        {
            if (await azure.DeleteAsync(entity, CancellationToken.None))
                return;
            else
                throw new AzureServiceException("Unable to delete associated Azure resource.");
        }

    }

}
