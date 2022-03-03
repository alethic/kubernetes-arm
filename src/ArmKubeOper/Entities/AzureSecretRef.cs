using KubeOps.Operator.Entities.Annotations;

namespace ArmKubeOper.Entities
{

    public class AzureSecretRef
    {

        /// <summary>
        /// Namespace of the secret.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Name of the secret.
        /// </summary>
        [Required]
        public string Name { get; set; }

    }

}