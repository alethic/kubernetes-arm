using KubeOps.Operator.Entities.Annotations;

namespace ArmKubeOper.Entities
{

    public class AzureSubscriptionResourceSpec
    {

        /// <summary>
        /// Method by which the Azure subscription is established.
        /// </summary>
        [Required]
        public AzureSubscriptionLink Subscription { get; set; }

        /// <summary>
        /// Name of the Resource Provider of the resource.
        /// </summary>
        [Required]
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
        /// Definition of the resource.
        /// </summary>
        [PreserveUnknownFields]
        public object Properties { get; set; }

    }

}
