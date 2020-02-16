using System;

namespace VSS.Hosted.VLCommon
{
  [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
  public class SnapshotAttribute : Attribute
  {
  }
}
