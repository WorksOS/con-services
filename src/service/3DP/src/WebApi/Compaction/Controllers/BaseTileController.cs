using System;
using System.Threading.Tasks;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;

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
    protected BaseTileController(IConfigurationStore configStore, IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager)
    : base(configStore, fileListProxy, settingsManager)
    { }

    /// <summary>
    /// Gets the summary volumes parameters according to the calcultion type
    /// </summary>
    /// <param name="projectUid">Project UID</param>
    /// <param name="volumeCalcType">The summary volumes calculation type</param>
    /// <param name="volumeBaseUid">Base Design or Filter UID for summary volumes determined by volumeCalcType</param>
    /// <param name="volumeTopUid">Top Design or Filter UID for summary volumes determined by volumeCalcType</param>
    /// <returns>Tuple of base filter, top filter and volume design descriptor</returns>
    protected async Task<Tuple<FilterResult, FilterResult, DesignDescriptor>> GetSummaryVolumesParameters(Guid projectUid, VolumeCalcType? volumeCalcType, Guid? volumeBaseUid, Guid? volumeTopUid)
    {
      FilterResult baseFilter = null;
      FilterResult topFilter = null;
      DesignDescriptor volumeDesign = null;

      if (volumeCalcType.HasValue)
      {
        switch (volumeCalcType.Value)
        {
          case VolumeCalcType.GroundToGround:
            baseFilter = await GetCompactionFilter(projectUid, volumeBaseUid);
            topFilter = await GetCompactionFilter(projectUid, volumeTopUid);
            break;
          case VolumeCalcType.GroundToDesign:
            baseFilter = await GetCompactionFilter(projectUid, volumeBaseUid);
            volumeDesign = await GetAndValidateDesignDescriptor(projectUid, volumeTopUid, true);
            break;
          case VolumeCalcType.DesignToGround:
            volumeDesign = await GetAndValidateDesignDescriptor(projectUid, volumeBaseUid, true);
            topFilter = await GetCompactionFilter(projectUid, volumeTopUid);
            break;
        }
      }

      return new Tuple<FilterResult, FilterResult, DesignDescriptor>(baseFilter, topFilter, volumeDesign);
    }
  }
}
