using System;
using System.Collections.Generic;
using System.Linq;
using VSS.Hosted.VLCommon.PMDuePopulator;



namespace VSS.Hosted.VLCommon
{

  internal class MaintenanceAPI : IMaintenanceAPI
  {
    private readonly int DefaultExpectedWeeklyMileage = 1000;

    /// <summary>
    /// This method saves the interval with its associated checklist and parts, and associates the interval with the specified
    /// asset.
    /// 
    /// If this is the first-time edit of an interval (via any asset), it will be transitioned from a factory Default
    /// interval, to a Custom interval. This method does not support edits to factory Default intervals. Instead, the factory
    /// Default is cloned to a new custom interval, which is stored with a reference back to the original Default interval.
    /// 
    /// The save cascades down through the child objects of the interval (the checklists of the interval,
    /// and for each checklist, the parts list). At all levels, factory Defaults are not editable, so these child objects
    /// are cloned also.
    /// 
    /// The interval being saved, may include custom checklist steps. This child collection of custom checklist steps
    /// replaces the existing custom checklist steps. Each checklist step, has a child collection of parts, which
    /// replaces the existing step's parts list. Three generations of fun.
    /// 
    /// In the case where a new custom interval has been created to replace a default, the default steps are not
    /// cloned into the custom interval, seeing as they are accessible via the default interval.
    /// 
    /// (Current intervals/steps/parts are not shared between assets, so updates do not require cloning custom
    /// intervals/steps/parts of other assets)
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="assetID"></param>
    /// <param name="interval"></param>
    /// <returns></returns>
    public long SaveAssetInterval(INH_OP ctx, long assetID, PMInterval interval, PMTrackingTypeEnum trackingType,
                                  bool callPMAPIForAllMCIntervals = true)
    {
      long savedIntervalID = interval.ID;

      PMInterval customInterval = null;
      PMInterval existingInterval = null;
      if (interval.ID > -1)
      {
        var a = (from ia in ctx.PMIntervalAssetReadOnly
                 where ia.fk_AssetID == assetID && ia.fk_DefaultPMIntervalID == interval.ID
                 select ia).FirstOrDefault();

        if (a != null)
        {
          interval.ID = a.fk_PMIntervalID;
          savedIntervalID = a.fk_PMIntervalID;
          //interval.IsCustom = true;
        }
        existingInterval = (from pmi in ctx.PMInterval
                            where pmi.ID == interval.ID
                            select pmi).FirstOrDefault();

        //if (existingInterval != null && existingInterval.IsDeleted)
        //  throw new InvalidOperationException("Invalid interval.");
      }

      //If a default interval has been changed or it's a completely new custom interval then 
      //create a new custom interval otherwise update existing custom interval
      if (existingInterval != null && existingInterval.IsCustom)
      {
        existingInterval.Title = interval.Title;
        existingInterval.Description = interval.Description;
        existingInterval.ifk_PMTrackingTypeID = (int)trackingType;
        existingInterval.TrackingValueHoursFirst = interval.TrackingValueHoursFirst;
        existingInterval.TrackingValueHoursNext = interval.TrackingValueHoursNext;
        existingInterval.TrackingValueMilesFirst = interval.TrackingValueMilesFirst;
        existingInterval.TrackingValueMilesNext = interval.TrackingValueMilesNext;
        //existingInterval.IsCustom = existingInterval.IsCustom; you can't edit a custom to make it a Default.
        existingInterval.IsMetric = interval.IsMetric;
        existingInterval.IsCumulative = interval.IsCumulative;
        existingInterval.IsDeleted = interval.IsDeleted;
        existingInterval.Rank = interval.Rank;
        existingInterval.UpdateUTC = DateTime.UtcNow;
        existingInterval.IsTrackingEnabled = interval.IsTrackingEnabled;
        existingInterval.IsMajorComponent = interval.IsMajorComponent;
      }
      else
      {
        customInterval = new PMInterval
                           {
                             Title = interval.Title,
                             Description = interval.Description,
                             ifk_PMTrackingTypeID = (int)trackingType,
                             TrackingValueHoursFirst = interval.TrackingValueHoursFirst,
                             TrackingValueHoursNext = interval.TrackingValueHoursNext,
                             TrackingValueMilesFirst = interval.TrackingValueMilesFirst,
                             TrackingValueMilesNext = interval.TrackingValueMilesNext,
                             IsDeleted = interval.IsDeleted,
                             IsCustom = true,
                             UpdateUTC = DateTime.UtcNow,
                             IsMetric = interval.IsMetric,
                             IsCumulative = interval.IsCumulative,
                             Rank = interval.Rank,
                             IsTrackingEnabled = interval.IsTrackingEnabled,
                             IsMajorComponent = interval.IsMajorComponent
                           };
        customInterval.fk_PMSalesModelID = API.Equipment.GetOemPMSalesModelID(ctx, assetID);

        //Set up association from the interval to the asset 
        PMIntervalAsset intervalAsset = new PMIntervalAsset
                                          {
                                            fk_AssetID = assetID,
                                            CustomPMInterval = customInterval,
                                            fk_DefaultPMIntervalID =
                                              (existingInterval != null && !existingInterval.IsCustom
                                                 ? existingInterval.ID
                                                 : (long?)null)
                                          };

        ctx.PMIntervalAsset.AddObject(intervalAsset);
      }

      ctx.SaveChanges();
      if (null != customInterval)
        savedIntervalID = customInterval.ID;

      // Add/Update custom checklist step collection of the interval
      List<long> savedStepIDs = new List<long>();
       

      if (callPMAPIForAllMCIntervals)
      {
        API.PMDuePopulator.PopulatePMDue(ctx, assetID, null);
        FactPMDueAccess.UpdateAssets(new List<long> { assetID });
      }
      return savedIntervalID;
    }

   

