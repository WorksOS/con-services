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
    public partial class DimAssetCycle
    {
        #region Primitive Properties
    
        public virtual long ifk_DimAssetID
        {
            get;
            set;
        }
    
        public virtual long ifk_CycleID
        {
            get;
            set;
        }
    
        public virtual long ifk_DimCustomerID
        {
            get;
            set;
        }
    
        public virtual string Name
        {
            get;
            set;
        }
    
        public virtual int ifk_DimEventTypeID_Start
        {
            get;
            set;
        }
    
        public virtual int StartSensorNumber
        {
            get;
            set;
        }
    
        public virtual bool StartSensorStartIsOn
        {
            get;
            set;
        }
    
        public virtual int ifk_DimEventTypeID_Stop
        {
            get;
            set;
        }
    
        public virtual int StopSensorNumber
        {
            get;
            set;
        }
    
        public virtual bool StopSensorStopIsOn
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
