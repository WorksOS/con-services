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
    public partial class CustomerType
    {
        #region Primitive Properties
    
        public virtual int ID
        {
            get;
            set;
        }
    
        public virtual string Name
        {
            get;
            set;
        }

        #endregion

        #region Navigation Properties
    
        public virtual ICollection<Customer> Customer
        {
            get
            {
                if (_customer == null)
                {
                    var newCollection = new FixupCollection<Customer>();
                    newCollection.CollectionChanged += FixupCustomer;
                    _customer = newCollection;
                }
                return _customer;
            }
            set
            {
                if (!ReferenceEquals(_customer, value))
                {
                    var previousValue = _customer as FixupCollection<Customer>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupCustomer;
                    }
                    _customer = value;
                    var newValue = value as FixupCollection<Customer>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupCustomer;
                    }
                }
            }
        }
        private ICollection<Customer> _customer;

        #endregion

        #region Association Fixup
    
        private void FixupCustomer(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Customer item in e.NewItems)
                {
                    item.CustomerType = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (Customer item in e.OldItems)
                {
                    if (ReferenceEquals(item.CustomerType, this))
                    {
                        item.CustomerType = null;
                    }
                }
            }
        }

        #endregion

    }
}
