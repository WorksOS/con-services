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
    public partial class  ScheduleReportAccessAudit
    {
        #region Primitive Properties
    
        public virtual long ID
        {
            get;
            set;
        }
    
        public virtual Nullable<long> fk_ScheduleReportLocationID
        {
            get;
            set;
        }
    
        public virtual System.DateTime AccessDateTimeUTC
        {
            get;
            set;
        }

        #endregion
    }
}
