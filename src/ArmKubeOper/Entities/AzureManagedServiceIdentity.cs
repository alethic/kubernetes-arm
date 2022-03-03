using System.Collections.Generic;

namespace ArmKubeOper.Entities
{

    public class AzureManagedServiceIdentity
    {

        public string PrincipalId { get; set; }

        public string TenantId { get; set; }

        public string Type { get; set; }

        public Dictionary<string, AzureUserAssignedIdentity> UserAssignedIdentities { get; set; }

    }

}