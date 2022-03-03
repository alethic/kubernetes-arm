namespace ArmKubeOper.Entities
{

    /// <summary>
    /// Determines how to create or find an Azure resource group.
    /// </summary>
    public class AzureResourceGroupLink
    {

        /// <summary>
        /// If specified, an existing Azure resource group will be located.
        /// </summary>
        public AzureResourceGroupQuery Query { get; set; }

        /// <summary>
        /// If specified, the Azure resource group will be provided by the located AzureResourceGroup resource.
        /// </summary>
        public AzureResourceGroupRef Ref { get; set; }

    }

}