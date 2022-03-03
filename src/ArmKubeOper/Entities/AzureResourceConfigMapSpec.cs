
using System;

using KubeOps.Operator.Entities.Annotations;

namespace ArmKubeOper.Entities
{

    public class AzureResourceConfigMapSpec
    {

        /// <summary>
        /// Reference to the Azure resource to invoke the operation on.
        /// </summary>
        [Required]
        public AzureResourceLink Resource { get; set; }

        /// <summary>
        /// API version of the operation.
        /// </summary>
        [Required]
        public string ApiVersion { get; set; }

        /// <summary>
        /// Optionally, retrieve the configmap data from a nested endpoint of the resource. This can be used to invoke the List* family of operations.
        /// </summary>
        public AzureResourceEndpoint Endpoint { get; set; }

        /// <summary>
        /// The amount of time between reading the values from the resource.
        /// </summary>
        public string RefreshInterval { get; set; }

        /// <summary>
        /// Specification of the target configmap.
        /// </summary>
        [Required]
        public AzureConfigMapRef Target { get; set; }

        /// <summary>
        /// Description of the configmap to generate.
        /// </summary>
        [Required]
        public AzureConfigMapTemplate Template { get; set; }

    }

}
