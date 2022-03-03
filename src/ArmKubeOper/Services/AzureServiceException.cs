using System;

namespace ArmKubeOper.Services
{

    /// <summary>
    /// Describes an error during processing by the <see cref="AzureService"/>.
    /// </summary>
    public class AzureServiceException : Exception
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="message"></param>
        public AzureServiceException(string message) :
            base(message)
        {

        }

    }

}
