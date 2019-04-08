using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Gateway.WebApi.ActionServices
{
  public interface IReportDataValidationUtility
  {
    /// <summary>
    /// Validates parameters for report data
    /// </summary>
    bool ValidateData(object request, ISiteModel siteModel);

  }
}
