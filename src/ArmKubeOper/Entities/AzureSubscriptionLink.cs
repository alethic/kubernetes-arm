namespace ArmKubeOper.Entities
{

    /// <summary>
    /// Determines how to create or find an Azure subscription.
    /// </summary>
    public class AzureSubscriptionLink
    {

        /// <summary>
        /// If specified, an existing Azure subscription will be located.
        /// </summary>
        public AzureSubscriptionQuery Query { get; set; }

        /// <summary>
        /// If specified, the Azure subscription will be provided by the located AzureSubscription resource.
        /// </summary>
        public AzureSubscriptionRef Ref { get; set; }

    }

}
