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
    public partial class BSSProvisioningMsg
    {
        #region Primitive Properties
    
        public virtual int fk_BSSStatusID
        {
            get { return _fk_BSSStatusID; }
            set
            {
                if (_fk_BSSStatusID != value)
                {
                    if (BSSStatus != null && BSSStatus.ID != value)
                    {
                        BSSStatus = null;
                    }
                    _fk_BSSStatusID = value;
                }
            }
        }
        private int _fk_BSSStatusID;
    
        public virtual System.DateTime InsertUTC
        {
            get;
            set;
        }
    
        public virtual string BSSMessageType
        {
            get;
            set;
        }
    
        public virtual long SequenceNumber
        {
            get;
            set;
        }
    
        public virtual string SenderIP
        {
            get;
            set;
        }
    
        public virtual string MessageXML
        {
            get;
            set;
        }
    
        public virtual byte FailedCount
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> ProcessedUTC
        {
            get;
            set;
        }
    
        public virtual string MachineName
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
    
        public virtual BSSStatus BSSStatus
        {
            get { return _bSSStatus; }
            set
            {
                if (!ReferenceEquals(_bSSStatus, value))
                {
                    var previousValue = _bSSStatus;
                    _bSSStatus = value;
                    FixupBSSStatus(previousValue);
                }
            }
        }
        private BSSStatus _bSSStatus;
    
        public virtual ICollection<BSSResponseMsg> BSSResponseMsg
        {
            get
            {
                if (_bSSResponseMsg == null)
                {
                    var newCollection = new FixupCollection<BSSResponseMsg>();
                    newCollection.CollectionChanged += FixupBSSResponseMsg;
                    _bSSResponseMsg = newCollection;
                }
                return _bSSResponseMsg;
            }
            set
            {
                if (!ReferenceEquals(_bSSResponseMsg, value))
                {
                    var previousValue = _bSSResponseMsg as FixupCollection<BSSResponseMsg>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupBSSResponseMsg;
                    }
                    _bSSResponseMsg = value;
                    var newValue = value as FixupCollection<BSSResponseMsg>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupBSSResponseMsg;
                    }
                }
            }
        }
        private ICollection<BSSResponseMsg> _bSSResponseMsg;

        #endregion

        #region Association Fixup
    
        private void FixupBSSStatus(BSSStatus previousValue)
        {
            if (BSSStatus != null)
            {
                if (fk_BSSStatusID != BSSStatus.ID)
                {
                    fk_BSSStatusID = BSSStatus.ID;
                }
            }
        }
    
        private void FixupBSSResponseMsg(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (BSSResponseMsg item in e.NewItems)
                {
                    item.BSSProvisioningMsg = this;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (BSSResponseMsg item in e.OldItems)
                {
                    if (ReferenceEquals(item.BSSProvisioningMsg, this))
                    {
                        item.BSSProvisioningMsg = null;
                    }
                }
            }
        }

        #endregion

    }
}
