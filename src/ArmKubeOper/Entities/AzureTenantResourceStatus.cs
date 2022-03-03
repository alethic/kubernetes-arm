using KubeOps.Operator.Entities.Annotations;

namespace ArmKubeOper.Entities
{

    public class AzureTenantResourceStatus
    {

        [PreserveUnknownFields]
        public object Properties { get; set; }

    }

}