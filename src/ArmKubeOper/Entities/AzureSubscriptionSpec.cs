using KubeOps.Operator.Entities.Annotations;

namespace ArmKubeOper.Entities
{

    public class AzureSubscriptionSpec
    {

        /// <summary>
        /// Name of the Azure subscription.
        /// </summary>
        [Required]
        public string Name { get; set; }

    }

}
