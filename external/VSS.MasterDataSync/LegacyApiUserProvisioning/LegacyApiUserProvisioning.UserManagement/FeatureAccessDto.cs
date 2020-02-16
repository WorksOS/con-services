using LegacyApiUserProvisioning.UserManagement.Interfaces;

namespace LegacyApiUserProvisioning.UserManagement
{
    public class FeatureAccessDto : IFeatureAccess
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }
}