using KubeOps.Operator.Entities.Annotations;

namespace ArmKubeOper.Entities
{

    /// <summary>
    /// References an existing AzureResourceGroup resource.
    /// </summary>
    public class AzureResourceGroupRef
    {

        /// <summary>
        /// Optional Kubernetes namespace of the AzureResourceGroup resource.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Name of the AzureResourceGroup resource.
        /// </summary>
        [Required]
        public string Name { get; set; }

    }

}