    public bool SetAssetIntervalDynamicUpdate(INH_OP ctx, long assetID, bool isDynamicUpdate)
    {
      return true;
    }

    public PMIntervalWithNextDueInfo GetNextDueInterval(INH_OP opCtx, long assetID, double currentRuntimeHours,
                                                        double currentOdometerMiles,
                                                        List<PMIntervalWithNextDueInfo> allIntervals)
    {
      allIntervals = allIntervals.OrderBy(f => f.NextIntervalByTrackingType).ToList();

      PMIntervalWithNextDueInfo nextCumulativeIntervalDue = GetNextCumulativeIntervalDue(currentRuntimeHours,
                                                                                         currentOdometerMiles,
                                                                                         allIntervals);

      PMIntervalWithNextDueInfo nextIncrementalIntervalDue = GetNextIncrementalIntervalDue(allIntervals);

      if (nextCumulativeIntervalDue == null)
      {
        return nextIncrementalIntervalDue;
      }

      if (nextIncrementalIntervalDue == null)
      {
        return nextCumulativeIntervalDue;
      }

      if (nextIncrementalIntervalDue.NextIntervalDueUTC == nextCumulativeIntervalDue.NextIntervalDueUTC)
      //this means zero expected hours and or mileage so use DueInByTrackingType for comparison
      {
        if (nextCumulativeIntervalDue.TrackingType != nextIncrementalIntervalDue.TrackingType)
        {
          return nextCumulativeIntervalDue;
        }
        if (nextCumulativeIntervalDue.DueInByTrackingType.Equals(double.NaN))
        {
          return nextCumulativeIntervalDue;
        }
        if (nextCumulativeIntervalDue.DueInByTrackingType <= nextIncrementalIntervalDue.DueInByTrackingType)
        {
          return nextCumulativeIntervalDue;
        }
        return nextIncrementalIntervalDue;
      }

      if (nextCumulativeIntervalDue.NextIntervalDueUTC < nextIncrementalIntervalDue.NextIntervalDueUTC)
      {
        return nextCumulativeIntervalDue;
      }
      return nextIncrementalIntervalDue;
    }

    public List<PMCompletedService> GetCompletedService(INH_OP ctx, long assetID)
    {
      return (from cs in ctx.PMCompletedServiceReadOnly.Include("PMInterval")
              where cs.fk_AssetID.Value == assetID
                    && cs.Visible
              select cs).ToList();
    }

