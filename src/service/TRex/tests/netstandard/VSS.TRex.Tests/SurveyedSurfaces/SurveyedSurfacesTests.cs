using System;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Designs.Models;
using VSS.TRex.Geometry;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.Tests.BinaryReaderWriter;
using Xunit;

namespace VSS.TRex.Tests.SurveyedSurfaces
{
  public class SurveyedSurfacesTests
  {
    [Fact]
    public void Creation()
    {
      var ss = new TRex.SurveyedSurfaces.SurveyedSurfaces();
      ss.Should().NotBeNull();
      ss.Count.Should().Be(0);
    }

    [Fact]
    public void AddSurveyedSurfaceDetails()
    {
      var surveyedSurfaceUid = Guid.NewGuid();
      var designUid = Guid.NewGuid();
      var date = DateTime.UtcNow;
      var ss = new TRex.SurveyedSurfaces.SurveyedSurfaces();
      ss.AddSurveyedSurfaceDetails(surveyedSurfaceUid, new DesignDescriptor(designUid, "Folder", "FileName", 12.34), date, BoundingWorldExtent3D.Full());

      ss.Count.Should().Be(1);
      ss[0].Should().BeEquivalentTo(new SurveyedSurface(surveyedSurfaceUid, new DesignDescriptor(designUid, "Folder", "FileName", 12.34), date, BoundingWorldExtent3D.Full()));
    }

    [Fact]
    public void RemoveSurveyedSurfaceDetails()
    {
      var surveyedSurfaceUid = Guid.NewGuid();
      var designUid = Guid.NewGuid();
      var ss = new TRex.SurveyedSurfaces.SurveyedSurfaces();
      ss.AddSurveyedSurfaceDetails(surveyedSurfaceUid, new DesignDescriptor(designUid, "Folder", "FileName", 12.34), DateTime.UtcNow, BoundingWorldExtent3D.Full());

      ss.Count.Should().Be(1);

      ss.RemoveSurveyedSurface(surveyedSurfaceUid).Should().BeTrue();
      ss.Count.Should().Be(0);
    }

    [Fact]
    public void RemoveSurveyedSurfaceDetails_EmptyList()
    {
      var ss = new TRex.SurveyedSurfaces.SurveyedSurfaces();
      ss.RemoveSurveyedSurface(Guid.NewGuid()).Should().BeFalse();
    }

    [Fact]
    public void SortChronologically()
    {
      var surveyedSurfaceUid1 = Guid.NewGuid();
      var surveyedSurfaceUid2 = Guid.NewGuid();
      var designUid = Guid.NewGuid();
      var ss = new TRex.SurveyedSurfaces.SurveyedSurfaces();
      var date = DateTime.UtcNow;
      ss.AddSurveyedSurfaceDetails(surveyedSurfaceUid1, new DesignDescriptor(designUid, "Folder", "FileName", 12.34), date, BoundingWorldExtent3D.Full());
      ss.AddSurveyedSurfaceDetails(surveyedSurfaceUid2, new DesignDescriptor(designUid, "Folder", "FileName", 12.34), date.AddMinutes(1), BoundingWorldExtent3D.Full());

      ss.SortChronologically(true);

      ss[0].AsAtDate.Should().Be(date.AddMinutes(1));
      ss[1].AsAtDate.Should().Be(date);

      ss.SortChronologically(false);

      ss[0].AsAtDate.Should().Be(date);
      ss[1].AsAtDate.Should().Be(date.AddMinutes(1));
    }

    [Fact]
    public void BinaryReadWrite()
    {
      var ss = new TRex.SurveyedSurfaces.SurveyedSurfaces();
      ss.AddSurveyedSurfaceDetails(Guid.NewGuid(), new DesignDescriptor(Guid.NewGuid(), "Folder", "FileName", 12.34), DateTime.UtcNow, BoundingWorldExtent3D.Full());

      TestBinary_ReaderWriterHelper.RoundTripSerialise(ss);
    }

