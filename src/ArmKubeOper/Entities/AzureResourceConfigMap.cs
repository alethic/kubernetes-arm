using k8s.Models;

using KubeOps.Operator.Entities;

namespace ArmKubeOper.Entities
{

    /// <summary>
    /// Creates and monitors an Azure resource's list* operation for results and saves them in a configmap.
    /// </summary>
    [KubernetesEntity(Group = "arm.azure.com", ApiVersion = "v1")]
    public class AzureResourceConfigMap : CustomKubernetesEntity<AzureResourceConfigMapSpec, AzureResourceConfigMapStatus>
    {



    }

}