    private PMIntervalWithNextDueInfo GetNextCumulativeIntervalDue(
      double currentRuntimeHours,
      double currentOdometerMiles,
      List<PMIntervalWithNextDueInfo> allIntervalsWithNextDueInfo)
    {
      List<PMIntervalWithNextDueInfo> allCumulativeIntervalsWithNextDueInfo =
        (from interval in allIntervalsWithNextDueInfo
         where interval.Interval.IsCumulative
         select interval)
          .ToList();

      PMIntervalWithNextDueInfo nextCumulativeIntervalDue = null;
      PMIntervalWithNextDueInfo greatestOverdueInterval = null;

      if (allCumulativeIntervalsWithNextDueInfo != null && allCumulativeIntervalsWithNextDueInfo.Count > 0)
      {
        nextCumulativeIntervalDue = allCumulativeIntervalsWithNextDueInfo
          .OrderBy(f => f.DueAtByTrackingType)
          // this works for cumulatives because they are required to all have the same tracking type
          .First();

        switch (nextCumulativeIntervalDue.Interval.ifk_PMTrackingTypeID)
        {
          case (int)PMTrackingTypeEnum.RuntimeHours:
            greatestOverdueInterval = allCumulativeIntervalsWithNextDueInfo
              .Where(f => f.DueAtHours <= currentRuntimeHours)
              .OrderByDescending(f => f.Interval.Rank)
              .FirstOrDefault();
            break;
          case (int)PMTrackingTypeEnum.Mileage:
          default:
            greatestOverdueInterval = allCumulativeIntervalsWithNextDueInfo
              .Where(f => f.DueAtMiles <= currentOdometerMiles)
              .OrderByDescending(f => f.Interval.Rank)
              .FirstOrDefault();
            break;
        }
      }

      if (greatestOverdueInterval != null)
      {
        nextCumulativeIntervalDue = greatestOverdueInterval;
      }

      return nextCumulativeIntervalDue;
    }

    private PMIntervalWithNextDueInfo GetNextIncrementalIntervalDue(
      List<PMIntervalWithNextDueInfo> allIntervalsWithNextDueInfo)
    {
      bool hasNullNextIntervalDueUTC = (from interval in allIntervalsWithNextDueInfo
                                        where !interval.Interval.IsCumulative
                                              && interval.NextIntervalDueUTC.KeyDate() == DateTime.MaxValue.KeyDate()
                                        select interval).Count() > 0;

      if (hasNullNextIntervalDueUTC)
      {
        return (from interval in allIntervalsWithNextDueInfo
                where !interval.Interval.IsCumulative
                select interval)
          .OrderBy(f => f.DueInByTrackingType).ThenBy(f => f.DueAtByTrackingType).ThenBy(
            f => f.Interval.IsMajorComponent)
          .FirstOrDefault();
      }

      return (from interval in allIntervalsWithNextDueInfo
              where !interval.Interval.IsCumulative
              select interval)
        .OrderBy(f => f.NextIntervalDueUTC).ThenBy(f => f.DueInByTrackingType).ThenBy(f => f.Interval.IsMajorComponent)
        .FirstOrDefault();
    }

    public PMIntervalWithNextDueInfo GetGreatestCumulativeIntervalWithinXPercentOfBeingDue(INH_OP ctx,
                                                                                           long assetID,
                                                                                           List
                                                                                             <PMIntervalWithNextDueInfo>
                                                                                             allIntervalsWithNextDueHours,
                                                                                           PMInterval
                                                                                             overdueCumulativeInterval,
                                                                                           double currentRuntimeHours,
                                                                                           int thresholdPercent)
    {
      return (from intervals in allIntervalsWithNextDueHours
              where intervals.Interval.IsCumulative
                    && intervals.Interval.Rank > overdueCumulativeInterval.Rank
                    && intervals.PercentDue >= 100 - thresholdPercent
              orderby intervals.Interval.Rank descending
              select intervals).FirstOrDefault();
    }

    private long CreateCompletedService(
      INH_OP ctx,
      long assetID,
      long intervalID,
      long userID,
      double runtimeHours,
      double odometerMiles,
      string performedBy,
      string intervalTitle,
      string serviceNotes,
      DateTime completedServiceDate,
      PMServiceCompletionTypeEnum completionType,
      bool visible = true)
    {
      PMCompletedService pmCompletedService = new PMCompletedService
                                                {
                                                  ServiceDate = completedServiceDate,
                                                  RuntimeHours = runtimeHours,
                                                  OdometerMiles = odometerMiles,
                                                  PerformedBy = performedBy,
                                                  IntervalTitle = intervalTitle,
                                                  UpdateUTC = DateTime.UtcNow,
                                                  Visible = visible
                                                };
      ctx.PMCompletedService.AddObject(pmCompletedService);

      pmCompletedService.PMInterval = (from i in ctx.PMInterval
                                       where i.ID == intervalID
                                       select i).FirstOrDefault();

      User user = (from u in ctx.User
                   where u.ID == userID
                   select u).FirstOrDefault();

      pmCompletedService.fk_UserID = user.ID;
      pmCompletedService.fk_AssetID = assetID;

      if (serviceNotes != null && !serviceNotes.Trim().Equals(""))
        pmCompletedService.ServiceNotes = serviceNotes;
      byte completionID = (byte)completionType;

      pmCompletedService.fk_PMServiceCompletionTypeID = completionID;

      ctx.SaveChanges();

      return pmCompletedService.ID;
    }


