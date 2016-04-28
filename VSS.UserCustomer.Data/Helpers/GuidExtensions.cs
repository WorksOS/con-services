using System;

namespace VSS.UserCustomer.Data.Helpers
{
  public static class GuidExtensions
  {
    public static string ToStringWithoutHyphens(this Guid meGuid)
    {
      return meGuid.ToString("N");
    }
  }
}
