namespace ArmKubeOper.Entities
{

    public class AzureAuthenticationSpec
    {

        /// <summary>
        /// Specifies that the authentication information will be delievered by Azure AD 
        /// </summary>
        public string PodIdentity { get; set; }

        /// <summary>
        /// Specifies that the authentication information will be stored in a secret. A secret by the given name must be
        /// within the same namespace as the as the AzureAuthentication.
        /// </summary>
        public AzureAuthenticationSecretRef SecretRef { get; set; }

    }

}