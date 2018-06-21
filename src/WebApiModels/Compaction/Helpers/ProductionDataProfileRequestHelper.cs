using Microsoft.Extensions.Logging;
using System;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Utilities;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Helpers
{
  /// <summary>
  /// The request representation for a linear or alignment based profile request for all thematic types other than summary volumes.
  /// Model represents a production data profile
  /// </summary>
  public class ProductionDataProfileRequestHelper : DataRequestBase, IProductionDataProfileRequestHelper
  {
    private FilterResult baseFilter;
    private FilterResult topFilter;
    private VolumeCalcType? volCalcType;
    private DesignDescriptor volumeDesign;

    public ProductionDataProfileRequestHelper()
    { }

    public ProductionDataProfileRequestHelper(ILoggerFactory logger, IConfigurationStore configurationStore,
      IFileListProxy fileListProxy, ICompactionSettingsManager settingsManager)
    {
      Log = logger.CreateLogger<ProductionDataProfileRequestHelper>();
      ConfigurationStore = configurationStore;
      FileListProxy = fileListProxy;
      SettingsManager = settingsManager;
    }

    public ProductionDataProfileRequestHelper SetVolumeCalcType(VolumeCalcType? calcType)
    {
      this.volCalcType = calcType;
      return this;
    }

    public ProductionDataProfileRequestHelper SetVolumeDesign(DesignDescriptor volumeDesign)
    {
      this.volumeDesign = volumeDesign;
      return this;
    }

    public ProductionDataProfileRequestHelper SetBaseFilter(FilterResult baseFilter)
    {
      this.baseFilter = baseFilter;
      return this;
    }

    public ProductionDataProfileRequestHelper SetTopFilter(FilterResult topFilter)
    {
      this.topFilter = topFilter;
      return this;
    }


    /// <summary>
    /// Creates an instance of the CompactionProfileProductionDataRequest class and populate it with data needed for a production data slice profile.   
    /// </summary>
    /// <param name="startLatDegrees"></param>
    /// <param name="startLonDegrees"></param>
    /// <param name="endLatDegrees"></param>
    /// <param name="endLonDegrees"></param>
    /// <returns>An instance of the CompactionProfileProductionDataRequest class.</returns>
    public CompactionProfileProductionDataRequest CreateProductionDataProfileRequest(double startLatDegrees, double startLonDegrees, double endLatDegrees, double endLonDegrees)
    {
      var llPoints = ProfileLLPoints.CreateProfileLLPoints(startLatDegrees.LatDegreesToRadians(), startLonDegrees.LonDegreesToRadians(), endLatDegrees.LatDegreesToRadians(), endLonDegrees.LonDegreesToRadians());

      var liftBuildSettings = SettingsManager.CompactionLiftBuildSettings(ProjectSettings);

      // callId is set to 'empty' because raptor will create and return a Guid if this is set to empty.
      // this would result in the acceptance tests failing to see the callID == in its equality test
      return CompactionProfileProductionDataRequest.CreateCompactionProfileProductionDataRequest(
        ProjectId,
        Guid.Empty,
        ProductionDataType.Height,
        Filter,
        null,
        null,
        null,
        llPoints,
        ValidationConstants3D.MIN_STATION,
        ValidationConstants3D.MIN_STATION,
        liftBuildSettings,
        false,
        DesignDescriptor,
        baseFilter,
        topFilter,
        volCalcType,
        volumeDesign);
    }
  }
}
