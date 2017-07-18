using System;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Utilities;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models
{
  /// <summary>
  ///   Represents abstract container for all request executors. Uses abstract factory pattern to seperate executor logic
  ///   from
  ///   controller logic for testability and possible executor versioning.
  /// </summary>
  public class Subscriptions

  {
    public string assetUId { get; set; }
    public string projectUid { get; set; }
    public string customerUid { get; set; }
    public int serviceTypeId { get; set; }
    public int startKeyDate { get; set; }
    public int endKeyDate { get; set; }


    public Subscriptions(string assetUId, string projectUid, string customerUid, int serviceTypeId, DateTime? startKeyDate, DateTime? endKeyDate)
    {
      this.assetUId = assetUId;
      this.projectUid = projectUid;
      this.customerUid = customerUid;
      this.serviceTypeId = serviceTypeId;
      this.startKeyDate = startKeyDate == null ? DateTimeExtensions.KeyDate(DateTime.MinValue) : DateTimeExtensions.KeyDate(startKeyDate.Value);
      this.endKeyDate = endKeyDate == null ? DateTimeExtensions.NullKeyDate : DateTimeExtensions.KeyDate(endKeyDate.Value);
    }
  }
}