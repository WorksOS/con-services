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
    public partial class CAT_FenceAlert
    {
        #region Primitive Properties
    
        public virtual long ID
        {
            get;
            set;
        }
    
        public virtual System.DateTime InsertUTC
        {
            get;
            set;
        }
    
        public virtual long MessageID
        {
            get;
            set;
        }
    
        public virtual long MasterMsgID
        {
            get;
            set;
        }
    
        public virtual System.DateTime EventUTC
        {
            get;
            set;
        }
    
        public virtual string SerialNumber
        {
            get;
            set;
        }
    
        public virtual string MakeCode
        {
            get;
            set;
        }
    
        public virtual string GpsDeviceID
        {
            get;
            set;
        }
    
        public virtual int DeviceTypeID
        {
            get;
            set;
        }
    
        public virtual bool TimeWatchActive
        {
            get;
            set;
        }
    
        public virtual bool ExclusiveWatchActive
        {
            get;
            set;
        }
    
        public virtual bool InclusiveWatchActive
        {
            get;
            set;
        }
    
        public virtual bool TimeWatchAlarm
        {
            get;
            set;
        }
    
        public virtual bool ExclusiveWatchAlarm
        {
            get;
            set;
        }
    
        public virtual bool InclusiveWatchAlarm
        {
            get;
            set;
        }
    
        public virtual bool SatelliteBlockage
        {
            get;
            set;
        }
    
        public virtual bool DisconnectSwitchUsed
        {
            get;
            set;
        }
    
        public virtual long RecordID
        {
            get;
            set;
        }

        #endregion

    }
}
