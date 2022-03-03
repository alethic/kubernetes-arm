using System.Collections.Generic;

using KubeOps.Operator.Entities.Annotations;

namespace ArmKubeOper.Entities
{

    public class AzureResourceEndpoint
    {

        /// <summary>
        /// HTTP method to use against the endpoint.
        /// </summary>
        [Required]
        public string Method { get; set; }

        /// <summary>
        /// Name of the operation endpoint. An example might be "listKeys".
        /// </summary>
        [Required]
        public string Path { get; set; }

        /// <summary>
        /// Body of the request to send to the endpoint.
        /// </summary>
        [PreserveUnknownFields]
        public object Body { get; set; }

        /// <summary>
        /// Query strings to add to the request to the endpoint.
        /// </summary>
        public Dictionary<string, string> Query { get; set; }

    }

}