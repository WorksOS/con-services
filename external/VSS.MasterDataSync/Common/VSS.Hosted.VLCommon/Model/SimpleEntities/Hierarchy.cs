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
    public partial class Hierarchy
    {
        #region Primitive Properties
    
        public virtual long ID
        {
            get;
            set;
        }
    
        public virtual long ParentID
        {
            get;
            set;
        }
    
        public virtual string HierarchyPath
        {
            get;
            set;
        }
    
        public virtual string Description
        {
            get;
            set;
        }
    
        public virtual string UserLogin
        {
            get;
            set;
        }
    
        public virtual System.DateTime InsertUTC
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> UpdateUTC
        {
            get;
            set;
        }
    
        public virtual long iFK_HierOwnerID
        {
            get;
            set;
        }
    
        public virtual string HierOwnerTable
        {
            get;
            set;
        }

        #endregion

    }
}
