using System.Runtime.Serialization;

namespace LegacyApiUserProvisioning.UserManagement
{
    [DataContract]
    public enum FeatureAppEnum
    {
        [EnumMember]
        NHAdmin = 1000,
        [EnumMember]
        NHWeb = 2000,
        [EnumMember]
        DataServices = 3000,
        [EnumMember]
        VLAdmin = 6000
    }
}