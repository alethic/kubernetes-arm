using System.Collections.Generic;

using KubeOps.Operator.Entities.Annotations;

namespace ArmKubeOper.Entities
{

    public class AzureResourceGroupSpec
    {

        /// <summary>
        /// Method by which the Azure subscription is established.
        /// </summary>
        [Required]
        public AzureSubscriptionLink Subscription { get; set; }

        /// <summary>
        /// Name of the resource group.
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Desired location of the resource group.
        /// </summary>
        [Required]
        public string Location { get; set; }

        /// <summary>
        /// Desired tags of the resource group.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; }

        /// <summary>
        /// Desired properties of the resource group.
        /// </summary>
        [PreserveUnknownFields]
        public object Properties { get; set; }

    }

}