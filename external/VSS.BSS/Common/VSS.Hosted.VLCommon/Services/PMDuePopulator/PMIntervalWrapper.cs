using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;

using VSS.Hosted.VLCommon;


using log4net;

namespace VSS.Hosted.VLCommon.PMDuePopulator
{
    public class PMIntervalWrapper
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodInfo.GetCurrentMethod().DeclaringType);
        public AssetWrapper assetW;
        public PMGetAllintervalsAccess.AssetPMInterval thisInterval;
        public PMIntervalInstanceWrapper firstFutureInstance;
        public List<PMCompletedInstance> clearedInstances;
        private double frequency;
        private double lastCompletedService;        
        
        public PMIntervalWrapper()
        {
        }

        public void Init(INH_OP ctx)
        {
            GetFutureInstances();

            if (firstFutureInstance == null)
            {
                log.IfInfoFormat("Asset({0} Future instance added for - {1}", assetW.assetID, thisInterval.Title);
                DateTime utcNow = DateTime.UtcNow;
                firstFutureInstance = new PMIntervalInstanceWrapper(new PMIntervalInstance(), thisInterval.TrackingTypeID);
                firstFutureInstance.thisInstance.fk_AssetID = assetW.assetID;
                firstFutureInstance.thisInstance.fk_PMIntervalID = thisInterval.ID;
                firstFutureInstance.thisInstance.ifk_UserID = null;
                firstFutureInstance.thisInstance.InstanceType = (int)PMIntervalInstanceTypeEnum.Current;
                firstFutureInstance.thisInstance.UpdateUTC = utcNow;

                assetW.pmIntervalInstances.Add(firstFutureInstance);
            }
            
            if(clearedInstances != null && clearedInstances.Count > 0)
              GetLastClearedService();
            else
              GetLastCompletedService();

            frequency = (lastCompletedService == 0 || thisInterval.IsCumulative) ? thisInterval.TrackingValueMilesOrHoursFirst.Value : thisInterval.TrackingValueMilesOrHoursNext.Value;
            firstFutureInstance.trackingValue = lastCompletedService;
        }

        private void GetFutureInstances()
        {
            if (assetW.pmIntervalInstances != null)
            {
                firstFutureInstance = (from inst in assetW.pmIntervalInstances
                                       where inst.thisInstance.fk_PMIntervalID == thisInterval.ID
                                       && inst.thisInstance.InstanceType == (int)PMIntervalInstanceTypeEnum.Current
                                       orderby inst.trackingValue
                                       select inst).FirstOrDefault();
            }
        }

        private void GetLastCompletedService()
        {
            if (thisInterval.IsCumulative == true)
            {
                // Last completed service of this or higher ranked interval
                if (assetW.pmCompletedInstances != null)
                {
                    lastCompletedService = (from pmii in assetW.pmCompletedInstances
                                            where pmii.IsCumulative == true
                                            && pmii.TrackingType == thisInterval.TrackingTypeID
                                            && pmii.Rank >= thisInterval.Rank
                                            orderby pmii.TrackingValue descending
                                            select pmii.TrackingValue).FirstOrDefault();
                }
            }
            else
            {
                if (assetW.pmCompletedInstances != null)
                {
                    lastCompletedService = (from pmii in assetW.pmCompletedInstances
                                            where pmii.TrackingType == thisInterval.TrackingTypeID
                                            && pmii.PMIntervalID == thisInterval.ID
                                            && pmii.IsCumulative == false
                                            orderby pmii.TrackingValue descending
                                            select pmii.TrackingValue).FirstOrDefault();
                }
            }
            log.IfInfoFormat("Asset({0} Last completed service was done at - {1} for {2}", assetW.assetID, lastCompletedService, thisInterval.Title);
        }

        private void GetLastClearedService()
        {
          if (thisInterval.IsCumulative == true)
            lastCompletedService = (from pmii in clearedInstances
                                    where pmii.Rank >= thisInterval.Rank
                                    orderby pmii.TrackingValue descending
                                    select pmii.TrackingValue).FirstOrDefault();          
          else
            lastCompletedService = (from pmii in clearedInstances
                                    where pmii.PMIntervalID == thisInterval.ID
                                    orderby pmii.TrackingValue descending
                                    select pmii.TrackingValue).FirstOrDefault();            
        }

        public void SetNextInstance(List<PMIntervalWrapper> pmIntervals)
        {
            IncrementTrackingValue();

            if (thisInterval.IsCumulative == true)
                CheckHigherRankInstance(pmIntervals);
            log.IfInfoFormat("Asset({0} Next schedule for - {1} is at {2}", assetW.assetID, thisInterval.Title, firstFutureInstance.trackingValue);

        }


        private void IncrementTrackingValue()
        {
            firstFutureInstance.trackingValue += frequency;
        }

        private double RecentCompletedInterval()
        {
            double RecentIntervalRuntime;
            var RecentInterval = (from pmii in assetW.pmCompletedInstances
                                  where pmii.IsCumulative == true
                                  && pmii.TrackingType == thisInterval.TrackingTypeID
                                  orderby pmii.UpdatedUTC descending
                                  select pmii).FirstOrDefault();
            return RecentIntervalRuntime = RecentInterval!=null ? RecentInterval.RuntimeHours:0.0;

        }
        private void CheckHigherRankInstance(List<PMIntervalWrapper> pmIntervals)
        {
            bool isRescheduleRequired=false;
            var pmIntervalLst = pmIntervals.Where(x => x.thisInterval.IsCumulative == true).OrderBy(k => k.thisInterval.Rank).ToList();           

            var higherInstance = (from pmii in assetW.pmIntervalInstances
                                  join pmi in assetW.pmIntervals on pmii.thisInstance.fk_PMIntervalID equals pmi.thisInterval.ID
                                  where pmi.thisInterval.IsCumulative
                                  && pmii.thisInstance.InstanceType == (int)PMIntervalInstanceTypeEnum.Current
                                  && pmii.trackingValue <= firstFutureInstance.trackingValue
                                  && pmi.thisInterval.Rank > thisInterval.Rank
                                  select pmii).FirstOrDefault();

            if (pmIntervalLst.Count > 2)
            {
                double recentCompletedInterval = pmIntervalLst.First().RecentCompletedInterval();
                var leastFrequency = pmIntervalLst.First().thisInterval.TrackingValueMilesOrHoursFirst;
                var leastRankInterval = pmIntervalLst.First().firstFutureInstance.trackingValue;
                var nexttoLeastRankInterval = pmIntervalLst.Skip(1).First().firstFutureInstance.trackingValue;
                var highestRankInterval = pmIntervalLst.Last().firstFutureInstance.trackingValue;
                isRescheduleRequired = ((leastRankInterval + leastFrequency) >= highestRankInterval && leastRankInterval >= nexttoLeastRankInterval  && recentCompletedInterval <= highestRankInterval) ? true : false;               

                if (isRescheduleRequired)
                {
                    higherInstance = (from pmii in assetW.pmIntervalInstances
                                      join pmi in assetW.pmIntervals on pmii.thisInstance.fk_PMIntervalID equals pmi.thisInterval.ID
                                      where pmi.thisInterval.IsCumulative
                                      && pmii.thisInstance.InstanceType == (int)PMIntervalInstanceTypeEnum.Current
                                      && pmii.trackingValue == highestRankInterval
                                      && pmi.thisInterval.Rank > thisInterval.Rank
                                      select pmii).FirstOrDefault();
                }
            }         


            if (higherInstance != null)
            {
                double calculatedNextTrackingValue = higherInstance.trackingValue + frequency;
                if (calculatedNextTrackingValue > firstFutureInstance.trackingValue)
                {
                    firstFutureInstance.trackingValue = calculatedNextTrackingValue;
                }
            }
        }
    }
}
