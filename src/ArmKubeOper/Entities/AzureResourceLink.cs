namespace ArmKubeOper.Entities
{

    /// <summary>
    /// Determines how to create or find an Azure resource.
    /// </summary>
    public class AzureResourceLink
    {

        /// <summary>
        /// If specified, an existing Azure resource will be located.
        /// </summary>
        public AzureResourceQuery Query { get; set; }

        /// <summary>
        /// If specified, the Azure resource will be provided by the located AzureResource resource.
        /// </summary>
        public AzureResourceRef Ref { get; set; }

    }

}