    public double? GetExpectedWeeklyMileage(INH_OP ctx, long assetID)
    {
      return (from a in ctx.AssetReadOnly
              where a.AssetID == assetID
              select a.ExpectedWeeklyMileage).FirstOrDefault();
    }

    private static void SetMostRecentCompletedServiceForAllIntervals(List<PMCompletedService> completedService,
                                                                     List<PMIntervalWithNextDueInfo>
                                                                       allIntervalsWithNextDueHours)
    {
      PMCompletedService mostRecentCompletedService = (from service in completedService
                                                       where
                                                         service.fk_PMServiceCompletionTypeID ==
                                                         (int)PMServiceCompletionTypeEnum.Completed
                                                       orderby service.ServiceDate descending
                                                       select service).FirstOrDefault();

      foreach (PMIntervalWithNextDueInfo interval in allIntervalsWithNextDueHours)
      {
        if (mostRecentCompletedService != null)
        {
          interval.LastServiceCompleted = new PMCompletedInstance()
                                            {
                                              ID = mostRecentCompletedService.ID,
                                              AssetID = mostRecentCompletedService.fk_AssetID.Value,
                                              IntervalTitle = mostRecentCompletedService.IntervalTitle,
                                              ServiceDate = mostRecentCompletedService.ServiceDate,
                                              RuntimeHours = mostRecentCompletedService.RuntimeHours,
                                              OdometerMiles = mostRecentCompletedService.OdometerMiles,
                                              ServiceCompletionType =
                                                mostRecentCompletedService.fk_PMServiceCompletionTypeID,
                                              PMIntervalID = mostRecentCompletedService.fk_PMIntervalID
                                            };
        }
      }
    }

    private void CalculateNextDueDate(List<PMIntervalWithNextDueInfo> allIntervalsWithNextDueHours,
                                      ExpectedRuntimeHours expectedRuntimeHours, double? expectedWeeklyMileage)
    {
      DateTime now = DateTime.UtcNow;

      foreach (PMIntervalWithNextDueInfo pmIntervalWithNextDueHours in allIntervalsWithNextDueHours)
      {
        DateTime? nextDueAtUTC = null;
        switch ((PMTrackingTypeEnum)pmIntervalWithNextDueHours.Interval.ifk_PMTrackingTypeID)
        {
          case PMTrackingTypeEnum.RuntimeHours:
            if (!IsExpectedRuntimeZero(expectedRuntimeHours))
            {
              nextDueAtUTC = CalculateNextDueWithRuntime(expectedRuntimeHours, now, pmIntervalWithNextDueHours);
            }
            break;
          case PMTrackingTypeEnum.Mileage:
            if (expectedWeeklyMileage > 0)
            {
              nextDueAtUTC = CalculateNextDueWithMiles(pmIntervalWithNextDueHours, now, expectedWeeklyMileage);
            }
            break;
        }

        pmIntervalWithNextDueHours.NextIntervalDueUTC = nextDueAtUTC ?? DateTime.MaxValue;
      }
    }

    private DateTime? CalculateNextDueWithMiles(PMIntervalWithNextDueInfo pmIntervalWithNextDueHours,
                                                DateTime? nextDueAtUTC, double? expectedWeeklyMileage)
    {
      double dailyMileage = 0;
      if (!expectedWeeklyMileage.HasValue || expectedWeeklyMileage <= 0)
        expectedWeeklyMileage = DefaultExpectedWeeklyMileage;

      dailyMileage = expectedWeeklyMileage.Value / 7.0;
      double dueIn = pmIntervalWithNextDueHours.DueInMiles / dailyMileage;
      if ((int)dueIn != dueIn && dueIn > 0)
        dueIn++;
      else if ((int)dueIn != dueIn && dueIn < 0)
        dueIn--;

      nextDueAtUTC = nextDueAtUTC.Value.AddDays((int)dueIn);

      return nextDueAtUTC;
    }

