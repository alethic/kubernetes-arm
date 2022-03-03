
using KubeOps.Operator.Entities.Annotations;

namespace ArmKubeOper.Entities
{

    public class AzureResourceSpec
    {

        /// <summary>
        /// Method by which the Azure resource group is established.
        /// </summary>
        public AzureResourceGroupLink ResourceGroup { get; set; }

        /// <summary>
        /// Optional reference to the parent Azure resource.
        /// </summary>
        [PreserveUnknownFields]
        public object Parent { get; set; }

        /// <summary>
        /// Name of the resource provider of the resource.
        /// </summary>
        public string ResourceProvider { get; set; }

        /// <summary>
        /// Type of the resource.
        /// </summary>
        [Required]
        public string ResourceType { get; set; }

        /// <summary>
        /// API Version used to interact with the resource.
        /// </summary>
        [Required]
        public string ApiVersion { get; set; }

        /// <summary>
        /// Name of the resource.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Template of the resource definition to monitor.
        /// </summary>
        [Required]
        [PreserveUnknownFields]
        public object Template { get; set; }

    }

}
