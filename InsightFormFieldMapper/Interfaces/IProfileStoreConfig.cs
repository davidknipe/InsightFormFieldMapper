namespace InsightFormFieldMapper.Interfaces
{
    public interface IProfileStoreConfig
    {
        string RootApiUrl { get; }
        string SubscriptionKey { get; }
        bool IsConfigured();
    }
}