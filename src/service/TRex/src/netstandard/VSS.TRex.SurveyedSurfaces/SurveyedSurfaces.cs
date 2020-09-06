using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.ExistenceMaps.Interfaces;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SurveyedSurfaces.Interfaces;
// ReSharper disable once IdentifierTypo
using Consts = VSS.TRex.ExistenceMaps.Interfaces.Consts;

namespace VSS.TRex.SurveyedSurfaces
{
  public class SurveyedSurfaces : List<ISurveyedSurface>, ISurveyedSurfaces
  {
    private const byte VERSION_NUMBER = 1;

    private IExistenceMaps _existenceMaps;
    private IExistenceMaps GetExistenceMaps() => _existenceMaps ??= DIContext.Obtain<IExistenceMaps>();

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

      foreach (var ss in this)
      {
        ss.Write(writer);
      }
    }

    public void Read(BinaryReader reader)
    {
      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      var theCount = reader.ReadInt32();
      for (var i = 0; i < theCount; i++)
      {
        var surveyedSurface = new SurveyedSurface();
        surveyedSurface.Read(reader);
        Add(surveyedSurface);
      }
    }

    /// <summary>
    /// Determine if the surveyed surfaces in this list are the same as the surveyed surfaces in the other list, based on ID comparison
    /// </summary>
    public bool IsSameAs(ISurveyedSurfaces other)
    {
      if (Count != other.Count)
      {
        return false;
      }

      for (var I = 0; I < Count; I++)
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
    public bool RemoveSurveyedSurface(Guid surveyedSurfaceUid)
    {
      var match = Find(x => x.ID == surveyedSurfaceUid);

      return match != null && Remove(match);
    }

    /// <summary>
    /// Locates a surveyed surface in the list with the given GUID
    /// </summary>
    public ISurveyedSurface Locate(Guid surveyedSurfaceUid)
    {
      // Note: This happens a lot and the for loop is faster than foreach or Find(x => x.ID)
      // If numbers of surveyed surfaces become large a Dictionary<Guid, SS> would be good...
      for (var i = 0; i < Count; i++)
      {
        if (this[i].ID == surveyedSurfaceUid)
          return this[i];
      }

      return null;
    }

    public void Assign(ISurveyedSurfaces source)
    {
      Clear();

      foreach (var ss in source)
      {
        Add(ss); // formerly Add(ss.Clone());
      }
    }

    public void SortChronologically(bool descending) => Sort((x, y) => descending ? y.AsAtDate.CompareTo(x.AsAtDate) : x.AsAtDate.CompareTo(y.AsAtDate));

    /// <summary>
    /// Determines if there is at least one surveyed surface with an as at date later than the data provided as a DateTime
    /// Optimal performance will be observed if the list is sorted in ascending chronological order
    /// </summary>
    public bool HasSurfaceLaterThan(DateTime timeStamp)
    {
      var result = false;

      for (var i = Count - 1; i >= 0; i--)
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
    public bool HasSurfaceLaterThan(long timeStamp)
    {
      var localTimeStamp = DateTime.FromBinary(timeStamp);

      var result = false;

      for (var i = Count - 1; i >= 0; i--)
      {
        if (this[i].AsAtDate.CompareTo(localTimeStamp) > 0)
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
    public bool HasSurfaceEarlierThan(DateTime timeStamp)
    {
      var result = false;

      for (var i = 0; i < Count; i++)
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
    public bool HasSurfaceEarlierThan(long timeStamp)
    {
      var localTimeStamp = DateTime.FromBinary(timeStamp);

      var result = false;

      for (var i = 0; i < Count; i++)
      {
        if (this[i].AsAtDate.CompareTo(localTimeStamp) < 0)
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
    public void FilterSurveyedSurfaceDetails(bool hasTimeFilter,
      DateTime startTime, DateTime endTime,
      bool excludeSurveyedSurfaces,
      ISurveyedSurfaces filteredSurveyedSurfaceDetails,
      Guid[] exclusionList)
    {
      if (excludeSurveyedSurfaces)
        return;

      if (startTime.Kind != DateTimeKind.Utc || endTime.Kind != DateTimeKind.Utc)
        throw new ArgumentException("StartTime and EndTime must be UTC date times");

      if (!hasTimeFilter && (exclusionList?.Length ?? 0) == 0)
      {
        filteredSurveyedSurfaceDetails.Assign(this);
        return;
      }

      filteredSurveyedSurfaceDetails.Clear();
      foreach (var ss in this)
      {
        if (!hasTimeFilter)
        {
          if (exclusionList == null || !exclusionList.Any(x => x == ss.ID)) // if SS not excluded from project
            filteredSurveyedSurfaceDetails.Add(ss); // Formerly ss.Clone
        }
        else
        {
          if (ss.AsAtDate >= startTime && ss.AsAtDate <= endTime &&
              (exclusionList == null || !exclusionList.Any(x => x == ss.ID))) // if SS not excluded from project
            filteredSurveyedSurfaceDetails.Add(ss); // Formerly ss.Clone
        }
      }
    }

    /// <summary>
    /// Given a filter compute which of the surfaces in the list match any given time aspect
    /// of the filter, and the overall existence map of the surveyed surfaces that match the filter.
    /// ComparisonList denotes a possibly pre-filtered set of surfaces for another filter; if this is the same as the 
    /// filtered set of surfaces then the overall existence map for those surfaces will not be computed as it is 
    /// assumed to be the same.
    /// </summary>
    public bool ProcessSurveyedSurfacesForFilter(Guid siteModelId,
      ICombinedFilter filter,
      ISurveyedSurfaces comparisonList,
      ISurveyedSurfaces filteredSurveyedSurfaces,
      ISubGridTreeBitMask overallExistenceMap)
    {
      // Filter out any surveyed surfaces which don't match current filter (if any) - realistically, this is time filters we're thinking of here
      FilterSurveyedSurfaceDetails(filter.AttributeFilter.HasTimeFilter,
        filter.AttributeFilter.StartTime, filter.AttributeFilter.EndTime,
        filter.AttributeFilter.ExcludeSurveyedSurfaces(),
        filteredSurveyedSurfaces,
        filter.AttributeFilter.SurveyedSurfaceExclusionList);

      if (filteredSurveyedSurfaces != null)
      {
        if (filteredSurveyedSurfaces.IsSameAs(comparisonList))
          return true;

        if (filteredSurveyedSurfaces.Count > 0)
        {
          var surveyedSurfaceExistenceMap = GetExistenceMaps().GetCombinedExistenceMap(siteModelId,
            filteredSurveyedSurfaces.Select(x => new Tuple<long, Guid>(Consts.EXISTENCE_SURVEYED_SURFACE_DESCRIPTOR, x.ID)).ToArray());

          if (overallExistenceMap == null)
            return false;

          overallExistenceMap.SetOp_OR(surveyedSurfaceExistenceMap);
        }
      }

      return true;
    }
  }
}
