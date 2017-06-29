using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MasterDataProxies.Interfaces;
using Microsoft.AspNetCore.Mvc;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  public static class CompactionControllerExtensions
  {
    /// <summary>
    /// Gets the ids of the surveyed surfaces to exclude from Raptor calculations. 
    /// This is either the deactivated ones if includeSurveyedSurfaces is true or all surveyed surfaces if the flag is false.
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="fileListProxy">Proxy client to get list of imported files for the project</param>
    /// <param name="projectUid">The UID of the project containing the surveyed surfaces</param>
    /// <param name="includeSurveyedSurfaces">Flag indicating if surveyed surfaces should be included. Default is true.</param>
    /// <param name="customHeaders">Http request custom headers</param>
    /// <returns>The list of file ids for the surveyed surfaces to be excluded</returns>
    public static async Task<List<long>> GetExcludedSurveyedSurfaceIds(this Controller controller, IFileListProxy fileListProxy, Guid projectUid, bool? includeSurveyedSurfaces, IDictionary<string, string> customHeaders)
    {
      var fileList = await fileListProxy.GetFiles(projectUid.ToString(), customHeaders);
      if (fileList == null || fileList.Count == 0)
        return null;
      if (!includeSurveyedSurfaces.HasValue)
      {
        includeSurveyedSurfaces = true;
      }
      return fileList.Where(f => f.ImportedFileType == ImportedFileType.SurveyedSurface &&
                                 (!includeSurveyedSurfaces.Value || !f.IsActivated))
        .Select(f => f.LegacyFileId).ToList();
    }

    /// <summary>
    /// Gets the list of contributing machines from the query parameters
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="assetID">The asset ID</param>
    /// <param name="machineName">The machine name</param>
    /// <param name="isJohnDoe">The john doe flag</param>
    /// <returns>List of machines</returns>
    public static List<MachineDetails> GetMachines(this Controller controller, long? assetID, string machineName, bool? isJohnDoe)
    {
      MachineDetails machine = null;
      if (assetID.HasValue || !string.IsNullOrEmpty(machineName) || isJohnDoe.HasValue)
      {
        if (!assetID.HasValue || string.IsNullOrEmpty(machineName) || !isJohnDoe.HasValue)
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
              "If using a machine, asset ID machine name and john doe flag must be provided"));
        }
        machine = MachineDetails.CreateMachineDetails(assetID.Value, machineName, isJohnDoe.Value);
      }
      return machine == null ? null : new List<MachineDetails> { machine };
    }

    public static void ProcessStatusCode(this Controller controller, ServiceException se)
    {
      if (se.Code == HttpStatusCode.BadRequest &&
          se.GetResult.Code == ContractExecutionStatesEnum.FailedToGetResults)
      {
        se.Code = HttpStatusCode.NoContent;
      }
    }

  }
}
