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
    public partial class CustomDataEngineParameters
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
    
        public virtual Nullable<long> DebugRefID
        {
            get;
            set;
        }
    
        public virtual Nullable<long> SourceMsgID
        {
            get;
            set;
        }
    
        public virtual int fk_DimSourceID
        {
            get;
            set;
        }
    
        public virtual System.DateTime EventUTC
        {
            get;
            set;
        }
    
        public virtual long AssetID
        {
            get;
            set;
        }
    
        public virtual string MID
        {
            get;
            set;
        }
    
        public virtual Nullable<double> MaxFuelGallons
        {
            get;
            set;
        }
    
        public virtual Nullable<double> IdleFuelGallons
        {
            get;
            set;
        }
    
        public virtual Nullable<double> MachineIdleFuelGallons
        {
            get;
            set;
        }
    
        public virtual Nullable<double> EngineIdleHours
        {
            get;
            set;
        }
    
        public virtual Nullable<int> Starts
        {
            get;
            set;
        }
    
        public virtual Nullable<long> Revolutions
        {
            get;
            set;
        }
    
        public virtual Nullable<double> ConsumptionGallons
        {
            get;
            set;
        }
    
        public virtual Nullable<double> LevelPercent
        {
            get;
            set;
        }
    
        public virtual Nullable<double> MachineIdleHours
        {
            get;
            set;
        }
    
        public virtual System.DateTime UpdateUTC
        {
            get;
            set;
        }
    
        public virtual bool ValidFuelPercent
        {
            get;
            set;
        }

        #endregion

    }
}
