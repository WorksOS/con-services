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
    public partial class MTSBIT
    {
        #region Primitive Properties
    
        public virtual long ID
        {
            get;
            set;
        }
    
        public virtual System.DateTime ReceivedUTC
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> GeneratedUTC
        {
            get;
            set;
        }
    
        public virtual System.DateTime InsertUTC
        {
            get;
            set;
        }
    
        public virtual Nullable<long> SequenceNumber
        {
            get;
            set;
        }
    
        public virtual Nullable<int> BlockID
        {
            get;
            set;
        }
    
        public virtual byte[] BlockPayload
        {
            get;
            set;
        }
    
        public virtual string SerialNumber
        {
            get;
            set;
        }
    
        public virtual int DeviceType
        {
            get;
            set;
        }

        #endregion

    }
}
