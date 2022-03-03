using KubeOps.Operator.Entities.Annotations;

namespace ArmKubeOper.Entities
{

    /// <summary>
    /// Describes the current state of the resource.
    /// </summary>
    public class AzureResourceStatus
    {

        /// <summary>
        /// Current ID of the resource.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Current definition of the resource.
        /// </summary>
        [PreserveUnknownFields]
        public object Resource { get; set; }

        /// <summary>
        /// Current state of the resource.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Last error received.
        /// </summary>
        public string Error { get; set; }

    }

}
