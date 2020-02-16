using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VSS.Hosted.VLCommon
{
  public class UserFeatureAccess
  {
    public FeatureAppEnum featureApp;
    public FeatureEnum feature;
    public FeatureChildEnum featureChild;
    public FeatureAccessEnum access;

    public static List<UserFeatureAccess> GetNHWebAdminFeatureAccess()
    {
      List<UserFeatureAccess> features = new List<UserFeatureAccess>();
      features.Add(new UserFeatureAccess()
      {
        access = FeatureAccessEnum.Full,        
        featureApp = FeatureAppEnum.NHWeb
      });

      return features;
    }
  }
}
