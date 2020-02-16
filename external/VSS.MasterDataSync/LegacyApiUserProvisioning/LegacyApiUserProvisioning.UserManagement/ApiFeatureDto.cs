using LegacyApiUserProvisioning.UserManagement.Interfaces;
using VSS.Hosted.VLCommon;

namespace LegacyApiUserProvisioning.UserManagement
{
    public class ApiFeatureDto : IApiFeature
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public IFeatureAccess Access { get; set; }
    }
}