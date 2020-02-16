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
    public partial class FactAssetUtilizationDaily
    {
        #region Primitive Properties
    
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
    
        public virtual Nullable<int> fk_AssetPriorKeyDate
        {
            get;
            set;
        }
    
        public virtual int ifk_DimWorkDefinitionID
        {
            get;
            set;
        }
    
        public virtual Nullable<double> RuntimeHours
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
    
        public virtual Nullable<double> TransmissionPTOHours
        {
            get;
            set;
        }
    
        public virtual Nullable<double> EnginePTOHours
        {
            get;
            set;
        }
    
        public virtual Nullable<double> DistanceTravelled
        {
            get;
            set;
        }
    
        public virtual Nullable<double> TotalFuelConsumedGallons
        {
            get;
            set;
        }
    
        public virtual Nullable<double> IdleFuelConsumedGallons
        {
            get;
            set;
        }
    
        public virtual Nullable<double> WorkingFuelConsumedGallons
        {
            get;
            set;
        }
    
        public virtual Nullable<double> AverageBurnRateGallonsPerHour
        {
            get;
            set;
        }
    
        public virtual bool IsEstimated
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> EventUTC
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> EventDeviceTime
        {
            get;
            set;
        }
    
        public virtual Nullable<double> RuntimeHoursMeter
        {
            get;
            set;
        }
    
        public virtual Nullable<double> IdleHoursMeter
        {
            get;
            set;
        }
    
        public virtual Nullable<double> MileageMeter
        {
            get;
            set;
        }
    
        public virtual Nullable<double> TotalFuelConsumedGallonsMeter
        {
            get;
            set;
        }
    
        public virtual Nullable<double> IdleFuelConsumedGallonsMeter
        {
            get;
            set;
        }
    
        public virtual Nullable<int> ifk_DimTimeZoneID
        {
            get;
            set;
        }
    
        public virtual System.DateTime InsertUTC
        {
            get;
            set;
        }
    
        public virtual System.DateTime UpdateUTC
        {
            get;
            set;
        }
    
        public virtual int MasterDisconnectTypeID
        {
            get;
            set;
        }
    
        public virtual int ifk_RuntimeHoursCalloutTypeID
        {
            get;
            set;
        }
    
        public virtual int ifk_IdleHoursCalloutTypeID
        {
            get;
            set;
        }
    
        public virtual int ifk_TotalFuelConsumedGallonsCalloutTypeID
        {
            get;
            set;
        }
    
        public virtual int ifk_IdleFuelConsumedGallonsCalloutTypeID
        {
            get;
            set;
        }
    
        public virtual int ifk_WorkingHoursCalloutTypeID
        {
            get;
            set;
        }

        #endregion

    }
}
