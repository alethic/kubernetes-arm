using KubeOps.Operator.Entities.Annotations;

namespace ArmKubeOper.Entities
{

    public class AzureSubscriptionResourceStatus
    {

        /// <summary>
        /// Current definition of the resource.
        /// </summary>
        [PreserveUnknownFields]
        public object Properties { get; set; }

    }

}