    private TRex.SurveyedSurfaces.SurveyedSurfaces MakeSurveyedSurfacesSet(DateTime date)
    {
      var designUid = Guid.NewGuid();
      var ss = new TRex.SurveyedSurfaces.SurveyedSurfaces();

      ss.AddSurveyedSurfaceDetails(Guid.NewGuid(), new DesignDescriptor(designUid, "Folder", "FileName", 12.34), date, BoundingWorldExtent3D.Full());
      ss.AddSurveyedSurfaceDetails(Guid.NewGuid(), new DesignDescriptor(designUid, "Folder", "FileName", 12.34), date.AddMinutes(1), BoundingWorldExtent3D.Full());
      ss.AddSurveyedSurfaceDetails(Guid.NewGuid(), new DesignDescriptor(designUid, "Folder", "FileName", 12.34), date.AddMinutes(2), BoundingWorldExtent3D.Full());
      ss.AddSurveyedSurfaceDetails(Guid.NewGuid(), new DesignDescriptor(designUid, "Folder", "FileName", 12.34), date.AddMinutes(3), BoundingWorldExtent3D.Full());
      ss.AddSurveyedSurfaceDetails(Guid.NewGuid(), new DesignDescriptor(designUid, "Folder", "FileName", 12.34), date.AddMinutes(4), BoundingWorldExtent3D.Full());

      return ss;
    }

    [Fact]
    public void FilterSurveyedSurfaceDetails_NoFilter_ExludeSurfaces()
    {
      var date = DateTime.UtcNow;
      var ss = MakeSurveyedSurfacesSet(date);

      var filtered = new TRex.SurveyedSurfaces.SurveyedSurfaces();

      ss.FilterSurveyedSurfaceDetails(false, DateTime.MinValue, DateTime.MinValue, true, filtered, new Guid[0]);

      filtered.Count.Should().Be(0);
    }

    [Fact]
    public void FilterSurveyedSurfaceDetails_NoFilter_NoExclusions()
    {
      var date = DateTime.UtcNow;
      var ss = MakeSurveyedSurfacesSet(date);

      var filtered = new TRex.SurveyedSurfaces.SurveyedSurfaces();

      ss.FilterSurveyedSurfaceDetails(false, DateTime.MinValue, DateTime.MinValue, false, filtered, new Guid[0]);

      filtered.Should().BeEquivalentTo(ss);
    }

    [Fact]
    public void FilterSurveyedSurfaceDetails_WithTimeFilter_AllOfTime_NoExclusions()
    {
      var date = DateTime.UtcNow;
      var ss = MakeSurveyedSurfacesSet(date);

      var filtered = new TRex.SurveyedSurfaces.SurveyedSurfaces();

      ss.FilterSurveyedSurfaceDetails(true, DateTime.MinValue, DateTime.MaxValue, false, filtered, new Guid[0]);

      filtered.Should().BeEquivalentTo(ss);
    }

    [Fact]
    public void FilterSurveyedSurfaceDetails_WithTimeFilter_RestrictedTime_NoExclusions()
    {
      var date = DateTime.UtcNow;
      var ss = MakeSurveyedSurfacesSet(date);

      var filtered = new TRex.SurveyedSurfaces.SurveyedSurfaces();

      // Include the first two surveyed surfaces in the list
      ss.FilterSurveyedSurfaceDetails(true, date, date.AddMinutes(1), false, filtered, new Guid[0]);

      filtered.Count.Should().Be(2);
      filtered.Select(x => x.AsAtDate).Should().BeEquivalentTo(new []{date, date.AddMinutes(1)});
    }

    [Fact]
    public void FilterSurveyedSurfaceDetails_WithTimeFilter_AllOfTime_WithExclusions()
    {
      var date = DateTime.UtcNow;
      var ss = MakeSurveyedSurfacesSet(date);

      var filtered = new TRex.SurveyedSurfaces.SurveyedSurfaces();

      // Exclude all surveyed surfaces after first two
      ss.FilterSurveyedSurfaceDetails(true, DateTime.MinValue, DateTime.MaxValue, false, filtered, 
        ss.Skip(2).Select(x => x.ID).ToArray());

      filtered.Count.Should().Be(2);
      filtered.Should().BeEquivalentTo(ss.Take(2));
    }

