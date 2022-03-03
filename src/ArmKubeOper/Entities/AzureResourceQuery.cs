using KubeOps.Operator.Entities.Annotations;

namespace ArmKubeOper.Entities
{

    /// <summary>
    /// Specifies that an existing Azure resource should be located.
    /// </summary>
    public class AzureResourceQuery
    {

        /// <summary>
        /// Method of locating the resource group for the resource.
        /// </summary>
        public AzureResourceGroupLink ResourceGroup { get; set; }

        /// <summary>
        /// Parent resource. Typed as a <see cref="AzureResourceLink"/>.
        /// </summary>
        [PreserveUnknownFields]
        public object ParentResource { get; set; }

        /// <summary>
        /// Name of the resource provider of the resource.
        /// </summary>
        [Required]
        public string ResourceProvider { get; set; }

        /// <summary>
        /// Type of the resource.
        /// </summary>
        [Required]
        public string ResourceType { get; set; }

        /// <summary>
        /// Name of the Azure resource.
        /// </summary>
        [Required]
        public string Name { get; set; }

    }

}