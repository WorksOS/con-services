using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.Hosted.VLCommon.Bss
{
  public class SummaryHelper
  {
    public static string GetServiceViewSummary(IList<ServiceViewInfoDto> serviceViews)
    {
      var sb = new StringBuilder();
      foreach (var serviceView in serviceViews)
      {
        sb.AppendFormat(
          "ID: {0} Type: {1} Active: {2} to {3} for AssetID: {4} SN: {5} for Customer ID: {6} Name: {7}.{8}",
          serviceView.ServiceViewId,
          serviceView.ServiceTypeName,
          serviceView.StartDateKey,
          serviceView.EndDateKey,
          serviceView.AssetId,
          serviceView.AssetSerialNumber,
          serviceView.CustomerId,
          serviceView.CustomerName,
          Environment.NewLine);
      }
      // The string returned from this method is used in other string.Format calls, so any curly braces need to be escaped.
      return sb.ToString().Replace("{", "{{").Replace("}", "}}");
    }
  }
}