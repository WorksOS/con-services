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
    public partial class CustomerRelationshipExport
    {
        #region Primitive Properties
    
        public virtual long ID
        {
            get;
            set;
        }
    
        public virtual long ParentCustomerID
        {
            get;
            set;
        }
    
        public virtual long AssociatedCustomerID
        {
            get;
            set;
        }
    
        public virtual string Operation
        {
            get;
            set;
        }
    
        public virtual System.DateTime LastCustomerRelationshipExportUTC
        {
            get;
            set;
        }

        #endregion

    }
}