    [Fact]
    public void FilterSurveyedSurfaceDetails_WithTimeFilter_RestrictedTime_WithExclusions_Overlapped()
    {
      var date = DateTime.UtcNow;
      var ss = MakeSurveyedSurfacesSet(date);

      var filtered = new TRex.SurveyedSurfaces.SurveyedSurfaces();

      // Include the first two surveyed surfaces in the list, but exclude the first one
      ss.FilterSurveyedSurfaceDetails(true, date, date.AddMinutes(1), false, filtered, new []{ss[0].ID});

      filtered.Count.Should().Be(1);
      filtered[0].Should().Be(ss[1]);
    }

    [Fact]
    public void FilterSurveyedSurfaceDetails_WithTimeFilter_RestrictedTime_WithExclusions_NonOverlapped()
    {
      var date = DateTime.UtcNow;
      var ss = MakeSurveyedSurfacesSet(date);

      var filtered = new TRex.SurveyedSurfaces.SurveyedSurfaces();

      // Include the first two surveyed surfaces in the list, and exclude the last one in the surveyed surfaces list
      ss.FilterSurveyedSurfaceDetails(true, date, date.AddMinutes(1), false, filtered, new[] { ss.Last().ID });

      filtered.Count.Should().Be(2);
      filtered.Should().BeEquivalentTo(ss.Take(2));
    }

    [Fact]
    public void FilterSurveyedSurfaceDetails_NoTimeFilter_WithExclusions_NonOverlapped()
    {
      var date = DateTime.UtcNow;
      var ss = MakeSurveyedSurfacesSet(date);

      var filtered = new TRex.SurveyedSurfaces.SurveyedSurfaces();

      // Include the first two surveyed surfaces in the list, and exclude the last one in the surveyed surfaces list
      ss.FilterSurveyedSurfaceDetails(false, DateTime.MinValue, DateTime.MinValue, false, filtered, new[] { ss.Last().ID });

      filtered.Count.Should().Be(ss.Count - 1);
      filtered.Should().BeEquivalentTo(ss.Take(ss.Count - 1));
    }

    /*
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
    }     */

    [Fact]
    public void HasSurfaceEarlierThan_Long()
    {
      var date = DateTime.UtcNow;
      var ss = MakeSurveyedSurfacesSet(date);

      ss.HasSurfaceEarlierThan(date.Ticks).Should().BeFalse();
      ss.HasSurfaceEarlierThan(date.Ticks + 1).Should().BeTrue();
    }

    [Fact]
    public void HasSurfaceEarlierThan_DateTime()
    {
      var date = DateTime.UtcNow;
      var ss = MakeSurveyedSurfacesSet(date);

      ss.HasSurfaceEarlierThan(date).Should().BeFalse();
      ss.HasSurfaceEarlierThan(date.AddTicks(1)).Should().BeTrue();
    }

    [Fact]
    public void HasSurfaceLaterThan_Long()
    {
      var date = DateTime.UtcNow;
      var ss = MakeSurveyedSurfacesSet(date);

      ss.HasSurfaceLaterThan(date.AddMinutes(4).Ticks).Should().BeFalse();
      ss.HasSurfaceLaterThan(date.AddMinutes(4).Ticks - 1).Should().BeTrue();
    }

    [Fact]
    public void HasSurfaceLaterThan_DateTime()
    {
      var date = DateTime.UtcNow;
      var ss = MakeSurveyedSurfacesSet(date);

      ss.HasSurfaceLaterThan(date.AddMinutes(4)).Should().BeFalse();
      ss.HasSurfaceLaterThan(date.AddMinutes(4).AddTicks(-1)).Should().BeTrue();
    }

    [Fact]
    public void Locate()
    {
      var date = DateTime.UtcNow;
      var ss = MakeSurveyedSurfacesSet(date);

      ss.All(x => ss.Locate(x.ID) == x).Should().BeTrue();
    }
  }
}
