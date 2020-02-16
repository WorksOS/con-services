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
using System.ComponentModel.DataAnnotations.Schema;

namespace VSS.Hosted.VLCommon
{
  public partial class UserFeature
  {
    #region Primitive Properties

    public virtual long fk_User
    {
      get { return _fk_User; }
      set
      {
        if (_fk_User != value)
        {
          if (User != null && User.ID != value)
          {
            User = null;
          }
          _fk_User = value;
        }
      }
    }
    private long _fk_User;

    public virtual int fk_Feature
    {
      get { return _fk_Feature; }
      set
      {
        if (_fk_Feature != value)
        {
          if (Feature != null && Feature.ID != value)
          {
            Feature = null;
          }
          _fk_Feature = value;
        }
      }
    }
    private int _fk_Feature;

    public virtual int fk_FeatureAccess
    {
      get { return _fk_FeatureAccess; }
      set
      {
        if (_fk_FeatureAccess != value)
        {
          if (FeatureAccess != null && FeatureAccess.ID != value)
          {
            FeatureAccess = null;
          }
          _fk_FeatureAccess = value;
        }
      }
    }
    private int _fk_FeatureAccess;

    public virtual Nullable<System.DateTime> FirstDataFeedRequestUTC
    {
      get;
      set;
    }

    public virtual Nullable<System.DateTime> LastDataFeedRequestUTC
    {
      get;
      set;
    }

    public virtual Nullable<System.DateTime> CreatedDate
    {
      get;
      set;
    }

    public virtual string Createdby
    {
      get;
      set;
    }

    public virtual System.DateTime UpdateUTC
    {
      get;
      set;
    }

    public virtual long ID
    {
      get;
      set;
    }

    #endregion

    #region Navigation Properties

    public virtual Feature Feature
    {
      get { return _feature; }
      set
      {
        if (!ReferenceEquals(_feature, value))
        {
          var previousValue = _feature;
          _feature = value;
          FixupFeature(previousValue);
        }
      }
    }
    private Feature _feature;

    public virtual FeatureAccess FeatureAccess
    {
      get { return _featureAccess; }
      set
      {
        if (!ReferenceEquals(_featureAccess, value))
        {
          var previousValue = _featureAccess;
          _featureAccess = value;
          FixupFeatureAccess(previousValue);
        }
      }
    }
    private FeatureAccess _featureAccess;

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

    private void FixupFeature(Feature previousValue)
    {
      if (previousValue != null && previousValue.UserFeature.Contains(this))
      {
        previousValue.UserFeature.Remove(this);
      }

      if (Feature != null)
      {
        if (!Feature.UserFeature.Contains(this))
        {
          Feature.UserFeature.Add(this);
        }
        if (fk_Feature != Feature.ID)
        {
          fk_Feature = Feature.ID;
        }
      }
    }

    private void FixupFeatureAccess(FeatureAccess previousValue)
    {
      if (previousValue != null && previousValue.UserFeature.Contains(this))
      {
        previousValue.UserFeature.Remove(this);
      }

      if (FeatureAccess != null)
      {
        if (!FeatureAccess.UserFeature.Contains(this))
        {
          FeatureAccess.UserFeature.Add(this);
        }
        if (fk_FeatureAccess != FeatureAccess.ID)
        {
          fk_FeatureAccess = FeatureAccess.ID;
        }
      }
    }

    private void FixupUser(User previousValue)
    {
      if (previousValue != null && previousValue.UserFeature.Contains(this))
      {
        previousValue.UserFeature.Remove(this);
      }

      if (User != null)
      {
        if (!User.UserFeature.Contains(this))
        {
          User.UserFeature.Add(this);
        }
        if (fk_User != User.ID)
        {
          fk_User = User.ID;
        }
      }
    }

    #endregion

  }
}
