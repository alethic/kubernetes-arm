using System.Collections.Generic;

namespace ArmKubeOper.Entities
{

    public class AzureSecretTemplate
    {

        /// <summary>
        /// Gets the 'type' of the resulting secret.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets the metadata of the resulting secret.
        /// </summary>
        public AzureTemplateMetadata Metadata { get; set; }

        /// <summary>
        /// Gets the data of the resulting secret.
        /// </summary>
        public Dictionary<string, string> Data { get; set; }

        /// <summary>
        /// Gets the data of the resulting secret.
        /// </summary>
        public Dictionary<string, string> StringData { get; set; }

    }

}