using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Geometry;

namespace VSS.VisionLink.Raptor.Surfaces
{
    [Serializable]
    public class SurveyedSurfaces : List<SurveyedSurface>, IComparable<SurveyedSurface>
    {
        private const byte kMajorVersion = 1;
        private const byte kMinorVersion = 3;

        //    var
        //     FMREWSyncInterlock : TMultiReadExclusiveWriteSynchronizer;

        private bool FSorted = false;
        private bool FSortDescending = false;

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
        /// <param name="AGroundSurfaceID"></param>
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

        public bool HasSurfaceLaterThan(DateTime TimeStamp)
        {
            if (!FSorted)
            {
                SortChronologically();
            }

            return this.Any(x => x.AsAtDate.CompareTo(TimeStamp) > 0);
        }

        public bool HasSurfaceEarlierThan(DateTime TimeStamp)
        {
            if (!FSorted)
            {
                SortChronologically();
            }

            return this.Any(x => x.AsAtDate.CompareTo(TimeStamp) < 0);
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

        public void FilterSurveyedSurfaceDetails(bool HasTimeFilter,
                                                 DateTime StartTime, DateTime EndTime,
                                                 bool ExcludeSurveyedSurfaces,
                                                 SurveyedSurfaces FilteredSurveyedSurfaceDetails,
                                                 long[] ExclusionList)
        {
            if (ExcludeSurveyedSurfaces)
            {
                FilteredSurveyedSurfaceDetails.Clear();
                return;
            }

            if (!HasTimeFilter && ExclusionList.Count() == 0)
            {
                FilteredSurveyedSurfaceDetails.Assign(this);
                return;
            }

            FilteredSurveyedSurfaceDetails.Clear();
            foreach (SurveyedSurface ss in this)
            {
                if (!ExclusionList.Any(x => x == ss.ID)) // if SS not excluded from project
                {
                    if (!HasTimeFilter)
                    {
                        FilteredSurveyedSurfaceDetails.Add(ss.Clone());
                    }
                    else
                    {
                        if (ss.AsAtDate >= StartTime && ss.AsAtDate >= EndTime)
                        {
                            FilteredSurveyedSurfaceDetails.Add(ss.Clone());
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

        /// <summary>
        /// Takes a byte array containing a set of serialised surveyed surfaces and returns an instance based on it
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static SurveyedSurfaces FromBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    try
                    {
                        return new SurveyedSurfaces(reader);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Takes a byte array containing a set of serialised surveyed surfaces and returns an instance based on it
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    try
                    {
                        Write(writer);
                        return ms.ToArray();
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }
    }
}