    private static DateTime? CalculateNextDueWithRuntime(ExpectedRuntimeHours expectedRuntimeHours, DateTime now,
                                                         PMIntervalWithNextDueInfo pmIntervalWithNextDueHours)
    {
      DateTime nextDueAtUTC = now;
      double dueInHours = pmIntervalWithNextDueHours.DueInHours;
      bool isOverdue = false;
      int totalDays = 0;
      int currentDayOfWeek = (int)now.DayOfWeek;
      if (pmIntervalWithNextDueHours.DueInHours <= 0)
      {
        dueInHours = dueInHours * -1;
        isOverdue = true;
      }

      while (dueInHours > 0)
      {
        if (currentDayOfWeek > 6) currentDayOfWeek = 0;
        switch (currentDayOfWeek)
        {
          case (int)DayOfWeek.Sunday:
            dueInHours -= expectedRuntimeHours.Sun;
            break;
          case (int)DayOfWeek.Monday:
            dueInHours -= expectedRuntimeHours.Mon;
            break;
          case (int)DayOfWeek.Tuesday:
            dueInHours -= expectedRuntimeHours.Tue;
            break;
          case (int)DayOfWeek.Wednesday:
            dueInHours -= expectedRuntimeHours.Wed;
            break;
          case (int)DayOfWeek.Thursday:
            dueInHours -= expectedRuntimeHours.Thu;
            break;
          case (int)DayOfWeek.Friday:
            dueInHours -= expectedRuntimeHours.Fri;
            break;
          case (int)DayOfWeek.Saturday:
            dueInHours -= expectedRuntimeHours.Sat;
            break;
        }

        currentDayOfWeek++;
        totalDays++;
      }

      // it's possible, though unrealistic, to set up a scenario where an interval is due past 12/31/9999.  If this happens, just set to max (or min).
      try
      {
        nextDueAtUTC = nextDueAtUTC.AddDays(isOverdue ? -totalDays : totalDays);
      }
      catch (ArgumentOutOfRangeException)
      {
        nextDueAtUTC = isOverdue ? DateTime.MinValue : DateTime.MaxValue;
      }

      return nextDueAtUTC;
    }

    private bool IsExpectedRuntimeZero(ExpectedRuntimeHours expectedRuntimeHours)
    {
      return !(expectedRuntimeHours != null &&
               (expectedRuntimeHours.Mon > 0 ||
                expectedRuntimeHours.Tue > 0 ||
                expectedRuntimeHours.Wed > 0 ||
                expectedRuntimeHours.Thu > 0 ||
                expectedRuntimeHours.Fri > 0 ||
                expectedRuntimeHours.Sat > 0 ||
                expectedRuntimeHours.Sun > 0));
    }

    public List<int> GetSupportedDeviceTypesForManualMaintenance()
    {
      List<int> result = new List<int>();
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>(true))
      {
        var featureSetAppFeatures = (from dtype in opCtx.DeviceTypeReadOnly
                                     join afs in opCtx.AppFeatureSetReadOnly on dtype.fk_AppFeatureSetID equals afs.ID
                                     join afsap in opCtx.AppFeatureSetAppFeatureReadOnly on afs.ID equals afsap.fk_AppFeatureSetID
                                     where afsap.fk_AppFeatureID == (int)AppFeatureEnum.UpdateLocation
                                     select dtype.ID
                                     ).ToList();
        result = featureSetAppFeatures;
      }

      return result;

    }

    // US 8711 - Copy Intervals to other assets
    public bool CopyIntervalToAsset(INH_OP ctx, long sourceAssetID, long targetAssetID, bool copyChecklistAndParts,
                                    bool copyServiceLevelIntervals, bool copyIndependentIntervals,
                                    bool copyMajorComponentIntervals)
    {
      try
      {
        PMCopyIntervalAccess.CopyIntervals(sourceAssetID, targetAssetID, copyChecklistAndParts,
                                            copyServiceLevelIntervals, copyIndependentIntervals,
                                            copyMajorComponentIntervals);
      }
      catch (Exception)
      {
        throw new InvalidOperationException(string.Format("Error Copying Intervals to Target Asset"),
                                            new IntentionallyThrownException());
      }
      return true;
    }

    public List<string> GetParentAccounts(long customerID)
    {
      List<string> parentNames;
      using (INH_OP opCtx = ObjectContextFactory.NewNHContext<INH_OP>())
      {
        Customer currentCustomer = (from c in opCtx.CustomerReadOnly
                                    join ct in opCtx.CustomerTypeReadOnly on c.fk_CustomerTypeID equals ct.ID
                                    where c.ID == customerID
                                    select c).FirstOrDefault();

        parentNames = AccountLookup.GetParentAccountNames(customerID).ToList();
        if (currentCustomer.fk_CustomerTypeID == (int)CustomerTypeEnum.Customer)
        {
          var accountIDs = (from relationships in opCtx.CustomerRelationshipReadOnly
                            where relationships.fk_ParentCustomerID == currentCustomer.ID
                            select relationships.fk_ClientCustomerID).ToList();

          foreach (var accountID in accountIDs)
          {
            parentNames.AddRange(AccountLookup.GetParentAccountNames(accountID).ToList());
          }
        }
        parentNames.Add(currentCustomer.Name);
      }
      return (from name in parentNames select name).Distinct().ToList<string>();
    }

