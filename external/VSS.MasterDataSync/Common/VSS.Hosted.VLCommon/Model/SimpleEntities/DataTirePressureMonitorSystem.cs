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
    public partial class DataTirePressureMonitorSystem
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
    
        public virtual long AssetID
        {
            get;
            set;
        }
    
        public virtual System.DateTime EventUTC
        {
            get;
            set;
        }
    
        public virtual Nullable<int> ECMCount
        {
            get;
            set;
        }
    
        public virtual string ECMSourceAddress
        {
            get;
            set;
        }
    
        public virtual string ECMDescription
        {
            get;
            set;
        }
    
        public virtual Nullable<int> ECMTireCount
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
    
        public virtual Nullable<int> fk_SensorTypeID
        {
            get;
            set;
        }
    
        public virtual int fk_TPMSSensorAspectsID
        {
            get;
            set;
        }
    
        public virtual Nullable<double> SensorValue
        {
            get;
            set;
        }

        #endregion

    }
}
