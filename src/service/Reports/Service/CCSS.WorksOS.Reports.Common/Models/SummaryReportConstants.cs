namespace CCSS.WorksOS.Reports.Common.Models
{
  public class SummaryReportConstants
  {
    public const string ReportElevationParameter = "reportElevation";
    public const string ReportCmvParameter = "reportCmv";
    public const string ReportMdpParameter = "reportMdp";
    public const string ReportPassCountParameter = "reportPassCount";
    public const string ReportTemperatureParameter = "reportTemperature";
    public const string ReportCutFillParameter = "reportCutFill";
    //For dictionary of values
    public static readonly string[] Keys =
    {
      ReportElevationParameter, ReportCutFillParameter, ReportCmvParameter, ReportMdpParameter, ReportPassCountParameter, ReportTemperatureParameter
    };

    public const string FilterUid = "filterUid";
    public const string BaseUid = "baseUid";
    public const string TopUid = "topUid";
    public const string VolumeCalculationType = "volumeCalcType";
  }
}
