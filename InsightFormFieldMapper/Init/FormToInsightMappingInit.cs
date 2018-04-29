using System;
using System.Linq;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Forms.Core;
using EPiServer.Forms.EditView;
using EPiServer.Forms.Helpers.Internal;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.Localization;
using EPiServer.Security;
using EPiServer.ServiceLocation;

namespace InsightFormFieldMapper.Init
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Forms.EditView.InitializationModule))]
    public class FormToInsightMappingInit : IInitializableModule
    {
        public const string InsightProfileMappingPropertyName = "Forms_InsightProfileFieldMappings";

        private static readonly object _lock = new object();
        private Injected<IContentTypeRepository> _contentTypeRepository;
        private Injected<ITabDefinitionRepository> _tabDefinitionRepository;
        private Injected<IPropertyDefinitionRepository> _propertyDefinitionRepository;
        private Injected<IPropertyDefinitionTypeRepository> _propertyDefinitionTypeRepository;
        private Injected<LocalizationService> _localizationService;

        public void Initialize(InitializationEngine context)
        {
            SetupInsightFormMappingProperties();
        }

        private void SetupInsightFormMappingProperties()
        {
            CreateOrDeleteTab("Insight profile mappings", IsInsightInstalled());
            var allowMappingType = typeof(IUIExternalFieldMapping);
            var derivedTypes = typeof(BlockBase).GetDerivedTypes();
            foreach (var modelType in derivedTypes)
            {
                var contentType = _contentTypeRepository.Service.Load(modelType);
                if (contentType != null)
                {
                    if (IsInsightInstalled() && allowMappingType.IsAssignableFrom(modelType))
                    {
                        CreateUpdatePropertyDefinition(contentType, InsightProfileMappingPropertyName,
                            _localizationService.Service.GetString("/episerver/forms/editview/insightmapping", "Insight property"),
                            typeof(PropertyString), "Insight profile mappings", 10);
                    }
                    //else
                    //{
                    //    this.RemovePropertyDefinition(contentType, "Forms_ExternalSystemsFieldMappings");
                    //}
                }
            }
        }

        private void CreateOrDeleteTab(string tabName, bool createNew)
        {
            var obj2 = _lock;
            lock (obj2)
            {
                TabDefinition tabDefinition = this._tabDefinitionRepository.Service.Load(tabName);
                if (createNew)
                {
                    if (tabDefinition != null) return;

                    tabDefinition = new TabDefinition
                    {
                        Name = tabName,
                        SortIndex = 300,
                        RequiredAccess = AccessLevel.Administer
                    };
                    _tabDefinitionRepository.Service.Save(tabDefinition);
                }
                else if (tabDefinition != null)
                {
                    _tabDefinitionRepository.Service.Delete(tabDefinition);
                }
            }
        }

        private void CreateUpdatePropertyDefinition(ContentType contentType, string propertyDefinitionName, string editCaption = null, Type propertyDefinitionType = null, string tabName = null, int? propertyOrder = new int?())
        {
            PropertyDefinition propertyDefinition =
                GetPropertyDefinition(contentType, propertyDefinitionName);
            if (propertyDefinition == null)
            {
                if (propertyDefinitionType == null)
                {
                    return;
                }
                propertyDefinition = new PropertyDefinition();
            }
            else
            {
                propertyDefinition = propertyDefinition.CreateWritableClone();
            }
            propertyDefinition.ContentTypeID = contentType.ID;
            propertyDefinition.DisplayEditUI = true;
            propertyDefinition.DefaultValueType = DefaultValueType.None;
            if (propertyDefinitionName != null)
            {
                propertyDefinition.Name = propertyDefinitionName;
            }
            if (editCaption != null)
            {
                propertyDefinition.EditCaption = editCaption;
            }
            if (propertyDefinitionType != null)
            {
                propertyDefinition.Type = this._propertyDefinitionTypeRepository.Service.Load(propertyDefinitionType);
            }
            if (tabName != null)
            {
                propertyDefinition.Tab = _tabDefinitionRepository.Service.Load(tabName);
            }
            if (propertyOrder.HasValue)
            {
                propertyDefinition.FieldOrder = propertyOrder.Value;
            }
            _propertyDefinitionRepository.Service.Save(propertyDefinition);
        }

        private PropertyDefinition GetPropertyDefinition(ContentType contentType, string propertyName, Type propertyDefinitionType = null)
        {
            var source = from pd in contentType.PropertyDefinitions
                where pd.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase)
                select pd;
            if (propertyDefinitionType != null)
            {
                source = from pd in source
                    where pd.Type.DefinitionType == propertyDefinitionType
                    select pd;
            }
            return source.FirstOrDefault();
        }

        private bool IsInsightInstalled()
        {

            return true;
        }

        public void Uninitialize(InitializationEngine context) { }
    }
}