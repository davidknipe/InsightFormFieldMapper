using System.Configuration;
using EPiServer.ServiceLocation;
using InsightFormFieldMapper.Interfaces;

namespace InsightFormFieldMapper.Impl
{
    [ServiceConfiguration(typeof(IProfileStoreConfig), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ProfileStoreConfig : IProfileStoreConfig
    {
        public string RootApiUrl => ConfigurationManager.AppSettings["profileStore.RootApiUrl"];
        public string SubscriptionKey => ConfigurationManager.AppSettings["profileStore.SubscriptionKey"];

        public bool IsConfigured()
        {
            return !string.IsNullOrEmpty(ConfigurationManager.AppSettings["profileStore.RootApiUrl"]) &&
                   !string.IsNullOrEmpty(ConfigurationManager.AppSettings["profileStore.SubscriptionKey"]);
        }
    }
}
