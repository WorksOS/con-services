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
    public partial class FactAssetCycleEventEmulatedManual
    {
        #region Primitive Properties
    
        public virtual long ID
        {
            get;
            set;
        }
    
        public virtual long ifk_DimCustomerID
        {
            get;
            set;
        }
    
        public virtual long ifk_DimProjectID
        {
            get;
            set;
        }
    
        public virtual long ifk_DimAssetID
        {
            get;
            set;
        }
    
        public virtual int EventAssetKeyDate
        {
            get;
            set;
        }
    
        public virtual System.DateTime EventUTC
        {
            get;
            set;
        }
    
        public virtual System.DateTime EventDeviceTime
        {
            get;
            set;
        }
    
        public virtual int ifk_DimEventTypeID
        {
            get;
            set;
        }
    
        public virtual long ifk_DimSiteID
        {
            get;
            set;
        }
    
        public virtual bool IsDeleted
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

        #endregion

    }
}
