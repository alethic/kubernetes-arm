using k8s.Models;

using KubeOps.Operator.Entities;

namespace ArmKubeOper.Entities
{

    /// <summary>
    /// Creates an monitors an Azure subscription.
    /// </summary>
    [KubernetesEntity(Group = "arm.azure.com", ApiVersion = "v1")]
    public class AzureSubscription : CustomKubernetesEntity<AzureSubscriptionSpec, AzureSubscriptionStatus>
    {



    }
}
