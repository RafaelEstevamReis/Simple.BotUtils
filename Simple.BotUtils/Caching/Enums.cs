namespace Simple.BotUtils.Caching
{
    public enum ExpirationPolicy
    {
        DoNotRenew,
        LastUpdate,
        LastAccess,
    }
}