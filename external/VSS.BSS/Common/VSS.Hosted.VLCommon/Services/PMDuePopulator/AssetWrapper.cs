using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;

using VSS.Hosted.VLCommon;


using log4net;

namespace VSS.Hosted.VLCommon.PMDuePopulator
{ 
  public class AssetWrapper
  {
    private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
    public long assetID;
    public List<PMIntervalWrapper> pmIntervals;
    public List<PMIntervalInstanceWrapper> pmIntervalInstances;
    public List<PMCompletedInstance> pmCompletedInstances;

    public AssetWrapper(long _assetID, INH_OP ctx, List<PMCompletedInstance> clearedInstances, bool isResetIntervalsToDefault)
    {
      assetID = _assetID;

      GetAllIntervals();

      log.IfInfoFormat("Asset({0} Loaded PMIntervals Count - {1}", _assetID, pmIntervals.Count);

      GetAllIntervalInstances(ctx);

      log.IfInfoFormat("Asset({0} Loaded PMIntervalInstances Count - {1}", _assetID, pmIntervalInstances.Count);

      pmCompletedInstances = API.PMDuePopulator.GetAllPMCompletedInstancesPerAsset(ctx, assetID, isResetIntervalsToDefault);

      log.IfInfoFormat("Asset({0} Loaded PMCompleted Count - {1}", _assetID, pmCompletedInstances.Count);

      foreach (var item in pmIntervals)
      {
        item.assetW = this;
        item.clearedInstances = clearedInstances;
        item.Init(ctx);
      }
    }

    private void GetAllIntervals()
    {
      var allIntervals = PMGetAllintervalsAccess.GetAllIntervals(assetID);

      pmIntervals = (from pmi in allIntervals
                     select new PMIntervalWrapper()
                     {
                       thisInterval = pmi
                     }
                     ).ToList();
    }

    private void GetAllIntervalInstances(INH_OP ctx)
    {
      var pmIntervalInstancesRaw = (from pii in ctx.PMIntervalInstanceReadOnly
                                    where pii.fk_AssetID == assetID
                                    select pii).ToList();

      pmIntervalInstances = (from pmii in pmIntervalInstancesRaw
                             join pmi in pmIntervals on pmii.fk_PMIntervalID equals pmi.thisInterval.ID
                             select new PMIntervalInstanceWrapper()
                             {
                               thisInstance = pmii,
                               trackingType = pmi.thisInterval.TrackingTypeID
                             }).ToList();
    }

    public void SetAllFutureInstances(long? userID, INH_OP ctx, int resetModifiedRuntimehours)
    {
        foreach (var thisIntervalW in pmIntervals.OrderByDescending(k => k.thisInterval.Rank))
        {
            thisIntervalW.SetNextInstance(pmIntervals); 
        }        

      // save future instances bulk by calling stored proc
      PMIntervalInstanceSaver.Save(pmIntervalInstances.Select(f => new
      {
        fk_AssetID = f.thisInstance.fk_AssetID,
        fk_PMIntervalID = f.thisInstance.fk_PMIntervalID,
        RuntimeHours = f.thisInstance.RuntimeHours,
        OdometerMiles = f.thisInstance.OdometerMiles,
        InstanceType = f.thisInstance.InstanceType,
        ifk_UserID = f.thisInstance.ifk_UserID
      }).CopyToDataTable(), new List<long>{assetID}, resetModifiedRuntimehours, ctx);
    }    
  }
}
