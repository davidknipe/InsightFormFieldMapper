using System.Collections.Generic;
using System.Web;
using EPiServer;
using EPiServer.Core;
using EPiServer.Forms.Core;
using EPiServer.Forms.Core.Data;
using EPiServer.Forms.Core.PostSubmissionActor;
using EPiServer.Forms.Helpers.Internal;
using EPiServer.Forms.Implementation.Elements;
using EPiServer.ServiceLocation;
using InsightFormFieldMapper.Impl;
using InsightFormFieldMapper.Init;
using InsightFormFieldMapper.Interfaces;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace InsightFormFieldMapper.Actors
{
    public class UpsertInsightProfileActor : PostSubmissionActorBase
    {
        private string resourceGetProfile = "api/v1.0/Profiles";
        private string resourceUpdateProfile = "api/v1.0/Profiles/{id}";

        private Injected<IProfileStoreConfig> _profileStoreConfig;
        private Injected<IContentRepository> _contentRepo;
        private Injected<IFormDataRepository> _formDataRepository;
        private Injected<IMappedProfilePropertyNames> _mappedProfilePropertyNames;

        public override bool IsSyncedWithSubmissionProcess => false;

        public override bool PerformOnEveryStepSubmission => false;

        public override object Run(object input)
        {
            if (!_profileStoreConfig.Service.IsConfigured()) return null;

            var deviceId = GetDeviceId(HttpRequestContext);
            if (deviceId == null) return null;

            var formElementMapping = GetMappedProfileFields(FormIdentity.GetFormBlock());
            var friendlyResults =
                _formDataRepository.Service.TransformSubmissionDataWithFriendlyName(SubmissionData.Data,
                    SubmissionFriendlyNameInfos, true);

            var firstName = string.Empty;
            var lastName = string.Empty;
            var updateProfile = false;
            JToken currentProfile = null;
            foreach (var formField in friendlyResults)
            {
                if (formElementMapping.ContainsKey(formField.Key) 
                    && formElementMapping[formField.Key] == MappedProfilePropertyNames.FirstNameKey)
                {
                    firstName = formField.Value.ToString();
                }
                else if (formElementMapping.ContainsKey(formField.Key)
                         && formElementMapping[formField.Key] == MappedProfilePropertyNames.LastNameKey)
                {
                    lastName = formField.Value.ToString();
                }
                else if (formElementMapping.ContainsKey(formField.Key))
                {
                    if (currentProfile == null) currentProfile = GetProfile(deviceId);
                    updateProfile = true;
                    SetProfileValue(currentProfile, formElementMapping[formField.Key], formField.Value.ToString());
                }
            }

            if (firstName != string.Empty || lastName != string.Empty)
            {
                if (currentProfile == null) currentProfile = GetProfile(deviceId);
                updateProfile = true;
                SetProfileValue(currentProfile, "Name", firstName + " " + lastName);
            }

            if (updateProfile && currentProfile != null)
            {
                return UpdateProfile(currentProfile);
            }

            return "Nothing to update";
        }

        private Dictionary<string, string> GetMappedProfileFields(FormContainerBlock formContainerBlock)
        {
            var results = new Dictionary<string, string>();

            if (formContainerBlock?.ElementsArea?.Items != null && (formContainerBlock.ElementsArea.Items.Count != 0))
            {
                var allElements = formContainerBlock.ElementsArea.Items;
                foreach (var formElement in allElements)
                {
                    var formItem = _contentRepo.Service.Get<ElementBlockBase>(formElement.ContentLink,
                        (formContainerBlock as ILocale)?.Language);

                    if (formItem?.Property[FormToInsightMappingInit.InsightProfileMappingPropertyName]?.Value != null)
                    {
                        var insightPropertyName = formItem
                            .Property[FormToInsightMappingInit.InsightProfileMappingPropertyName].Value.ToString();
                        if (!string.IsNullOrEmpty(insightPropertyName))
                        {
                            var formContent = formItem as IContent;
                            results.Add(formContent.Name, insightPropertyName);
                        }
                    }
                }
            }

            return results;
        }

        private JToken GetProfile(string deviceId)
        {
            // Set up the request
            var client = new RestClient(_profileStoreConfig.Service.RootApiUrl);
            var request = new RestRequest(resourceGetProfile, Method.GET);
            request.AddHeader("Ocp-Apim-Subscription-Key", _profileStoreConfig.Service.SubscriptionKey);

            // Filter the profiles based on the current device id
            request.AddParameter("$filter", "DeviceIds eq " + deviceId);

            // Execute the request to get the profile
            var getProfileResponse = client.Execute(request);
            var getProfileContent = getProfileResponse.Content;

            // Get the results as a JArray object
            var profileResponseObject = JObject.Parse(getProfileContent);
            var profileArray = (JArray) profileResponseObject["items"];

            // Expecting an array of profiles with one item in it
            var profileObject = profileArray.First;

            return profileObject;
        }

        private IRestResponse UpdateProfile(JToken profileObject)
        {
            // Set up the update profile request
            var client = new RestClient(_profileStoreConfig.Service.RootApiUrl);
            var profileUpdateRequest = new RestRequest(resourceUpdateProfile, Method.PUT);
            profileUpdateRequest.AddHeader("Ocp-Apim-Subscription-Key", _profileStoreConfig.Service.SubscriptionKey);
            profileUpdateRequest.AddUrlSegment("id", profileObject["ProfileId"]);

            // Populate the body to update the profile
            var updateBody = profileObject.ToString();
            profileUpdateRequest.AddParameter("application/json", updateBody, ParameterType.RequestBody);

            // PUT the update request to the API
            var updateResponse = client.Execute(profileUpdateRequest);

            return updateResponse;
        }

        private void SetProfileValue(JToken currentProfile, string key, string value)
        {
            if (_mappedProfilePropertyNames.Service.CorePropertyNames.Contains(key))
            {
                currentProfile[key] = value;
            }
            else if (_mappedProfilePropertyNames.Service.InfoPropertyNames.Contains(key))
            {
                currentProfile["Info"][key] = value;

            }
            else
            {
                if (!currentProfile["Payload"].HasValues)
                {
                    currentProfile["Payload"] = new JObject();
                }
                currentProfile["Payload"][key] = value;
            }
        }

        public JObject SetPropertyContent(JObject source, string name, object content)
        {
            var prop = source.Property(name);

            if (prop == null)
            {
                prop = new JProperty(name, content);

                source.Add(prop);
            }
            else
            {
                prop.Value = JToken.FromObject(content);
            }

            return source;
        }

        private string GetDeviceId(HttpRequestBase httpRequestContext)
        {
            return httpRequestContext.Cookies["_madid"]?.Value;
        }
    }
}