using KubeOps.Operator.Entities.Annotations;

namespace ArmKubeOper.Entities
{

    /// <summary>
    /// Specifies that an existing Azure resource group should be located.
    /// </summary>
    public class AzureResourceGroupQuery
    {

        /// <summary>
        /// ID of the Azure subscription to locate.
        /// </summary>
        [Required]
        public AzureSubscriptionLink Subscription { get; set; }

        /// <summary>
        /// Name of the Azure resource group within the specified subscription.
        /// </summary>
        [Required]
        public string Name { get; set; }

    }

}