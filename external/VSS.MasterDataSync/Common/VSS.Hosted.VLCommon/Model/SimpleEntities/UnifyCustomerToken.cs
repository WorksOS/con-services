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
    public partial class UnifyCustomerToken
    {
        #region Primitive Properties
    
        public virtual long ID
        {
            get;
            set;
        }
    
        public virtual long fk_CustomerID
        {
            get { return _fk_CustomerID; }
            set
            {
                if (_fk_CustomerID != value)
                {
                    if (Customer != null && Customer.ID != value)
                    {
                        Customer = null;
                    }
                    _fk_CustomerID = value;
                }
            }
        }
        private long _fk_CustomerID;
    
        public virtual System.Guid UnifyOrgID
        {
            get;
            set;
        }
    
        public virtual System.DateTime InsertUTC
        {
            get;
            set;
        }
    
        public virtual long fk_UserID
        {
            get { return _fk_UserID; }
            set
            {
                if (_fk_UserID != value)
                {
                    if (User != null && User.ID != value)
                    {
                        User = null;
                    }
                    _fk_UserID = value;
                }
            }
        }
        private long _fk_UserID;

        #endregion

        #region Navigation Properties
    
        public virtual Customer Customer
        {
            get { return _customer; }
            set
            {
                if (!ReferenceEquals(_customer, value))
                {
                    var previousValue = _customer;
                    _customer = value;
                    FixupCustomer(previousValue);
                }
            }
        }
        private Customer _customer;
    
        public virtual User User
        {
            get { return _user; }
            set
            {
                if (!ReferenceEquals(_user, value))
                {
                    var previousValue = _user;
                    _user = value;
                    FixupUser(previousValue);
                }
            }
        }
        private User _user;

        #endregion

        #region Association Fixup
    
        private void FixupCustomer(Customer previousValue)
        {
            if (previousValue != null && previousValue.UnifyCustomerToken.Contains(this))
            {
                previousValue.UnifyCustomerToken.Remove(this);
            }
    
            if (Customer != null)
            {
                if (!Customer.UnifyCustomerToken.Contains(this))
                {
                    Customer.UnifyCustomerToken.Add(this);
                }
                if (fk_CustomerID != Customer.ID)
                {
                    fk_CustomerID = Customer.ID;
                }
            }
        }
    
        private void FixupUser(User previousValue)
        {
            if (previousValue != null && previousValue.UnifyCustomerToken.Contains(this))
            {
                previousValue.UnifyCustomerToken.Remove(this);
            }
    
            if (User != null)
            {
                if (!User.UnifyCustomerToken.Contains(this))
                {
                    User.UnifyCustomerToken.Add(this);
                }
                if (fk_UserID != User.ID)
                {
                    fk_UserID = User.ID;
                }
            }
        }

        #endregion

    }
}
