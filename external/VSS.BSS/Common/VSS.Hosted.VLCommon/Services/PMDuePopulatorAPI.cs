using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;

using VSS.Hosted.VLCommon;


using VSS.Hosted.VLCommon.PMDuePopulator;
using log4net;

namespace VSS.Hosted.VLCommon
{
  internal class PMDuePopulatorAPI : IPMDuePopulatorAPI
  {
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);

    #region PopulateIntervalInstanceAPI
    
    public void PopulatePMDue(INH_OP ctx, long assetID, long? userID = null, int resetModifiedRuntimehours = 0, List<PMCompletedInstance> clearedInstances = null, bool isResetIntervalsToDefault = false)
    {      
      AssetWrapper oneAsset = new AssetWrapper(assetID, ctx, clearedInstances, isResetIntervalsToDefault);
      oneAsset.SetAllFutureInstances(userID, ctx, resetModifiedRuntimehours);
    }
    #endregion

    #region HelperMethods for PMPopulateAPI
    public List<PMCompletedInstance> GetAllPMCompletedInstancesPerAsset(INH_OP ctx, long assetID, bool isResetIntervalsToDefault=false)
    {
      List<int> serviceCompletionTypes;

      if(isResetIntervalsToDefault)
        serviceCompletionTypes = new List<int> { (int)PMServiceCompletionTypeEnum.Completed, (int)PMServiceCompletionTypeEnum.Skipped};
      else
        serviceCompletionTypes = new List<int> { (int)PMServiceCompletionTypeEnum.Completed, (int)PMServiceCompletionTypeEnum.Skipped, (int) PMServiceCompletionTypeEnum.Cleared};

      List<PMCompletedInstance> pmCompletedInstances = (from cs in ctx.PMCompletedServiceReadOnly
                                                        join pi in ctx.PMIntervalReadOnly on cs.fk_PMIntervalID equals pi.ID
                                                        from pia in ctx.PMIntervalAssetReadOnly.Where(f => f.fk_AssetID == assetID && f.fk_DefaultPMIntervalID == cs.fk_PMIntervalID).Select(f => f.fk_PMIntervalID).DefaultIfEmpty()
                                                        where cs.fk_AssetID == assetID
                                                        && serviceCompletionTypes.Contains(cs.fk_PMServiceCompletionTypeID)
                                                        && (((cs.fk_PMServiceCompletionTypeID == (int)PMServiceCompletionTypeEnum.Completed || cs.fk_PMServiceCompletionTypeID == (int)PMServiceCompletionTypeEnum.Skipped) && cs.Visible)
                                                        || (cs.fk_PMServiceCompletionTypeID == (int)PMServiceCompletionTypeEnum.Cleared && cs.Visible == false))
                                                        && (!pi.IsDeleted || (pi.IsDeleted && !pi.IsCustom && pi.IsCumulative))
                                                        select new PMCompletedInstance()
                                                        {
                                                          AssetID = assetID,
                                                          DefaultIntervalID = cs.fk_PMIntervalID,
                                                          PMIntervalAssetIntervalID = pia,
                                                          OdometerMiles = cs.OdometerMiles,
                                                          RuntimeHours = cs.RuntimeHours,
                                                          ServiceDate = cs.ServiceDate,
                                                          ServiceCompletionType = cs.fk_PMServiceCompletionTypeID,
                                                          TrackingType = pi.ifk_PMTrackingTypeID,
                                                          IsCumulative = pi.IsCumulative,
                                                          Rank = pi.Rank,
                                                          ID = cs.ID,
                                                          IntervalTitle = cs.IntervalTitle,
                                                          UpdatedUTC = cs.UpdateUTC
                                                        }).ToList();

      return pmCompletedInstances;
    } 
    #endregion
    
    #region FromPMTopics
    public bool CallPMPopulatorForAssets(INH_OP ctx, List<PMSalesModel> salesModels)
    {

      List<long> salesModelIDs = (from sm in salesModels
                                  where sm.ExternalID != null
                                  select sm.ID).Distinct().ToList();

      var assets = (from a in ctx.AssetReadOnly
                    from sm in ctx.PMSalesModelReadOnly
                    where salesModelIDs.Contains(sm.ID)
                    && (a.SerialNumberVIN.Substring(0,3) == sm.SerialNumberPrefix
                      || (sm.Model != null && a.fk_MakeCode == sm.fk_MakeCode && a.Model == sm.Model))
                    select new
                    {
                      AssetID = a.AssetID,
                      assetPrefix = a.SerialNumberVIN.Substring(0, 3),
                      assetSuffix = a.SerialNumberVIN.Substring(3),
                      make = a.fk_MakeCode,
                      model = a.Model
                    }).ToList();

      List<long> assetsToProcess = (from sm in salesModels
                                    from a in assets                                
                                      where sm.ExternalID != null
                                          && ((sm.SerialNumberPrefix == a.assetPrefix &&
                                               ParseSerialNumberSuffix(a.assetSuffix) >= sm.StartRange.Value
                                               && ParseSerialNumberSuffix(a.assetSuffix) <= sm.EndRange.Value)
                                            || (sm.Model != null && a.make == sm.fk_MakeCode && a.model == sm.Model))
                                      select a.AssetID).ToList();

      foreach (long assetID in assetsToProcess)
      {
        PopulatePMDue(ctx, assetID, null);
      }
      return true;
    }

    private static int ParseSerialNumberSuffix(string serialNumberSuffix)
    {
      int assetSuffix = 0;
      int.TryParse(serialNumberSuffix, out assetSuffix);
      return assetSuffix;
    }
    #endregion
  }
}