    public bool SavePMModifiedInterval(long? userId, long assetID, long intervalId, int modifiedRuntimeHours)
    {
      int modifiedResult = 0;
      using (INH_OP ctx = ObjectContextFactory.NewNHContext<INH_OP>())
      {

        var pmModifiedInterval = (from pmIntervalInstance in ctx.PMIntervalInstance
                                  where pmIntervalInstance.fk_AssetID == assetID
                                  && pmIntervalInstance.fk_PMIntervalID == intervalId
                                  select pmIntervalInstance).FirstOrDefault();

        if (pmModifiedInterval != null)
        {
          pmModifiedInterval.ModifiedRuntimeHours = (double)modifiedRuntimeHours;
          pmModifiedInterval.UpdateUTC = DateTime.UtcNow;
          pmModifiedInterval.ifk_UserID = userId;

          modifiedResult = ctx.SaveChanges();

          if (modifiedResult <= 0)
          {
            throw new InvalidOperationException(string.Format("Error updating modified PMInterval for the asset {0} and interval {1}",
                                                              assetID.ToString(), intervalId.ToString()));
          }
          else
          {
            // For updating the NH_RPT..FactPMDue table
            FactPMDueAccess.UpdateAssets(new List<long> { assetID });
            return true;
          }
        }
        else
        {
          throw new InvalidOperationException("PM Interval does not exists");
        }
      }
    }

    public bool StartTrackingMaintenance(SessionContext session, long assetID, List<PMInterval> intervals, double runtimeHours)
    {
      try
      {
        List<PMCompletedInstance> pmClearedInstances = new List<PMCompletedInstance>();

        //Clearing maintenance interval backlogs for incremental intervals i.e. Independant Intervals and Major Components
        List<PMInterval> incrementalIntervals = intervals.Where(m => m.IsCumulative == false && m.ifk_PMTrackingTypeID == 0).ToList(); //supports only hour-based
        if (incrementalIntervals.Count > 0)
          pmClearedInstances = StartTrackingMaintenanceForIncrementalIntervals(session, assetID, runtimeHours, pmClearedInstances, incrementalIntervals);

        //Clearing maintenance interval backlogs for cumulative intervals
        List<PMInterval> cumulativeIntervals = intervals.Where(m => m.IsCumulative == true && m.ifk_PMTrackingTypeID == 0).ToList(); //supports only hour-based      
        if (cumulativeIntervals.Count > 0)
          StartTrackingMaintenanceForCumulativeIntervals(session, assetID, runtimeHours, pmClearedInstances, cumulativeIntervals);

        SetStartTrackingMaintenance(session, assetID, pmClearedInstances);

        FactPMDueAccess.UpdateAssets(new List<long> { assetID });
      }
      catch (Exception)
      {
        throw new InvalidOperationException(string.Format("Error in Start Tracking Maintenance"),
                                            new IntentionallyThrownException());
      }
      return true;
    }

    #region Privates

    private void SetStartTrackingMaintenance(SessionContext session, long assetID, List<PMCompletedInstance> pmClearedInstances)
    {
      //Delete the existing cleared services of that asset from PMCompletedService
      List<PMCompletedService> pmCompletedServices = (from pii in session.NHOpContext.PMCompletedService
                                                      where pii.fk_AssetID == assetID
                                                      && pii.fk_PMServiceCompletionTypeID == (int)PMServiceCompletionTypeEnum.Cleared
                                                      select pii).ToList();
      foreach (var completedService in pmCompletedServices)
        session.NHOpContext.PMCompletedService.DeleteObject(completedService);

      session.NHOpContext.SaveChanges();

      //Insert all the cleared service levels, independant intervals and major components
      foreach (var clearedInstance in pmClearedInstances)
      {
        CreateCompletedService(session.NHOpContext, assetID, clearedInstance.PMIntervalID, session.UserID.Value, clearedInstance.RuntimeHours, 0,
          session.UserName, clearedInstance.IntervalTitle, null, DateTime.UtcNow, PMServiceCompletionTypeEnum.Cleared, false);
      }
    }

