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
    public partial class ProjectType
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
    
        public virtual ICollection<Project> Project
        {
            get
            {
                if (_project == null)
                {
                    var newCollection = new FixupCollection<Project>();
                    newCollection.CollectionChanged += FixupProject;
                    _project = newCollection;
                }
                return _project;
            }
            set
            {
                if (!ReferenceEquals(_project, value))
                {
                    var previousValue = _project as FixupCollection<Project>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupProject;
                    }
                    _project = value;
                    var newValue = value as FixupCollection<Project>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupProject;
                    }
                }
            }
        }
        private ICollection<Project> _project;

        #endregion

        #region Association Fixup
    
        private void FixupProject(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Project item in e.NewItems)
                {
                    item.fk_ProjectTypeID = ID;
                }
            }
    
            if (e.OldItems != null)
            {
                foreach (Project item in e.OldItems)
                {
                }
            }
        }

        #endregion

    }
}
