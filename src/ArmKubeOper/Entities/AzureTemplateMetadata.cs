using System.Collections.Generic;

namespace ArmKubeOper.Entities
{

    public class AzureTemplateMetadata
    {

        public Dictionary<string, string> Annotations { get; set; }

        public Dictionary<string,string> Labels { get; set; }

    }

}