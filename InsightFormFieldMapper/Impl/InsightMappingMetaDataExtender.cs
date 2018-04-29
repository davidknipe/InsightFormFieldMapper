using System;
using System.Collections.Generic;
using System.Globalization;
using EPiServer;
using EPiServer.Forms.EditView;
using EPiServer.ServiceLocation;
using EPiServer.Shell.ObjectEditing;

namespace InsightFormFieldMapper.Impl
{
    public class InsightMappingMetaDataExtender : IMetadataExtender
    {
        internal Injected<EPiServer.Shell.Modules.ModuleTable> ModuleTable { get; set; }

        public void ModifyMetadata(ExtendedMetadata metadata, IEnumerable<Attribute> attributes)
        {
            if (typeof(IUIExternalFieldMapping).IsAssignableFrom(metadata.Model.GetOriginalType()))
            {
                foreach (var modelMetadata in metadata.Properties)
                {
                    var property = (ExtendedMetadata) modelMetadata;
                    if (property.PropertyName == "Forms_InsightProfileFieldMappings")
                    {
                        property.ClientEditingClass = "epi/shell/form/SuggestionSelectionEditor";
                        property.CustomEditorSettings["uiType"] = metadata.ClientEditingClass;
                        property.CustomEditorSettings["uiWrapperType"] = "flyout";
                        var format = ModuleTable.Service.ResolvePath("Shell", "stores/selectionquery/{0}/");
                        property.EditorConfiguration["storeurl"] = string.Format(CultureInfo.InvariantCulture, format,
                            typeof(ProfileStorePropertiesSelectionQuery).FullName);
                    }
                }
            }
        }
    }
}