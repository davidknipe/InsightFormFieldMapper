using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Shell.ObjectEditing;
using InsightFormFieldMapper.Impl;

namespace InsightFormFieldMapper.Init
{
    [InitializableModule]
    [ModuleDependency(typeof(FormToInsightMappingInit))]
    public class RegisterMetaDataExtender : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            if (context.HostType == HostType.WebApplication)
            {
                var registry = context.Locate.Advanced.GetInstance<MetadataHandlerRegistry>();
                registry.RegisterMetadataHandler(typeof(ContentData), new InsightMappingMetaDataExtender());
            }
        }

        public void Uninitialize(InitializationEngine context) { }
    }
}