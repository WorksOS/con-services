using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VSS.TRex.Common;
using VSS.TRex.Common.Extensions;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
using Consts = VSS.TRex.ExistenceMaps.Interfaces.Consts;

namespace VSS.TRex.SurveyedSurfaces
{
  public class SurveyedSurfaces : List<ISurveyedSurface>, ISurveyedSurfaces
  {
    private const byte VERSION_NUMBER = 1;

    private IExistenceMaps existenceMaps;
    private IExistenceMaps GetExistenceMaps() => existenceMaps ?? (existenceMaps = DIContext.Obtain<IExistenceMaps>());

    /// <summary>
    /// No-arg constructor
    /// </summary>
    public SurveyedSurfaces()
    {
    }

    public void Write(BinaryWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.Write(Count);

      foreach (ISurveyedSurface ss in this)
      {
        ss.Write(writer);
      }
    }

    public void Read(BinaryReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      int theCount = reader.ReadInt32();
      for (int i = 0; i < theCount; i++)
      {
        SurveyedSurface surveyedSurface = new SurveyedSurface();
        surveyedSurface.Read(reader);
        Add(surveyedSurface);
      }
    }

    /// <summary>
    /// Determine if the surveyed surfaces in this list are the same as the surveyed surfaces in the other list, based on ID comparison
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsSameAs(ISurveyedSurfaces other)
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
    /// Create a new surveyed surface in the list based on the provided details
    /// </summary>
    /// <param name="surveyedSurfaceUid"></param>
    /// <param name="designDescriptor"></param>
    /// <param name="asAtDate"></param>
    /// <param name="extents"></param>
    /// <returns></returns>
    public ISurveyedSurface AddSurveyedSurfaceDetails(Guid surveyedSurfaceUid,
      DesignDescriptor designDescriptor,
      DateTime asAtDate,
      BoundingWorldExtent3D extents)
    {
      var ss = Find(x => x.ID == surveyedSurfaceUid);

      if (ss == null) // No existing surveyed surface
      {
        ss = new SurveyedSurface(surveyedSurfaceUid, designDescriptor, asAtDate, extents);
        Add(ss);
      }

      return ss;
    }

    /// <summary>
    /// Remove a given surveyed surface from the list of surveyed surfaces for a site model
    /// </summary>
    /// <param name="surveyedSurfaceUid"></param>
    /// <returns></returns>
    public bool RemoveSurveyedSurface(Guid surveyedSurfaceUid)
    {
      ISurveyedSurface match = Find(x => x.ID == surveyedSurfaceUid);

      return match != null && Remove(match);
    }

    /// <summary>
    /// Locates a surveyed surface in the list with the given GUID
    /// </summary>
    /// <param name="surveyedSurfaceUid"></param>
    /// <returns></returns>
    public ISurveyedSurface Locate(Guid surveyedSurfaceUid)
    {
      // Note: This happens a lot and the for loop is faster than foreach or Find(x => x.ID)
      // If numbers of surveyed surfaces become large a Dictionary<Guid, SS> would be good...
      for (int i = 0; i < Count; i++)
        if (this[i].ID == surveyedSurfaceUid)
          return this[i];

      return null;
    }

    public void Assign(ISurveyedSurfaces source)
    {
      Clear();

      foreach (ISurveyedSurface ss in source)
      {
        Add(ss); // formerly Add(ss.Clone());
      }
    }

    public void SortChronologically(bool Descending) => Sort((x, y) => Descending ? y.AsAtDate.CompareTo(x.AsAtDate) : x.AsAtDate.CompareTo(y.AsAtDate));

    /// <summary>
    /// Determines if there is at least one surveyed surface with an as at date later than the data provided as a DateTime
    /// Optimal performance will be observed if the list is sorted in ascending chronological order
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <returns></returns>
    public bool HasSurfaceLaterThan(DateTime timeStamp)
    {
      bool result = false;

      for (int i = Count - 1; i >= 0; i--)
      {
        if (this[i].AsAtDate.CompareTo(timeStamp) > 0)
        {
          result = true;
          break;
        }
      }

      return result;
    }