    private void StartTrackingMaintenanceForCumulativeIntervals(SessionContext session, long assetID, double runtimeHours, List<PMCompletedInstance> pmClearedInstances, List<PMInterval> cumulativeIntervals)
    {
      double lastClearedServiceHours = 0;
      List<long> intervalIDs = (from interval in cumulativeIntervals select interval.ID).ToList();
      foreach (var cumulativeInterval in cumulativeIntervals.OrderByDescending(f => f.Rank))
      {
        if (runtimeHours > cumulativeInterval.TrackingValueHoursFirst)
        {
          double temp = 0;
          temp = runtimeHours % cumulativeInterval.TrackingValueHoursFirst == 0 ? (runtimeHours / cumulativeInterval.TrackingValueHoursFirst) - 1 : temp = (runtimeHours / cumulativeInterval.TrackingValueHoursFirst);
          lastClearedServiceHours = Convert.ToInt32(((Math.Truncate(temp)) * cumulativeInterval.TrackingValueHoursFirst));

          if (lastClearedServiceHours > 0)
          {
            pmClearedInstances.Add(new PMCompletedInstance()
            {
              TrackingType = cumulativeInterval.ifk_PMTrackingTypeID,
              RuntimeHours = lastClearedServiceHours,
              Rank = cumulativeInterval.Rank,
              PMIntervalID = cumulativeInterval.ID,
              IntervalTitle = cumulativeInterval.Title
            });
            API.PMDuePopulator.PopulatePMDue(session.NHOpContext, assetID, session.UserID, 0, pmClearedInstances);
            break;
          }
        }
      }

      while (true)
      {
        List<PMIntervalInstance> pmIntervalInstances = (from pii in session.NHOpContext.PMIntervalInstanceReadOnly
                                                        where intervalIDs.Contains(pii.fk_PMIntervalID)
                                                        && pii.fk_AssetID == assetID
                                                        select pii).ToList();
        if (pmIntervalInstances != null)
        {
          PMCompletedInstance pmClearedInstance = GetLastCompletedServiceWithRank(session.NHOpContext, pmIntervalInstances, cumulativeIntervals, runtimeHours);
          if (pmClearedInstance != null)
          {
            pmClearedInstances.Add(pmClearedInstance);
            API.PMDuePopulator.PopulatePMDue(session.NHOpContext, assetID, session.UserID, 0, pmClearedInstances);
          }
          else
            break;
        }
      }

      if (lastClearedServiceHours == 0) //Reset the PMIntervalInstance to default if no servicelevels are cleared
        PMIntervalInstanceResetToDefault(session, assetID, cumulativeIntervals);

      List<PMIntervalInstance> pmIntervalInstance = (from pmii in session.NHOpContext.PMIntervalInstance
                                                     join pmi in session.NHOpContext.PMInterval
                                                     on pmii.fk_PMIntervalID equals pmi.ID
                                                     where intervalIDs.Contains(pmii.fk_PMIntervalID) &&
                                                     pmii.fk_AssetID == assetID &&
                                                     pmi.IsCumulative == true
                                                     orderby pmi.Rank
                                                     select pmii).ToList();
      for(var i = 0; i < pmIntervalInstance.Count; i ++)
      {        
        if (pmIntervalInstance[i].ModifiedRuntimeHours.HasValue && runtimeHours > pmIntervalInstance[i].ModifiedRuntimeHours)
          UpdatePMIntervalInstance(session, assetID, pmIntervalInstance[i]);
      }
    }

    private void UpdatePMIntervalInstance(SessionContext session, long assetID, PMIntervalInstance pmIntervalInstance)
    {
      int modifiedResult;
      pmIntervalInstance.ModifiedRuntimeHours = null;
      pmIntervalInstance.ifk_UserID = session.UserID;
      pmIntervalInstance.UpdateUTC = DateTime.UtcNow;

      modifiedResult = session.NHOpContext.SaveChanges();
      if (modifiedResult <= 0)
      {
        throw new InvalidOperationException(string.Format("Error updating PMInterval for the asset {0} and interval {1}",
                                                            assetID.ToString(), pmIntervalInstance.fk_PMIntervalID));
      }
    }

    private void PMIntervalInstanceResetToDefault(SessionContext session, long assetID, List<PMInterval> PMIntervals)
    {
      foreach (var interval in PMIntervals)
      {
        var pmIntervalInstance = (from pii in session.NHOpContext.PMIntervalInstance
                                  where pii.fk_PMIntervalID == interval.ID
                                  && pii.fk_AssetID == assetID 
                                  select pii).FirstOrDefault();

        if (pmIntervalInstance != null)
        {
          pmIntervalInstance.RuntimeHours = interval.TrackingValueHoursFirst;
          pmIntervalInstance.UpdateUTC = DateTime.Now;
          session.NHOpContext.SaveChanges();
        }
      }
    }

