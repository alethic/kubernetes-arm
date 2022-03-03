using KubeOps.Operator.Entities.Annotations;

namespace ArmKubeOper.Entities
{

    public class AzureTemplateSecretRef
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

        /// <summary>
        /// Name of the secret key.
        /// </summary>
        [Required]
        public string Key { get; set; }

        /// <summary>
        /// Type of the secret, 'string' or 'base64'. If 'string' is specified, the secret data is decoded as UTF-8 character data. 'string' is the default.
        /// </summary>
        public string Encoding { get; set; }

    }

}
