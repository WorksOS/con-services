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
    public partial class DimSensor
    {
        #region Primitive Properties
    
        public virtual long ID
        {
            get;
            set;
        }
    
        public virtual long fk_DimAssetID
        {
            get;
            set;
        }
    
        public virtual int SensorNumber
        {
            get;
            set;
        }
    
        public virtual string Name
        {
            get;
            set;
        }
    
        public virtual string OnDescription
        {
            get;
            set;
        }
    
        public virtual string OffDescription
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
