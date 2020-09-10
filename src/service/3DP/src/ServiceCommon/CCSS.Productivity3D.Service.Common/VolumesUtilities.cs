using System;
using System.Threading.Tasks;
using CCSS.Productivity3D.Service.Common.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Cache.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Filter.Abstractions.Interfaces;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces;

namespace CCSS.Productivity3D.Service.Common
{
  public class VolumesUtilities
  {
    /// <summary>
    /// Gets the summary volumes parameters according to the calculation type
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="volumeCalcType">The summary volumes calculation type</param>
    /// <param name="volumeBaseUid">Base Design or Filter UID for summary volumes determined by volumeCalcType</param>
    /// <param name="volumeTopUid">Top Design or Filter UID for summary volumes determined by volumeCalcType</param>
    /// <returns>Tuple of base filter, top filter and volume design descriptor</returns>
    public static async Task<Tuple<FilterResult, FilterResult, DesignDescriptor>> GetSummaryVolumesParameters(
      Guid projectUid, VolumeCalcType? volumeCalcType, Guid? volumeBaseUid, Guid? volumeTopUid, string userUid, 
      IHeaderDictionary customHeaders, IFileImportProxy fileImportProxy, IConfigurationStore configStore, ILogger log,
      IFilterServiceProxy filterServiceProxy, IDataCache filterCache, string projectTimeZone)
    {
      Task<FilterResult> baseFilter = null;
      Task<FilterResult> topFilter = null;
      Task<DesignDescriptor> volumeDesign = null;

      if (volumeCalcType.HasValue)
      {
        switch (volumeCalcType.Value)
        {
          case VolumeCalcType.GroundToGround:
            baseFilter = FilterUtilities.GetCompactionFilter(projectUid, projectTimeZone, userUid, volumeBaseUid, 
              filterCache, customHeaders, log, filterServiceProxy, fileImportProxy, configStore);
            topFilter = FilterUtilities.GetCompactionFilter(projectUid, projectTimeZone, userUid, volumeTopUid,
              filterCache, customHeaders, log, filterServiceProxy, fileImportProxy, configStore);

            await Task.WhenAll(baseFilter, topFilter);
            break;
          case VolumeCalcType.GroundToDesign:
            baseFilter = FilterUtilities.GetCompactionFilter(projectUid, projectTimeZone, userUid, volumeBaseUid,
              filterCache, customHeaders, log, filterServiceProxy, fileImportProxy, configStore);
            volumeDesign = DesignUtilities.GetAndValidateDesignDescriptor(projectUid, volumeTopUid, userUid, customHeaders, fileImportProxy, configStore, log, OperationType.Profiling);

            await Task.WhenAll(baseFilter, volumeDesign);
            break;
          case VolumeCalcType.DesignToGround:
            volumeDesign = DesignUtilities.GetAndValidateDesignDescriptor(projectUid, volumeTopUid, userUid, customHeaders, fileImportProxy, configStore, log, OperationType.Profiling);
            topFilter = FilterUtilities.GetCompactionFilter(projectUid, projectTimeZone, userUid, volumeTopUid,
              filterCache, customHeaders, log, filterServiceProxy, fileImportProxy, configStore);

            await Task.WhenAll(volumeDesign, topFilter);
            break;
        }
      }

      return new Tuple<FilterResult, FilterResult, DesignDescriptor>(baseFilter?.Result, topFilter?.Result, volumeDesign?.Result);
    }
  }
}
