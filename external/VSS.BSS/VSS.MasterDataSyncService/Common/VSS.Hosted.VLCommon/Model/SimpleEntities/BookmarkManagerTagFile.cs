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
    public partial class BookmarkManagerTagFile
    {
        #region Primitive Properties
    
        public virtual string OrganizationID
        {
            get;
            set;
        }
    
        public virtual string OrganizationName
        {
            get;
            set;
        }
    
        public virtual bool InProgress
        {
            get;
            set;
        }
    
        public virtual System.DateTime BookmarkUTC
        {
            get;
            set;
        }
    
        public virtual System.DateTime DueUTC
        {
            get;
            set;
        }
    
        public virtual System.DateTime LastStartTimeUTC
        {
            get;
            set;
        }
    
        public virtual System.DateTime LastEndTimeUTC
        {
            get;
            set;
        }
    
        public virtual int LastSubmittedFilesCount
        {
            get;
            set;
        }
    
        public virtual int LastIgnoredFilesCount
        {
            get;
            set;
        }
    
        public virtual int LastRefusedFilesCount
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

    }
}
