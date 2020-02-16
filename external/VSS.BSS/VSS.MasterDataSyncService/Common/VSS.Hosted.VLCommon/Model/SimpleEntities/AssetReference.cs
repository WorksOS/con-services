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
    public partial class AssetReference
    {
        #region Primitive Properties
    
        public virtual long ID
        {
            get;
            set;
        }
    
        public virtual long fk_StoreID
        {
            get { return _fk_StoreID; }
            set
            {
                if (_fk_StoreID != value)
                {
                    if (Store != null && Store.ID != value)
                    {
                        Store = null;
                    }
                    _fk_StoreID = value;
                }
            }
        }
        private long _fk_StoreID;
    
        public virtual string Alias
        {
            get;
            set;
        }
    
        public virtual string Value
        {
            get;
            set;
        }
    
        public virtual System.Guid UID
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

        #region Navigation Properties
    
        public virtual Store Store
        {
            get { return _store; }
            set
            {
                if (!ReferenceEquals(_store, value))
                {
                    var previousValue = _store;
                    _store = value;
                    FixupStore(previousValue);
                }
            }
        }
        private Store _store;

        #endregion

        #region Association Fixup
    
        private void FixupStore(Store previousValue)
        {
            if (previousValue != null && previousValue.AssetReference.Contains(this))
            {
                previousValue.AssetReference.Remove(this);
            }
    
            if (Store != null)
            {
                if (!Store.AssetReference.Contains(this))
                {
                    Store.AssetReference.Add(this);
                }
                if (fk_StoreID != Store.ID)
                {
                    fk_StoreID = Store.ID;
                }
            }
        }

        #endregion

    }
}
