using k8s.Models;

using KubeOps.Operator.Entities;

namespace ArmKubeOper.Entities
{

    /// <summary>
    /// Creates and monitors an Azure resource group.
    /// </summary>
    [KubernetesEntity(Group = "arm.azure.com", ApiVersion = "v1")]
    public class AzureResourceGroup : CustomKubernetesEntity<AzureResourceGroupSpec, AzureResourceGroupStatus>
    {



    }

}