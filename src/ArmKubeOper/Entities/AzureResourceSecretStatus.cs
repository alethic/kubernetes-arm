using System;

namespace ArmKubeOper.Entities
{

    public class AzureResourceSecretStatus
    {

        /// <summary>
        /// ID of the resource being listed.
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        /// Namespace of the resulting secret.
        /// </summary>
        public string SecretNamespace { get; set; }

        /// <summary>
        /// Name of the resulting secret.
        /// </summary>
        public string SecretName { get; set; }

        /// <summary>
        /// The time and date the resource value was fetched and the target config map was updated.
        /// </summary>
        public DateTime? RefreshTime { get; set; }

        /// <summary>
        /// Current state of the reconcilation.
        /// </summary>
        public string State { get; internal set; }

        /// <summary>
        /// Last error that occurred attempting to reconcile.
        /// </summary>
        public string Error { get; set; }

    }

}