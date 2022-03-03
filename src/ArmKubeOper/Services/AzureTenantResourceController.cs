using System.Threading.Tasks;

using ArmKubeOper.Entities;

using KubeOps.Operator.Controller;
using KubeOps.Operator.Controller.Results;
using KubeOps.Operator.Rbac;

namespace ArmKubeOper.Services
{

    [EntityRbac(typeof(AzureTenantResource), Verbs = RbacVerb.All)]
    public class AzureTenantResourceController : IResourceController<AzureTenantResource>
    {

        public async Task<ResourceControllerResult> ReconcileAsync(AzureTenantResource entity)
        {
            return null;
        }

    }

}
