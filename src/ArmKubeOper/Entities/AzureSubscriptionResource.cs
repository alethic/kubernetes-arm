using k8s.Models;

using KubeOps.Operator.Entities;

namespace ArmKubeOper.Entities
{

    /// <summary>
    /// Creates and monitors an Azure Subscription Resource.
    /// </summary>
    [KubernetesEntity(Group = "arm.azure.com", ApiVersion = "v1")]
    public class AzureSubscriptionResource : CustomKubernetesEntity<AzureSubscriptionResourceSpec, AzureSubscriptionResourceStatus>
    {



    }
}
