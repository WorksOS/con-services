using CCSS.WorksOS.Reports.Abstractions.Models.Request;
using Microsoft.AspNetCore.Http;
using VSS.MasterData.Models.Models;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class GenericComposerRequest
  {
    public string UserUid { get; set; }
    public string CustomerUID { get; set; }
    public IHeaderDictionary CustomHeaders { get; set; }
    public UserPreferenceData UserPreference { get; set; }
    public ReportRequest ReportRequest { get; set; }
  }
}
