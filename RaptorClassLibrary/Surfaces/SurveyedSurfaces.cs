using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Surfaces
{
    public class SurveyedSurfaces : List<SurveyedSurface>, IComparable<SurveyedSurface>
    {
        private const int kMajorVersion = 1;
        private const int kMinorVersion = 3;

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
        public SurveyedSurface AddSurveyedSurfaceDetails(long AGroundSurfaceID,
                                                       DesignDescriptor ADesignDescriptor,
                                                       DateTime AAsAtDate)
        {
            SurveyedSurface match = this.Find(x => x.ID == AGroundSurfaceID);

            if (match != null)
            {
                return match;
            }

            SurveyedSurface ss = new SurveyedSurface(AGroundSurfaceID, ADesignDescriptor, AAsAtDate);
            Add(new SurveyedSurface(AGroundSurfaceID, ADesignDescriptor, AAsAtDate));

            Sort();

            return ss;
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

        public void FilterGroundSurfaceDetails(bool HasTimeFilter,
        DateTime StartTime, DateTime EndTime,
                                         bool ExcludeSurveyedSurfaces,
                                         SurveyedSurfaces FilteredGroundSurfaceDetails,
                                         long[] ExclusionList)
        {
            if (ExcludeSurveyedSurfaces)
            {
                FilteredGroundSurfaceDetails.Clear();
                return;
            }

            if (!HasTimeFilter && ExclusionList.Count() == 0)
            {
                FilteredGroundSurfaceDetails.Assign(this);
                return;
            }

            FilteredGroundSurfaceDetails.Clear();
            foreach (SurveyedSurface ss in this)
            {
                if (!ExclusionList.Any(x => x == ss.ID)) // if SS not excluded from project
                {
                    if (!HasTimeFilter)
                    {
                        FilteredGroundSurfaceDetails.Add(ss.Clone());
                    }
                    else
                    {
                        if (ss.AsAtDate >= StartTime && ss.AsAtDate >= EndTime)
                        {
                            FilteredGroundSurfaceDetails.Add(ss.Clone());
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
