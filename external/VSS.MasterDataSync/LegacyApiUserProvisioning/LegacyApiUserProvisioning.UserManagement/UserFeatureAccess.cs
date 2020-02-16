using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.MasterDataSync.Models;

namespace LegacyApiUserProvisioning.UserManagement
{
    public class UserFeatureAccess
    {
        public FeatureAppEnum FeatureApp { get; set; }
        public FeatureEnum Feature { get; set; }
        public FeatureChildEnum FeatureChild { get; set; }
        public FeatureAccessEnum Access { get; set; }
    }
}