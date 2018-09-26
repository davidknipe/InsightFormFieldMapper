using System.Collections.Generic;
using EPiServer.ServiceLocation;
using InsightFormFieldMapper.Interfaces;

namespace InsightFormFieldMapper.Impl
{
    /// <inheritdoc />
    [ServiceConfiguration(typeof(IMappedProfilePropertyNames), Lifecycle = ServiceInstanceScope.Singleton)]
    public class MappedProfilePropertyNames : IMappedProfilePropertyNames
    {
        public const string FirstNameKey = "[first name] part of Name";
        public const string LastNameKey = "[last name] part of Name";

        public MappedProfilePropertyNames()
        {
            CustomPayloadPropertyNames = new List<string>();
        }

        /// <inheritdoc />
        public List<string> CorePropertyNames { get; } = new List<string>()
        {
            "Name",
            "Manager"
        };

        /// <inheritdoc />
        public List<string> InfoPropertyNames { get; } = new List<string>()
        {
            "Email",
            "Picture",
            "Website",
            "StreetAddress",
            "Phone",
            "Mobile",
            "City",
            "State",
            "ZipCode",
            "JobTitle",
            "Company",
            "Country"
        };

        /// <inheritdoc />
        public List<string> CustomPayloadPropertyNames { get; set; }

        /// <inheritdoc />
        public List<string> MappedPropertyNames { get; } = new List<string>()
        {
            FirstNameKey,
            LastNameKey
        };
    }
}