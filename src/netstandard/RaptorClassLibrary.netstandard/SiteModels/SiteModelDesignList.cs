using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using VSS.TRex.Geometry;

namespace VSS.TRex.SiteModels
{
    [Serializable]
    public class SiteModelDesignList : List<SiteModelDesign>
    {
        [NonSerialized]
        private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType?.Name);

        /// <summary>
        /// Indexer supporting locating designs by the design name
        /// </summary>
        /// <param name="designName"></param>
        /// <returns></returns>
        public SiteModelDesign this[string designName]
        {
            get
            {
                int index = IndexOf(designName);
                return index > 0 ? this[index] : null;
            }
        }

        public int IndexOf(string designName) => FindIndex(x => x.Name == designName);

        public SiteModelDesign CreateNew(string name, BoundingWorldExtent3D extents)
        {
            int index = IndexOf(name);

            if (index != -1)
            {
                Log.LogError($"An identical design ({name}) already exists in the designs for this site.");
                return this[index];
            }

            SiteModelDesign design = new SiteModelDesign(name, extents);
            Add(design);

            return design;
        }

        public void InitialiseWorkingExtents()
        {
            lock (this)
            {
                ForEach(x => x.WorkingExtents.SetInverted());
            }
        }

        public void AssignWorkingExtentsToExtents()
        {
            lock (this)
            {
                ForEach(x => x.Extents.Assign(x.WorkingExtents));
            }
        }
    }
}
