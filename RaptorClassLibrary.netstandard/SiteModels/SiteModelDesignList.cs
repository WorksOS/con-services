using System;
using System.Collections.Generic;
using VSS.VisionLink.Raptor.Geometry;

namespace VSS.VisionLink.Raptor.SiteModels
{
    [Serializable]
    public class SiteModelDesignList : List<SiteModelDesign>
    {
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
                // TODO add when logging available
                // SIGLogMessage.Publish(Self, Format('An identical design (%s) already exists in the designs for this site.', [Name]), slmcAssert);
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
