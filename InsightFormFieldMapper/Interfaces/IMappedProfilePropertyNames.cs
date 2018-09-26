using System.Collections.Generic;

namespace InsightFormFieldMapper.Interfaces
{
    /// <summary>
    /// Exposes property names that are used in the Episerver Profile Store
    /// </summary>
    public interface IMappedProfilePropertyNames
    {
        /// <summary>
        /// Core profile properties
        /// </summary>
        List<string> CorePropertyNames { get; }

        /// <summary>
        /// Core information property names
        /// </summary>
        List<string> InfoPropertyNames { get; }

        /// <summary>
        /// Used if we want to allow custom payload properties to be selectable in the UI
        /// </summary>
        List<string> CustomPayloadPropertyNames { get; set; }

        /// <summary>
        /// Mapped properties
        /// </summary>
        List<string> MappedPropertyNames { get; }
    }
}