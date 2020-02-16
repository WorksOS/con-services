using Newtonsoft.Json;

namespace LegacyApiUserProvisioning.UserManagement.Interfaces
{
    public interface IBaseResponse
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        string Error { get; }
    }
}