    /// <summary>
    /// Determines if there is at least one surveyed surface with an as at date later than the data provided as a DateTime.ToBinary() Int64
    /// Optimal performance will be observed if the list is sorted in ascending chronological order
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <returns></returns>
    public bool HasSurfaceLaterThan(long timeStamp)
    {
      DateTime _TimeStamp = DateTime.FromBinary(timeStamp);

      bool result = false;

      for (int i = Count - 1; i >= 0; i--)
      {
        if (this[i].AsAtDate.CompareTo(_TimeStamp) > 0)
        {
          result = true;
          break;
        }
      }

      return result;
    }

    /// <summary>
    /// Determines if there is at least one surveyed surface with an as at date earlier than the data provided as a DateTime
    /// Optimal performance will be observed if the list is sorted in ascending chronological order
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <returns></returns>
    public bool HasSurfaceEarlierThan(DateTime timeStamp)
    {
      bool result = false;

      for (int i = 0; i < Count; i++)
      {
        if (this[i].AsAtDate.CompareTo(timeStamp) < 0)
        {
          result = true;
          break;
        }
      }

      return result;
    }

    /// <summary>
    /// Determines if there is at least one surveyed surface with an as at date earlier than the data provided as a DateTime.ToBinary() Int64
    /// Optimal performance will be observed if the list is sorted in ascending chronological order
    /// </summary>
    /// <param name="timeStamp"></param>
    /// <returns></returns>
    public bool HasSurfaceEarlierThan(long timeStamp)
    {
      DateTime _TimeStamp = DateTime.FromBinary(timeStamp);

      bool result = false;

      for (int i = 0; i < Count; i++)
      {
        if (this[i].AsAtDate.CompareTo(_TimeStamp) < 0)
        {
          result = true;
          break;
        }
      }

      return result;
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

      if (StartTime.Kind != DateTimeKind.Utc || EndTime.Kind != DateTimeKind.Utc)
        throw new ArgumentException("StartTime and EndTime must be UTC date times");

      if (!HasTimeFilter && (ExclusionList?.Length ?? 0) == 0)
      {
        FilteredSurveyedSurfaceDetails.Assign(this);
        return;
      }

      FilteredSurveyedSurfaceDetails.Clear();
      foreach (ISurveyedSurface ss in this)
      {
        if (!HasTimeFilter)
        {
          if (ExclusionList == null || !ExclusionList.Any(x => x == ss.ID)) // if SS not excluded from project
            FilteredSurveyedSurfaceDetails.Add(ss); // Formerly ss.Clone
        }
        else
        {
          if (ss.AsAtDate >= StartTime && ss.AsAtDate <= EndTime &&
              (ExclusionList == null || !ExclusionList.Any(x => x == ss.ID))) // if SS not excluded from project
            FilteredSurveyedSurfaceDetails.Add(ss); // Formerly ss.Clone
        }
      }
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

      if (FilteredSurveyedSurfaces != null)
      {
        if (FilteredSurveyedSurfaces.IsSameAs(ComparisonList))
          return true;

        if (FilteredSurveyedSurfaces.Count > 0)
        {
          ISubGridTreeBitMask surveyedSurfaceExistenceMap = GetExistenceMaps().GetCombinedExistenceMap(siteModelID,
            FilteredSurveyedSurfaces.Select(x => new Tuple<long, Guid>(Consts.EXISTENCE_SURVEYED_SURFACE_DESCRIPTOR, x.ID)).ToArray());

          if (OverallExistenceMap == null)
            return false;

          OverallExistenceMap.SetOp_OR(surveyedSurfaceExistenceMap);
        }
      }

      return true;
    }
  }
}
