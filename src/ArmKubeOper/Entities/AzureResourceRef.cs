using KubeOps.Operator.Entities.Annotations;

namespace ArmKubeOper.Entities
{

    /// <summary>
    /// References an existing AzureResource resource.
    /// </summary>
    public class AzureResourceRef
    {

        /// <summary>
        /// Optional Kubernetes namespace of the AzureResource resource.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Name of the AzureResource resource.
        /// </summary>
        [Required]
        public string Name { get; set; }

    }

}