using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VSS.VisionLink.Raptor.Designs;
using VSS.VisionLink.Raptor.Geometry;
using VSS.VisionLink.Raptor.Utilities.Interfaces;

namespace VSS.VisionLink.Raptor.Surfaces
{
    [Serializable]
    public class SurveyedSurfaces : List<SurveyedSurface>, IComparable<SurveyedSurface>, IBinaryReaderWriter
    {
        private const byte kMajorVersion = 1;
        private const byte kMinorVersion = 3;

        //    var
        //     FMREWSyncInterlock : TMultiReadExclusiveWriteSynchronizer;

        private bool FSorted = false;
        private bool FSortDescending = false;

        public bool Sorted { get { return FSorted; } }

        /// <summary>
        /// No-arg constructor
        /// </summary>
        public SurveyedSurfaces()
        {

        }

        /// <summary>
        /// Constructor accepting a Binary Reader instance from which to instantiate itself
        /// </summary>
        /// <param name="reader"></param>
        public SurveyedSurfaces(BinaryReader reader) : base()
        {
            Read(reader);
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(kMajorVersion);
            writer.Write(kMinorVersion);
            writer.Write((int)Count);

            foreach (SurveyedSurface ss in this)
            {
                ss.Write(writer);
            }
        }

        public void Read(BinaryReader reader)
        {
            ReadVersionFromStream(reader, out byte MajorVersion, out byte MinorVersion);

            if (MajorVersion != kMajorVersion)
            {
                throw new FormatException("Major version incorrect");
            }

            if (MinorVersion != kMinorVersion)
            {
                throw new FormatException("Minor version incorrect");
            }

            int TheCount = reader.ReadInt32();
            for (int i = 0; i < TheCount; i++)
            {
                SurveyedSurface ss = new SurveyedSurface();
                ss.Read(reader);
                Add(ss);
            }
        }

        public void ReadVersionFromStream(BinaryReader reader, out byte MajorVersion, out byte MinorVersion)
        {
            // Load file version info
            MajorVersion = reader.ReadByte();
            MinorVersion = reader.ReadByte();
        }

        /// <summary>
        /// Create a new surveyed surface in the list based on the provided details
        /// </summary>
        /// <param name="ASurveyedSurfaceID"></param>
        /// <param name="ADesignDescriptor"></param>
        /// <param name="AAsAtDate"></param>
        /// <returns></returns>
        public SurveyedSurface AddSurveyedSurfaceDetails(long ASurveyedSurfaceID,
                                                       DesignDescriptor ADesignDescriptor,
                                                       DateTime AAsAtDate,
                                                       BoundingWorldExtent3D AExtents)
        {
            SurveyedSurface match = this.Find(x => x.ID == ASurveyedSurfaceID);

            if (match != null)
            {
                return match;
            }

            SurveyedSurface ss = new SurveyedSurface(ASurveyedSurfaceID, ADesignDescriptor, AAsAtDate, AExtents);
            Add(ss);

            Sort();

            return ss;
        }

        /// <summary>
        /// Remove a given surveyed surface from the list of surveyed surfaces for a site model
        /// </summary>
        /// <param name="ASurveyedSurfaceID"></param>
        /// <returns></returns>
        public bool RemoveSurveyedSurface(long ASurveyedSurfaceID)
        {
            SurveyedSurface match = this.Find(x => x.ID == ASurveyedSurfaceID);

            return match == null ? false : this.Remove(match);
        }

        public SurveyedSurface Locate(long AID) => this.Find(x => x.ID == AID);

        public void Assign(SurveyedSurfaces source)
        {
            Clear();

            foreach (SurveyedSurface ss in source)
            {
                Add(ss.Clone());
            }
        }

        public void SortChronologically(bool Descending = true)
        {
            FSortDescending = Descending;

            Sort();

            FSorted = true;
        }

        public new void Sort()
        {
            base.Sort((x, y) => { return FSortDescending ? y.AsAtDate.CompareTo(x.AsAtDate) : x.AsAtDate.CompareTo(y.AsAtDate); });
        }

