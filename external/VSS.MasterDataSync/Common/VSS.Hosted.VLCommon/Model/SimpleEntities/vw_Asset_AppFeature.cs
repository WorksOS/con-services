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
    public partial class vw_Asset_AppFeature
    {
        #region Primitive Properties
    
        public virtual long fk_ActiveUserID
        {
            get;
            set;
        }
    
        public virtual long fk_AssetID
        {
            get;
            set;
        }
    
        public virtual int fk_AppFeatureID
        {
            get;
            set;
        }
    
        public virtual int StartKeyDate
        {
            get;
            set;
        }
    
        public virtual int EndKeyDate
        {
            get;
            set;
        }

        #endregion

    }
}
