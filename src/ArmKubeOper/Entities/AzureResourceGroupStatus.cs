using System.Collections.Generic;

using KubeOps.Operator.Entities.Annotations;

namespace ArmKubeOper.Entities
{

    public class AzureResourceGroupStatus
    {

        /// <summary>
        /// Current ID of the resource group.
        /// </summary>
        public string Id { get; internal set; }

        /// <summary>
        /// Current name of the resource group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Current location of the resource group.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Current tags of the resource group.
        /// </summary>
        public Dictionary<string, string> Tags { get; set; }

        /// <summary>
        /// Current properties of the resource group.
        /// </summary>
        [PreserveUnknownFields]
        public object Properties { get; set; }

        /// <summary>
        /// State of the subscription.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Last error manipulating the subscription.
        /// </summary>
        public string Error { get; internal set; }

    }

}