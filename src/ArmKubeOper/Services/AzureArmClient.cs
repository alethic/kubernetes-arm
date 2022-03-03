using System;

using Azure.Core;
using Azure.Core.Pipeline;
using Azure.ResourceManager;

namespace ArmKubeOper.Services
{

    /// <summary>
    /// Customized version of the ARM client, making available various internal properties.
    /// </summary>
    public class AzureArmClient : ArmClient
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="credential"></param>
        /// <param name="options"></param>
        public AzureArmClient(TokenCredential credential, ArmClientOptions options = null) :
            base(credential, options)
        {

        }

        /// <summary>
        /// Gets the BaseUri.
        /// </summary>
        public new Uri BaseUri => base.BaseUri;

        /// <summary>
        /// Gets the HttpPipeline for making requests.
        /// </summary>
        public new HttpPipeline Pipeline => base.Pipeline;

    }

}
