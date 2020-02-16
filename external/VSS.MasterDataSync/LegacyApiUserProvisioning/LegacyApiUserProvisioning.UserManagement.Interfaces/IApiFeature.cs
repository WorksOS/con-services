namespace LegacyApiUserProvisioning.UserManagement.Interfaces
{
    public interface IApiFeature
    {
        long Id { get; set; }
        string Name { get; set; }
        IFeatureAccess Access { get; set; }
    }
}