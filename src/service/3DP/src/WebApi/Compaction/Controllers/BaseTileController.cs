using System;
using System.Threading.Tasks;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Designs;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Productivity3D.WebApi.Models.Common;

namespace VSS.Productivity3D.WebApi.Compaction.Controllers
{
  /// <summary>
  /// Common base class for Tile based service controllers.
  /// </summary>
  public abstract class BaseTileController<T> : BaseController<T> where T : BaseController<T>
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    protected BaseTileController(IConfigurationStore configStore, IFileImportProxy fileImportProxy, ICompactionSettingsManager settingsManager)
    : base(configStore, fileImportProxy, settingsManager)
    { }

    /// <summary>
    /// Gets the summary volumes parameters according to the calculation type
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="volumeCalcType">The summary volumes calculation type</param>
    /// <param name="volumeBaseUid">Base Design or Filter UID for summary volumes determined by volumeCalcType</param>
    /// <param name="volumeTopUid">Top Design or Filter UID for summary volumes determined by volumeCalcType</param>
    /// <returns>Tuple of base filter, top filter and volume design descriptor</returns>
    protected async Task<Tuple<FilterResult, FilterResult, DesignDescriptor>> GetSummaryVolumesParameters(Guid projectUid, VolumeCalcType? volumeCalcType, Guid? volumeBaseUid, Guid? volumeTopUid)
    {
      Task<FilterResult> baseFilter = null;
      Task<FilterResult> topFilter = null;
      Task<DesignDescriptor> volumeDesign = null;

      if (volumeCalcType.HasValue)
      {
        switch (volumeCalcType.Value)
        {
          case VolumeCalcType.GroundToGround:
            baseFilter = GetCompactionFilter(projectUid, volumeBaseUid);
            topFilter = GetCompactionFilter(projectUid, volumeTopUid);

            await Task.WhenAll(baseFilter, topFilter);
            break;
          case VolumeCalcType.GroundToDesign:
            baseFilter = GetCompactionFilter(projectUid, volumeBaseUid);
            volumeDesign = GetAndValidateDesignDescriptor(projectUid, volumeTopUid, OperationType.Profiling);

            await Task.WhenAll(baseFilter, volumeDesign);
            break;
          case VolumeCalcType.DesignToGround:
            volumeDesign = GetAndValidateDesignDescriptor(projectUid, volumeBaseUid, OperationType.Profiling);
            topFilter = GetCompactionFilter(projectUid, volumeTopUid);

            await Task.WhenAll(volumeDesign, topFilter);
            break;
        }
      }

      return new Tuple<FilterResult, FilterResult, DesignDescriptor>(baseFilter?.Result, topFilter?.Result, volumeDesign?.Result);
    }
  }
}
