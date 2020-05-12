using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.Logging;
using VSS.TRex.SiteModels.GridFabric.Requests;
using VSS.TRex.SiteModels.Interfaces;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/sitemodels")]
  public class SiteModelController : Controller
  {
    private static readonly ILogger Log = Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

    /// <summary>
    /// Returns project extents for a site model
    /// </summary>
    /// <param name="siteModelID">Grid to return status for</param>
    /// <returns></returns>
    [HttpGet("{siteModelID}/extents")]
    public JsonResult GetExtents(string siteModelID)
    {
      return new JsonResult(DIContext.Obtain<ISiteModels>().GetSiteModel(Guid.Parse(siteModelID))?.SiteModelExtent);
    }

    /// <summary>
    /// Returns project extents for a site model
    /// </summary>
    /// <param name="siteModelID">Grid to return status for</param>
    /// <returns></returns>
    [HttpGet("{siteModelID}/machinedesigns")]
    public JsonResult GetMachineDesigns(string siteModelID)
    {
      return new JsonResult(DIContext.Obtain<ISiteModels>().GetSiteModel(Guid.Parse(siteModelID))?.SiteModelMachineDesigns);
    }

    /// <summary>
    /// Returns proofing runs for a site model
    /// </summary>
    /// <param name="siteModelID">Grid to return status for</param>
    /// <returns></returns>
    [HttpGet("{siteModelID}/siteproofingruns")]
    public JsonResult GetSiteProofingRuns(string siteModelID)
    {
      return new JsonResult(DIContext.Obtain<ISiteModels>().GetSiteModel(Guid.Parse(siteModelID))?.SiteProofingRuns);
    }

    /// <summary>
    /// Returns project date range for a site model
    /// </summary>
    /// <param name="siteModelID">Grid to return status for</param>
    /// <returns></returns>
    [HttpGet("{siteModelID}/daterange")]
    public JsonResult GetDateRange(string siteModelID)
    {
      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(Guid.Parse(siteModelID));

      if (siteModel == null)
        return new JsonResult(new Tuple<DateTime, DateTime>(Common.Consts.MIN_DATETIME_AS_UTC, Common.Consts.MIN_DATETIME_AS_UTC));

      var startEndDates = siteModel.GetDateRange();

      return new JsonResult(new Tuple<DateTime, DateTime>(startEndDates.startUtc, startEndDates.endUtc));
    }

    /// <summary>
    /// Returns the number of subgrids present in the production data spatial existence map 
    /// </summary>
    /// <param name="siteModelID">Grid to return status for</param>
    /// <returns></returns>
    [HttpGet("{siteModelID}/existencemap/subgridcount")]
    public JsonResult GetSubGridCount(string siteModelID)
    {
      return new JsonResult(DIContext.Obtain<ISiteModels>().GetSiteModel(Guid.Parse(siteModelID))?.ExistenceMap.CountBits() ?? 0);
    }

    /// <summary>
    /// Deletes all information related to the project held in TRex. This operation is irreversible
    /// </summary>
    /// <param name="siteModelId">Grid to return status for</param>
    /// <returns></returns>
    [HttpDelete("{siteModelID}")]
    public async Task<JsonResult> DeleteProject(string siteModelID)
    {
      Guid.TryParse(siteModelID, out var siteModelUid);

      var deleter = new DeleteSiteModelRequest();
      var response = await deleter.ExecuteAsync(new DeleteSiteModelRequestArgument
      {
        ExternalDescriptor = Guid.NewGuid(), 
        ProjectID = siteModelUid
      });

      return new JsonResult(response);
    }

    /// <summary>
    /// Returns project extents for a site model
    /// </summary>
    /// <returns></returns>
    [HttpGet("metadata")]
    public JsonResult GetAllProjectsMetadata()
    {
      return new JsonResult(DIContext.Obtain<ISiteModelMetadataManager>().GetAll());
    }
  }
}
