using KubeOps.Operator.Entities.Annotations;

namespace ArmKubeOper.Entities
{

    /// <summary>
    /// References an existing AzureSubscription resource.
    /// </summary>
    public class AzureSubscriptionRef
    {

        /// <summary>
        /// Optional Kubernetes namespace of the AzureSubscription resource.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Name of the Kubernetes AzureSubscription resource.
        /// </summary>
        [Required]
        public string Name { get; set; }

    }

}