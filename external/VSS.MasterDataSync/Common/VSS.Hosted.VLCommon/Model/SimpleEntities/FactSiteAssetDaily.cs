//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace VSS.Hosted.VLCommon
{
    public partial class FactSiteAssetDaily
    {
        #region Primitive Properties
    
        public virtual long ifk_DimSiteID
        {
            get;
            set;
        }
    
        public virtual long ifk_DimAssetID
        {
            get;
            set;
        }
    
        public virtual int fk_AssetKeyDate
        {
            get;
            set;
        }
    
        public virtual Nullable<double> IdleHours
        {
            get;
            set;
        }
    
        public virtual Nullable<double> WorkingHours
        {
            get;
            set;
        }
    
        public virtual Nullable<int> StartCycleToLocalCount
        {
            get;
            set;
        }
    
        public virtual Nullable<int> StartCycleToRemoteCount
        {
            get;
            set;
        }
    
        public virtual Nullable<int> StartCycleToUndefinedCount
        {
            get;
            set;
        }
    
        public virtual Nullable<int> StopCycleFromLocalCount
        {
            get;
            set;
        }
    
        public virtual Nullable<int> StopCycleFromRemoteCount
        {
            get;
            set;
        }
    
        public virtual Nullable<int> StopCycleFromUndefinedCount
        {
            get;
            set;
        }
    
        public virtual System.DateTime UpdateUTC
        {
            get;
            set;
        }
    
        public virtual Nullable<double> AvgTimePerLoadMinutes
        {
            get;
            set;
        }
    
        public virtual Nullable<double> AvgLoadTimeMinutes
        {
            get;
            set;
        }
    
        public virtual Nullable<double> AvgDumpTimeMinutes
        {
            get;
            set;
        }
    
        public virtual Nullable<double> AvgHaulTimeMinutes
        {
            get;
            set;
        }
    
        public virtual Nullable<double> AvgHaulDistanceKm
        {
            get;
            set;
        }
    
        public virtual Nullable<int> CycleCount
        {
            get;
            set;
        }
    
        public virtual Nullable<double> AvgCycleTimeMinutes
        {
            get;
            set;
        }
    
        public virtual Nullable<double> AvgCycleDistanceKm
        {
            get;
            set;
        }
    
        public virtual Nullable<double> InSiteHours
        {
            get;
            set;
        }
    
        public virtual Nullable<int> LoadCompleteCount
        {
            get;
            set;
        }
    
        public virtual Nullable<int> LoadIncompleteCount
        {
            get;
            set;
        }
    
        public virtual Nullable<int> LoadProximityCount
        {
            get;
            set;
        }

        #endregion

    }
}
