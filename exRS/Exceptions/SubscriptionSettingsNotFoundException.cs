namespace exRS.Exceptions
{
    public class SubscriptionSettingsNotFoundException : ExRSException
    {
        public SubscriptionSettingsNotFoundException(string settings)
            : base($"Can't find settings {settings}. Please create subscription settings for this report first or remove -s parameter for this report.")
        {
        }
    }
}
