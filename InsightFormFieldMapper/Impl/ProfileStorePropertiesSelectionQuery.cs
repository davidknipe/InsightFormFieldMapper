using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.ServiceLocation;
using EPiServer.Shell.ObjectEditing;
using InsightFormFieldMapper.Interfaces;

namespace InsightFormFieldMapper.Impl
{
    [ServiceConfiguration(typeof(ISelectionQuery))]
    public class ProfileStorePropertiesSelectionQuery : ISelectionQuery
    {
        private readonly IList<SelectItem> _items;

        public ProfileStorePropertiesSelectionQuery(IMappedProfilePropertyNames mappedProfilePropertyNames)
        {
            var mappedProfilePropertyNames1 = mappedProfilePropertyNames;
            _items = new List<SelectItem>();

            mappedProfilePropertyNames1.CorePropertyNames.ForEach(x => _items.Add(new SelectItem() { Text = x, Value = x }));
            mappedProfilePropertyNames1.InfoPropertyNames.ForEach(x => _items.Add(new SelectItem() { Text = x, Value = x }));
            mappedProfilePropertyNames1.CustomPayloadPropertyNames.ForEach(x => _items.Add(new SelectItem() { Text = x, Value = x }));
        }

        public IEnumerable<ISelectItem> GetItems(string query) => _items.Where(i => i.Text.StartsWith(query, StringComparison.OrdinalIgnoreCase));

        public ISelectItem GetItemByValue(string value) => _items.FirstOrDefault(i => i.Value.Equals(value));
    }
}