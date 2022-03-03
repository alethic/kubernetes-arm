using KubeOps.Operator.Entities.Annotations;

namespace ArmKubeOper.Entities
{

    public class AzureConfigMapRef
    {

        /// <summary>
        /// Namespace of the config map.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Name of the config map.
        /// </summary>
        [Required]
        public string Name { get; set; }

    }

}