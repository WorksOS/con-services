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
    public partial class vw_WorkingSet
    {
        #region Primitive Properties
    
        public virtual long ifk_ActiveUserID
        {
            get;
            set;
        }
    
        public virtual long fk_DimAssetID
        {
            get;
            set;
        }
    
        public virtual bool IsOwned
        {
            get;
            set;
        }
    
        public virtual Nullable<long> fk_ProjectID
        {
            get;
            set;
        }

        #endregion

    }
}
