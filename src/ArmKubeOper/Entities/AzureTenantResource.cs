using k8s.Models;

using KubeOps.Operator.Entities;

namespace ArmKubeOper.Entities
{

    [KubernetesEntity(Group = "arm.azure.com", ApiVersion = "v1")]
    public class AzureTenantResource : CustomKubernetesEntity<AzureTenantResourceSpec, AzureTenantResourceStatus>
    {



    }

}
