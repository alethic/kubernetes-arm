using KubeOps.Operator.Entities.Annotations;

namespace ArmKubeOper.Entities
{

    public class AzureTenantResourceSpec
    {
        
        [Required]
        public string TenantId { get; set; }

        [Required]
        public string ResourceProvider { get; set; }

        [Required]
        public string ResourceType { get; set; }

        [Required]
        public string ApiVersion { get; set; }

        [Required]
        [PreserveUnknownFields]
        public object Properties { get; set; }

    }

}
