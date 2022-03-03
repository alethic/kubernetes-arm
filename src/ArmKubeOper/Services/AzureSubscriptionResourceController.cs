using System.Threading.Tasks;

using ArmKubeOper.Entities;

using k8s.Models;

using KubeOps.Operator.Controller;
using KubeOps.Operator.Controller.Results;
using KubeOps.Operator.Rbac;

namespace ArmKubeOper.Services
{

    [EntityRbac(typeof(AzureSubscription), Verbs = RbacVerb.All)]
    [EntityRbac(typeof(AzureSubscriptionResource), Verbs = RbacVerb.All)]
    public class AzureSubscriptionResourceController : IResourceController<AzureSubscriptionResource>
    {

        public async Task<ResourceControllerResult> ReconcileAsync(AzureSubscriptionResource entity)
        {
            return null;
        }

    }

}
