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
    public partial class FactTireMonitorLevelPeriod
    {
        #region Primitive Properties
    
        public virtual long ifk_DimAssetID
        {
            get;
            set;
        }
    
        public virtual int ifk_AssetKeyDate
        {
            get;
            set;
        }
    
        public virtual int AxlePosition
        {
            get;
            set;
        }
    
        public virtual int TirePosition
        {
            get;
            set;
        }
    
        public virtual System.DateTime StartEventDeviceTime
        {
            get;
            set;
        }
    
        public virtual int ifk_DimSensorTypeID
        {
            get;
            set;
        }
    
        public virtual int ifk_DimWarningLevelID
        {
            get;
            set;
        }
    
        public virtual System.DateTime EndEventDeviceTime
        {
            get;
            set;
        }
    
        public virtual int EndEventType
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
