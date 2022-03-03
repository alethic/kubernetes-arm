using System.Collections.Generic;

namespace ArmKubeOper.Entities
{

    public class AzureConfigMapTemplate
    {

        /// <summary>
        /// Gets the metadata of the resulting configmap.
        /// </summary>
        public AzureTemplateMetadata Metadata { get; set; }

        /// <summary>
        /// Gets the data of the resulting configmap.
        /// </summary>
        public Dictionary<string, string> Data { get; set; }

    }

}