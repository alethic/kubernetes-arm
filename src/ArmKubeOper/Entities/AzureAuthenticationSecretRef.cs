using KubeOps.Operator.Entities.Annotations;

namespace ArmKubeOper.Entities
{

    public class AzureAuthenticationSecretRef
    {

        [Required]
        public string Name { get; set; }

        [Required]
        public string KeyName { get; set; }

    }

}