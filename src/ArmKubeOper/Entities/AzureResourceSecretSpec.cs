
using System;

using KubeOps.Operator.Entities.Annotations;

namespace ArmKubeOper.Entities
{

    public class AzureResourceSecretSpec
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
        /// Optionally, retrieve the secret data from a nested endpoint of the resource. This can be used to invoke the List* family of operations.
        /// </summary>
        public AzureResourceEndpoint Endpoint { get; set; }

        /// <summary>
        /// The amount of time between reading the values from the resource.
        /// </summary>
        public string RefreshInterval { get; set; }

        /// <summary>
        /// Specification of the target secret.
        /// </summary>
        [Required]
        public AzureSecretRef Target { get; set; }

        /// <summary>
        /// Name of the key within the secret to set.
        /// </summary>
        [Required]
        public AzureSecretTemplate Template { get; set; }

    }

}
