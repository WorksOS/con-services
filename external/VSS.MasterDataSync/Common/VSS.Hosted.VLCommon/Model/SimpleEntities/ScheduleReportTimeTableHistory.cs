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
    public partial class ScheduleReportTimeTableHistory
    {
        #region Primitive Properties
    
        public virtual long ID
        {
            get;
            set;
        }
    
        public virtual long fk_ScheduleReportID
        {
            get;
            set;
        }
    
        public virtual string Title
        {
            get;
            set;
        }
    
        public virtual string Description
        {
            get;
            set;
        }
    
        public virtual string ReportSessionID
        {
            get;
            set;
        }
    
        public virtual int fk_SchedulerStatusID
        {
            get;
            set;
        }
    
        public virtual System.DateTime UpdateUTC
        {
            get;
            set;
        }
    
        public virtual long UserId
        {
            get;
            set;
        }
    
        public virtual Nullable<int> Retry
        {
            get;
            set;
        }
    
        public virtual int ReportStartKeyDate
        {
            get;
            set;
        }
    
        public virtual int ReportEndKeyDate
        {
            get;
            set;
        }
    
        public virtual int ScheduleRunKeyDate
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> ScheduleStartDateUTC
        {
            get;
            set;
        }
    
        public virtual Nullable<System.DateTime> ScheduleEndDateUTC
        {
            get;
            set;
        }
    
        public virtual string ScheduleFrequency
        {
            get;
            set;
        }
    
        public virtual string ReportTypeName
        {
            get;
            set;
        }
    
        public virtual string TimezoneName
        {
            get;
            set;
        }
    
        public virtual string LanguageName
        {
            get;
            set;
        }

        #endregion

    }
}
