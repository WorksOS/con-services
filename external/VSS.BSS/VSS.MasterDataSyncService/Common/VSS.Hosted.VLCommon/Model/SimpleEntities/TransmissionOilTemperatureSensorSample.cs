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
    public partial class TransmissionOilTemperatureSensorSample
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
    
        public virtual long ifk_DimAssetID
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
    
        public virtual Nullable<double> Latitude
        {
            get;
            set;
        }
    
        public virtual Nullable<double> Longitude
        {
            get;
            set;
        }
    
        public virtual int fk_AssetKeyDate
        {
            get;
            set;
        }
    
        public virtual Nullable<double> Value
        {
            get;
            set;
        }
    
        public virtual Nullable<double> RuntimeHoursMeter
        {
            get;
            set;
        }
    
        public virtual Nullable<int> RPM
        {
            get;
            set;
        }

        #endregion

    }
}
