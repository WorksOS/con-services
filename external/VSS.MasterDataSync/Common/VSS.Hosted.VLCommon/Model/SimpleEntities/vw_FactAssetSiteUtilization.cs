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
    public partial class vw_FactAssetSiteUtilization
    {
        #region Primitive Properties
    
        public virtual long ifk_DimAssetID
        {
            get;
            set;
        }
    
        public virtual long ifk_DimSiteID
        {
            get;
            set;
        }
    
        public virtual int ifk_AssetKeyDate
        {
            get;
            set;
        }
    
        public virtual System.DateTime StartLocalTime_KeyDateBoundary
        {
            get;
            set;
        }
    
        public virtual Nullable<double> StartOdometerMiles_KeyDateBoundary
        {
            get;
            set;
        }
    
        public virtual System.DateTime EndLocalTime_KeyDateBoundary
        {
            get;
            set;
        }
    
        public virtual Nullable<double> EndOdometerMiles_KeyDateBoundary
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> EnterDeviceTime
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> EnterEventUTC
        {
            get;
            set;
        }
    
        public virtual Nullable<int> EnterEventTypeID
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> ExitDeviceTime
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> ExitEventUTC
        {
            get;
            set;
        }
    
        public virtual Nullable<int> ExitEventTypeID
        {
            get;
            set;
        }
    
        public virtual double InSiteHours
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

        #endregion

    }
}
