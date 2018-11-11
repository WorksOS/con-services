using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;

namespace VSS.TRex.SurveyedSurfaces
{
  public class SurveyedSurfaces : List<ISurveyedSurface>, IComparable<ISurveyedSurface>, ISurveyedSurfaces
  {
    private const byte kMajorVersion = 1;
    private const byte kMinorVersion = 3;

    private bool FSorted;
    private bool SortDescending;

    public bool Sorted { get { return FSorted; } }

    private IExistenceMaps existenceMaps;
    private IExistenceMaps GetExistenceMaps() => existenceMaps ?? (existenceMaps = DIContext.Obtain<IExistenceMaps>());

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
    public SurveyedSurfaces(BinaryReader reader)
    {
      Read(reader);
    }

    public void Write(BinaryWriter writer)
    {
      writer.Write(kMajorVersion);
      writer.Write(kMinorVersion);
      writer.Write((int)Count);

      foreach (ISurveyedSurface ss in this)
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
    /// <param name="AExtents"></param>
    /// <returns></returns>
    public ISurveyedSurface AddSurveyedSurfaceDetails(Guid ASurveyedSurfaceID,
                                                   DesignDescriptor ADesignDescriptor,
                                                   DateTime AAsAtDate,
                                                   BoundingWorldExtent3D AExtents)
    {
      ISurveyedSurface match = Find(x => x.ID == ASurveyedSurfaceID);

      if (match != null)
      {
        return match;
      }

      ISurveyedSurface ss = new SurveyedSurface(ASurveyedSurfaceID, ADesignDescriptor, AAsAtDate, AExtents);
      Add(ss);

      Sort();

      return ss;
    }

    /// <summary>
    /// Remove a given surveyed surface from the list of surveyed surfaces for a site model
    /// </summary>
    /// <param name="ASurveyedSurfaceID"></param>
    /// <returns></returns>
    public bool RemoveSurveyedSurface(Guid ASurveyedSurfaceID)
    {
      ISurveyedSurface match = Find(x => x.ID == ASurveyedSurfaceID);

      return match != null && Remove(match);
    }

    /// <summary>
    /// Locates a surveyed surface in the list with the given GUID
    /// </summary>
    /// <param name="AID"></param>
    /// <returns></returns>
    public ISurveyedSurface Locate(Guid AID)
    {
      // Note: This happens a lot and the for loop is faster than foreach or Find(x => x.ID)
      // If numbers of surveyed surfaces become large a Dictionary<Guid, SS> would be good...
      for (int i = 0; i < Count; i++)
        if (this[i].ID == AID)
          return this[i];

      return null;
    }

    public void Assign(ISurveyedSurfaces source)
    {
      Clear();

      foreach (ISurveyedSurface ss in source)
      {
        Add(ss.Clone());
      }
    }

    public void SortChronologically(bool Descending = true)
    {
      SortDescending = Descending;

      Sort();

      FSorted = true;
    }

    public new void Sort()
    {
      base.Sort((x, y) => SortDescending ? y.AsAtDate.CompareTo(x.AsAtDate) : x.AsAtDate.CompareTo(y.AsAtDate));
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
                                             ISurveyedSurfaces FilteredSurveyedSurfaceDetails,
                                             Guid[] ExclusionList)
    {
      if (ExcludeSurveyedSurfaces)
        return;

      if (!HasTimeFilter && ExclusionList.Length == 0)
      {
        FilteredSurveyedSurfaceDetails.Assign(this);
        return;
      }

      FilteredSurveyedSurfaceDetails.Clear();
      foreach (ISurveyedSurface ss in this)
      {
        if (!HasTimeFilter)
        {
          if (!ExclusionList.Any(x => x == ss.ID)) // if SS not excluded from project
            FilteredSurveyedSurfaceDetails.Add(ss);  // Formerly ss.Clone
        }
        else
        {
          if (ss.AsAtDate >= StartTime && ss.AsAtDate <= EndTime &&
              !ExclusionList.Any(x => x == ss.ID)) // if SS not excluded from project
            FilteredSurveyedSurfaceDetails.Add(ss);  // Formerly ss.Clone
        }
      }
    }

    public int CompareTo(ISurveyedSurface other)
    {
      throw new NotImplementedException();
    }

    public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);

    /// <summary>
    /// Given a filter compute which of the surfaces in the list match any given time aspect
    /// of the filter, and the overall existence map of the surveyed surfaces that match the filter.
    /// ComparisonList denotes a possibly pre-filtered set of surfaces for another filter; if this is the same as the 
    /// filtered set of surfaces then the overall existence map for those surfaces will not be computed as it is 
    /// assumed to be the same.
    /// </summary>
    /// <param name="siteModelID"></param>
    /// <param name="Filter"></param>
    /// <param name="ComparisonList"></param>
    /// <param name="FilteredSurveyedSurfaces"></param>
    /// <param name="OverallExistenceMap"></param>
    /// <returns></returns>
    public bool ProcessSurveyedSurfacesForFilter(Guid siteModelID,
                                                 ICombinedFilter Filter,
                                                 ISurveyedSurfaces ComparisonList,
                                                 ISurveyedSurfaces FilteredSurveyedSurfaces,
                                                 ISubGridTreeBitMask OverallExistenceMap)
    {
      // Filter out any surveyed surfaces which don't match current filter (if any) - realistically, this is time filters we're thinking of here
      FilterSurveyedSurfaceDetails(Filter.AttributeFilter.HasTimeFilter,
                                                    Filter.AttributeFilter.StartTime, Filter.AttributeFilter.EndTime,
                                                    Filter.AttributeFilter.ExcludeSurveyedSurfaces(),
                                                    FilteredSurveyedSurfaces,
                                                    Filter.AttributeFilter.SurveyedSurfaceExclusionList);

      if (FilteredSurveyedSurfaces?.Equals(ComparisonList) == true)
        return true;

      if (FilteredSurveyedSurfaces.Count > 0)
      {
        ISubGridTreeBitMask surveyedSurfaceExistenceMap = GetExistenceMaps().GetCombinedExistenceMap(siteModelID,
        FilteredSurveyedSurfaces.Select(x => new Tuple<long, Guid>(Consts.EXISTENCE_SURVEYED_SURFACE_DESCRIPTOR, x.ID)).ToArray());

        if (OverallExistenceMap == null)
          return false;

        if (surveyedSurfaceExistenceMap != null)
          OverallExistenceMap.SetOp_OR(surveyedSurfaceExistenceMap);
      }

      return true;
    }
  }
}
