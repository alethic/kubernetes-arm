using System.Collections.Generic;

namespace ArmKubeOper.Entities
{

    public class AzureSubscriptionStatus
    {

        /// <summary>
        /// ID of the subscription.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Unique ID of the subscription.
        /// </summary>
        public string SubscriptionId { get; internal set; }

        /// <summary>
        /// Tenant ID of the subscription.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Name of the subscription.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Tags of the subscription.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; }

        /// <summary>
        /// State of the subscription.
        /// </summary>
        public string State { get;  set; }

        /// <summary>
        /// Last error manipulating the subscription.
        /// </summary>
        public string Error { get; internal set; }

    }

}