        /// <summary>
        /// Determines if there is at least one surveyed surface with an as at date later than the data provided as a DateTime
        /// Optimal performance will be observed if the list is sorted in ascending chronological order
        /// </summary>
        /// <param name="TimeStamp"></param>
        /// <returns></returns>
        public bool HasSurfaceLaterThan(DateTime TimeStamp)
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (this[i].AsAtDate.CompareTo(TimeStamp) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if there is at least one surveyed surface with an as at date later than the data provided as a DateTime.ToBinary() Int64
        /// Optimal performance will be observed if the list is sorted in ascending chronological order
        /// </summary>
        /// <param name="TimeStamp"></param>
        /// <returns></returns>
        public bool HasSurfaceLaterThan(long TimeStamp)
        {
            DateTime _TimeStamp = DateTime.FromBinary(TimeStamp);

            for (int i = Count - 1; i >= 0; i--)
            {
                if (this[i].AsAtDate.CompareTo(_TimeStamp) > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if there is at least one surveyed surface with an as at date earlier than the data provided as a DateTime
        /// Optimal performance will be observed if the list is sorted in ascending chronological order
        /// </summary>
        /// <param name="TimeStamp"></param>
        /// <returns></returns>
        public bool HasSurfaceEarlierThan(DateTime TimeStamp)
        {
            for (int i = 0; i < Count; i++)
            {
                if (this[i].AsAtDate.CompareTo(TimeStamp) < 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines if there is at least one surveyed surface with an as at date earlier than the data provided as a DateTime.ToBinary() Int64
        /// Optimal performance will be observed if the list is sorted in ascending chronological order
        /// </summary>
        /// <param name="TimeStamp"></param>
        /// <returns></returns>
        public bool HasSurfaceEarlierThan(long TimeStamp)
        {
            DateTime _TimeStamp = DateTime.FromBinary(TimeStamp);

            for (int i = 0; i < Count; i++)
            {
                if (this[i].AsAtDate.CompareTo(_TimeStamp) < 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determine if the surveyed surfaces in this list are the same as the surveyed surfaces in the other list, based on ID comparison
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsSameAs(SurveyedSurfaces other)
        {
            if (Count != other.Count)
            {
                return false;
            }

            for (int I = 0; I < Count; I++)
            {
                if (this[I].ID != other[I].ID)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Perform filtering on a set of surveyed surfaces according to the supplied time constraints.
        /// Note: The list of filtered surveyed surfaces is assumed to be empty at the point it is passed to this method
        /// </summary>
        /// <param name="HasTimeFilter"></param>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="ExcludeSurveyedSurfaces"></param>
        /// <param name="FilteredSurveyedSurfaceDetails"></param>
        /// <param name="ExclusionList"></param>
        public void FilterSurveyedSurfaceDetails(bool HasTimeFilter,
                                                 DateTime StartTime, DateTime EndTime,
                                                 bool ExcludeSurveyedSurfaces,
                                                 SurveyedSurfaces FilteredSurveyedSurfaceDetails,
                                                 long[] ExclusionList)
        {
            if (ExcludeSurveyedSurfaces)
            {
                return;
            }

            if (!HasTimeFilter && ExclusionList.Count() == 0)
            {
                FilteredSurveyedSurfaceDetails.Assign(this);
                return;
            }

            foreach (SurveyedSurface ss in this)
            {
                if (!HasTimeFilter)
                {
                    if (!ExclusionList.Any(x => x == ss.ID)) // if SS not excluded from project
                    {
                        FilteredSurveyedSurfaceDetails.Add(ss);  // Formerly ss.Clone
                    }
                }
                else
                {
                    if (ss.AsAtDate >= StartTime && ss.AsAtDate <= EndTime)
                    {
                        if (!ExclusionList.Any(x => x == ss.ID)) // if SS not excluded from project
                        {
                            FilteredSurveyedSurfaceDetails.Add(ss);  // Formerly ss.Clone
                        }
                    }
                }
            }
        }

        //    procedure AcquireReadAccessInterlock; inline;
        //    procedure AcquireWriteAccessInterlock; inline;
        //    procedure ReleaseReadAccessInterlock; inline;
        //    procedure ReleaseWriteAccessInterlock; inline;

        public int CompareTo(SurveyedSurface other)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculate the cache key for the surveyd surface list
        /// </summary>
        /// <param name="SiteModelID"></param>
        /// <returns></returns>
        public static string CacheKey(long SiteModelID)
        {
            return $"{SiteModelID}-SurveyedSurfaces";
        }

        public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);
    }
}
