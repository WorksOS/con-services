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
    public partial class DataEngineOilLevel
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
    
        public virtual Nullable<double> Value
        {
            get;
            set;
        }

        #endregion

    }
}
