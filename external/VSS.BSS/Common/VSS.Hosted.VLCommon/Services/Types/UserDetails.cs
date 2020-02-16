using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Hosted.VLCommon.Services.Types
{
  public class UserDetails
  {
    public DateTime? EmailVerificationUTC;
     public DateTime? PasswordExpiredUTC;
    public bool IsActive;
    public List<UserFeatureAndAccess> UserFeatures;
    public List<CustomerDetails> CustomerList;
    public bool IsEmailVerified;
    public bool IsClientUser;
  }

  public class UserFeatureAndAccess
  {
    public string FeatureName;
    public string FeatureType;
    public string FeatureAccess;
  }

  public class CustomerDetails
  {
    public string CustomerName;
    public string UserName;
    public bool IsActive;
  }
}
