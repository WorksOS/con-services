using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VSS.Hosted.VLCommon;

namespace VSS.Hosted.VLCommon
{
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
  public class AuthorizorAttribute : Attribute
  {
    public FeatureAppEnum FeatureApp { get; set; }
    public FeatureEnum Feature { get; set; }
    public FeatureChildEnum FeatureChild { get; set; }
    public FeatureAccessEnum FeatureAccess { get; set; }
    public FeatureChildEnum[] FeatureChildren { get; set; }
  }
}
