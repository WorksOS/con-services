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
    public partial class CustomerRelationship
    {
        #region Primitive Properties
    
        public virtual long fk_ParentCustomerID
        {
            get;
            set;
        }
    
        public virtual long fk_ClientCustomerID
        {
            get;
            set;
        }
    
        public virtual string BSSRelationshipID
        {
            get;
            set;
        }
    
        public virtual int fk_CustomerRelationshipTypeID
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