    private List<PMCompletedInstance> StartTrackingMaintenanceForIncrementalIntervals(SessionContext session, long assetID, double runtimeHours, List<PMCompletedInstance> pmClearedInstances, List<PMInterval> incrementalIntervals)
    {
      double lastCompletedServiceHours = 0;
      foreach (var incrementalInterval in incrementalIntervals)
      {
        if (runtimeHours <= incrementalInterval.TrackingValueHoursFirst)
          lastCompletedServiceHours = 0;
        else if (runtimeHours > incrementalInterval.TrackingValueHoursFirst && runtimeHours <= incrementalInterval.TrackingValueHoursNext)
          lastCompletedServiceHours = incrementalInterval.TrackingValueHoursFirst;
        else
          lastCompletedServiceHours = CalculateLastCompletedService(runtimeHours, incrementalInterval.TrackingValueHoursNext, 0, incrementalInterval.TrackingValueHoursFirst);

        if (lastCompletedServiceHours > 0)
          pmClearedInstances.Add(new PMCompletedInstance()
            {
              TrackingType = incrementalInterval.ifk_PMTrackingTypeID,
              RuntimeHours = lastCompletedServiceHours,
              Rank = 0,
              PMIntervalID = incrementalInterval.ID,
              IntervalTitle = incrementalInterval.Title
            });
      }
      if (pmClearedInstances != null && pmClearedInstances.Count > 0)
        API.PMDuePopulator.PopulatePMDue(session.NHOpContext, assetID, session.UserID, 0, pmClearedInstances);

      if (lastCompletedServiceHours == 0 && pmClearedInstances.Count == 0) //Reset the PMIntervalInstance to default if no incremental intervals are cleared
        PMIntervalInstanceResetToDefault(session, assetID, incrementalIntervals);

      return pmClearedInstances;
    }

    private double FormatWithRoundingOrNegativeOne(double? numberToFormat)
    {
      return (int)Math.Round(numberToFormat ?? -1);
    }

    private List<PMInterval> GetVLDefaultPMIntervals(INH_OP ctx)
    {
      return (from pmi in ctx.PMIntervalReadOnly
              where pmi.fk_PMSalesModelID == EquipmentAPI.DEFAULT_SALES_MODEL_ID
                && !pmi.IsCustom && pmi.CompCode != null
              select pmi).ToList();
    }

    private string GetIntervalTitle(INH_OP ctx, long intervalID)
    {
      string intervalTitle = (from interval in ctx.PMIntervalReadOnly
                              where interval.ID == intervalID
                              select interval.Title).FirstOrDefault();

      if (intervalTitle == string.Empty)
        throw new InvalidOperationException("Interval not found.");

      return intervalTitle;
    }

    private double GetValueByTrackingType(PMTrackingTypeEnum trackingType, double runtimeHours, double odometerMiles)
    {
      switch (trackingType)
      {
        case PMTrackingTypeEnum.RuntimeHours:
          return runtimeHours;
        case PMTrackingTypeEnum.Mileage:
          return odometerMiles;
      }
      return 0;
    }

    private PMCompletedInstance GetLastCompletedServiceWithRank(INH_OP ctx, List<PMIntervalInstance> pmIntervalInstances, List<PMInterval> cumulativeIntervals, double runtimeHours)
    {
      foreach (var cumulativeInterval in cumulativeIntervals.OrderByDescending(f => f.Rank))
      {
        var trackingValueInHours = (from i in pmIntervalInstances where i.fk_PMIntervalID == cumulativeInterval.ID select i.RuntimeHours).SingleOrDefault();
        if (trackingValueInHours != null)
        {
          if (trackingValueInHours.HasValue && trackingValueInHours < runtimeHours)
          {
            PMCompletedInstance pmClearedInstance = new PMCompletedInstance();
            pmClearedInstance.PMIntervalID = cumulativeInterval.ID;
            pmClearedInstance.IntervalTitle = cumulativeInterval.Title;
            pmClearedInstance.RuntimeHours = trackingValueInHours.Value;
            pmClearedInstance.Rank = cumulativeInterval.Rank;
            return pmClearedInstance;
          }
          else
            continue;
        }
      }
      return null;
    }

    private double CalculateLastCompletedService(double runtimeHours, double nextFrequency, double previous, double next)
    {
      double lastCompletedServiceHours;
      if (runtimeHours > next)
      {
        previous = next;
        next += nextFrequency;
        lastCompletedServiceHours = CalculateLastCompletedService(runtimeHours, nextFrequency, previous, next);
      }
      else
        lastCompletedServiceHours = previous;

      return lastCompletedServiceHours;
    }

    #endregion
  }
}
