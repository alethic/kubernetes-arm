namespace ArmKubeOper.Entities
{

    /// <summary>
    /// Specifies that an existing Azure subscription should be located.
    /// </summary>
    public class AzureSubscriptionQuery
    {

        /// <summary>
        /// ID of the subscription to locate.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the subscription to locate.
        /// </summary>
        public string Name { get; set; }

    }

}
