using System;
using System.Linq;
using FluentAssertions;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SurveyedSurfaces;
using VSS.TRex.Tests.BinaryReaderWriter;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SurveyedSurfaces
{
  public class SurveyedSurfacesTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private TRex.SurveyedSurfaces.SurveyedSurfaces MakeSurveyedSurfacesSet(DateTime date, Guid[] guids = null)
    {
      var designUid = Guid.NewGuid();
      var ss = new TRex.SurveyedSurfaces.SurveyedSurfaces();

      ss.AddSurveyedSurfaceDetails(guids?[0] ?? Guid.NewGuid(), new DesignDescriptor(designUid, "Folder", "FileName"), date, BoundingWorldExtent3D.Full());
      ss.AddSurveyedSurfaceDetails(guids?[1] ?? Guid.NewGuid(), new DesignDescriptor(designUid, "Folder", "FileName"), date.AddMinutes(1), BoundingWorldExtent3D.Full());
      ss.AddSurveyedSurfaceDetails(guids?[2] ?? Guid.NewGuid(), new DesignDescriptor(designUid, "Folder", "FileName"), date.AddMinutes(2), BoundingWorldExtent3D.Full());
      ss.AddSurveyedSurfaceDetails(guids?[3] ?? Guid.NewGuid(), new DesignDescriptor(designUid, "Folder", "FileName"), date.AddMinutes(3), BoundingWorldExtent3D.Full());
      ss.AddSurveyedSurfaceDetails(guids?[4] ?? Guid.NewGuid(), new DesignDescriptor(designUid, "Folder", "FileName"), date.AddMinutes(4), BoundingWorldExtent3D.Full());

      return ss;
    }

    private void MakeSurveyedSurfacesSetInSiteModel(DateTime date, ISiteModel siteModel)
    {
      var designUid = Guid.NewGuid();

      siteModel.SurveyedSurfaces.AddSurveyedSurfaceDetails(Guid.NewGuid(), new DesignDescriptor(designUid, "Folder", "FileName"), date, BoundingWorldExtent3D.Full());
      siteModel.SurveyedSurfaces.AddSurveyedSurfaceDetails(Guid.NewGuid(), new DesignDescriptor(designUid, "Folder", "FileName"), date.AddMinutes(1), BoundingWorldExtent3D.Full());
      siteModel.SurveyedSurfaces.AddSurveyedSurfaceDetails(Guid.NewGuid(), new DesignDescriptor(designUid, "Folder", "FileName"), date.AddMinutes(2), BoundingWorldExtent3D.Full());
      siteModel.SurveyedSurfaces.AddSurveyedSurfaceDetails(Guid.NewGuid(), new DesignDescriptor(designUid, "Folder", "FileName"), date.AddMinutes(3), BoundingWorldExtent3D.Full());
      siteModel.SurveyedSurfaces.AddSurveyedSurfaceDetails(Guid.NewGuid(), new DesignDescriptor(designUid, "Folder", "FileName"), date.AddMinutes(4), BoundingWorldExtent3D.Full());
    }
    
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
      ss.AddSurveyedSurfaceDetails(surveyedSurfaceUid, new DesignDescriptor(designUid, "Folder", "FileName"), date, BoundingWorldExtent3D.Full());

      ss.Count.Should().Be(1);
      ss[0].Should().BeEquivalentTo(new SurveyedSurface(surveyedSurfaceUid, new DesignDescriptor(designUid, "Folder", "FileName"), date, BoundingWorldExtent3D.Full()));
    }

    [Fact]
    public void RemoveSurveyedSurfaceDetails()
    {
      var surveyedSurfaceUid = Guid.NewGuid();
      var designUid = Guid.NewGuid();
      var ss = new TRex.SurveyedSurfaces.SurveyedSurfaces();
      ss.AddSurveyedSurfaceDetails(surveyedSurfaceUid, new DesignDescriptor(designUid, "Folder", "FileName"), DateTime.UtcNow, BoundingWorldExtent3D.Full());

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
      ss.AddSurveyedSurfaceDetails(surveyedSurfaceUid1, new DesignDescriptor(designUid, "Folder", "FileName"), date, BoundingWorldExtent3D.Full());
      ss.AddSurveyedSurfaceDetails(surveyedSurfaceUid2, new DesignDescriptor(designUid, "Folder", "FileName"), date.AddMinutes(1), BoundingWorldExtent3D.Full());

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
      ss.AddSurveyedSurfaceDetails(Guid.NewGuid(), new DesignDescriptor(Guid.NewGuid(), "Folder", "FileName"), DateTime.UtcNow, BoundingWorldExtent3D.Full());

      TestBinary_ReaderWriterHelper.RoundTripSerialise(ss);
    }

    [Fact]
    public void FilterSurveyedSurfaceDetails_NoFilter_ExludeSurfaces()
    {
      var date = DateTime.UtcNow;
      var ss = MakeSurveyedSurfacesSet(date);

      var filtered = new TRex.SurveyedSurfaces.SurveyedSurfaces();

      ss.FilterSurveyedSurfaceDetails(false, Consts.MIN_DATETIME_AS_UTC, Consts.MIN_DATETIME_AS_UTC, true, filtered, new Guid[0]);

      filtered.Count.Should().Be(0);
    }

    [Fact]
    public void FilterSurveyedSurfaceDetails_NoFilter_NoExclusions()
    {
      var date = DateTime.UtcNow;
      var ss = MakeSurveyedSurfacesSet(date);

      var filtered = new TRex.SurveyedSurfaces.SurveyedSurfaces();

      ss.FilterSurveyedSurfaceDetails(false, Consts.MIN_DATETIME_AS_UTC, Consts.MIN_DATETIME_AS_UTC, false, filtered, new Guid[0]);

      filtered.Should().BeEquivalentTo(ss);
    }

    [Fact]
    public void FilterSurveyedSurfaceDetails_FailWithNonUTCDates()
    {
      var date = DateTime.UtcNow;
      var ss = MakeSurveyedSurfacesSet(date);

      var filtered = new TRex.SurveyedSurfaces.SurveyedSurfaces();

      Action act = () => ss.FilterSurveyedSurfaceDetails(true, DateTime.Now, DateTime.Now, false, filtered, new Guid[0]);
      act.Should().Throw<ArgumentException>().WithMessage("StartTime and EndTime must be UTC date times");
    }

    [Fact]
    public void FilterSurveyedSurfaceDetails_WithTimeFilter_AllOfTime_NoExclusions()
    {
      var date = DateTime.UtcNow;
      var ss = MakeSurveyedSurfacesSet(date);

      var filtered = new TRex.SurveyedSurfaces.SurveyedSurfaces();

      ss.FilterSurveyedSurfaceDetails(true, Consts.MIN_DATETIME_AS_UTC, Consts.MAX_DATETIME_AS_UTC, false, filtered, new Guid[0]);

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
      ss.FilterSurveyedSurfaceDetails(true, Consts.MIN_DATETIME_AS_UTC, Consts.MAX_DATETIME_AS_UTC, false, filtered, 
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
      ss.FilterSurveyedSurfaceDetails(false, Consts.MIN_DATETIME_AS_UTC, Consts.MIN_DATETIME_AS_UTC, false, filtered, new[] { ss.Last().ID });

      filtered.Count.Should().Be(ss.Count - 1);
      filtered.Should().BeEquivalentTo(ss.Take(ss.Count - 1));
    }

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
    public void Locate_Success()
    {
      var date = DateTime.UtcNow;
      var ss = MakeSurveyedSurfacesSet(date);

      ss.All(x => ss.Locate(x.ID) == x).Should().BeTrue();
    }

    [Fact]
    public void Locate_FailWithNotExist()
    {
      var date = DateTime.UtcNow;
      var ss = MakeSurveyedSurfacesSet(date);

      ss.Locate(Guid.NewGuid()).Should().BeNull();
    }

    [Fact]
    public void ProcessSurveyedSurfacesForFilter_WithOutTimeFilter_WithoutExistenceMap()
    {
      var date = DateTime.UtcNow;
      var filteredList = new TRex.SurveyedSurfaces.SurveyedSurfaces();
      var comparisonList = new TRex.SurveyedSurfaces.SurveyedSurfaces();

      var filter = new CombinedFilter();
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      MakeSurveyedSurfacesSetInSiteModel(date, siteModel);

      var result = siteModel.SurveyedSurfaces.ProcessSurveyedSurfacesForFilter(siteModel.ID,  filter, comparisonList, filteredList, null);
      result.Should().BeFalse();
    }

    [Fact]
    public void ProcessSurveyedSurfacesForFilter_WithOutTimeFilter_WithExistenceMap()
    {
      var date = DateTime.UtcNow;
      var filteredList = new TRex.SurveyedSurfaces.SurveyedSurfaces();
      var comparisonList = new TRex.SurveyedSurfaces.SurveyedSurfaces();

      var filter = new CombinedFilter();
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      MakeSurveyedSurfacesSetInSiteModel(date, siteModel);

      var result = siteModel.SurveyedSurfaces.ProcessSurveyedSurfacesForFilter(siteModel.ID, filter, comparisonList, filteredList, siteModel.ExistenceMap);
      result.Should().BeTrue();
      filteredList.Should().BeEquivalentTo(siteModel.SurveyedSurfaces);
    }

    [Fact]
    public void ProcessSurveyedSurfacesForFilter_WithoutTimeFilter_WithExistenceMap_WithMatchingComparisonList()
    {
      var date = DateTime.UtcNow;
      var filteredList = new TRex.SurveyedSurfaces.SurveyedSurfaces();
      var filter = new CombinedFilter();
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();

      MakeSurveyedSurfacesSetInSiteModel(date, siteModel);

      var result = siteModel.SurveyedSurfaces.ProcessSurveyedSurfacesForFilter(siteModel.ID, filter, siteModel.SurveyedSurfaces, filteredList, siteModel.ExistenceMap);
      result.Should().BeTrue();
      filteredList.Should().BeEquivalentTo(siteModel.SurveyedSurfaces);
    }

    [Fact]
    public void ProcessSurveyedSurfacesForFilter_WithTimeFilter()
    {
      var date = DateTime.UtcNow;
      var filteredList = new TRex.SurveyedSurfaces.SurveyedSurfaces();
      var filter = new CombinedFilter
      {
        AttributeFilter =
        {
          HasTimeFilter = true,
          StartTime = date,
          EndTime = date.AddMinutes(1) // Select two of the surveyed surfaces
        }
      };

      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      MakeSurveyedSurfacesSetInSiteModel(date, siteModel);

      var result = siteModel.SurveyedSurfaces.ProcessSurveyedSurfacesForFilter(siteModel.ID, filter, siteModel.SurveyedSurfaces, filteredList, siteModel.ExistenceMap);
      result.Should().BeTrue();
      filteredList.Count().Should().Be(2);
    }

    [Fact]
    public void IsSameAs()
    {
      var date = DateTime.UtcNow;
      var guids = Enumerable.Range(0, 5).Select(x => Guid.NewGuid()).ToArray();

      var ss = MakeSurveyedSurfacesSet(date, guids);
      ss.IsSameAs(ss).Should().BeTrue();

      var ss2 = MakeSurveyedSurfacesSet(date, guids);
      ss.IsSameAs(ss2).Should().BeTrue();

      var ss3 = MakeSurveyedSurfacesSet(date);
      ss.IsSameAs(ss3).Should().BeFalse();

      var ss4 = new TRex.SurveyedSurfaces.SurveyedSurfaces();
      ss4.AddSurveyedSurfaceDetails(Guid.NewGuid(), new DesignDescriptor(Guid.NewGuid(), "Folder", "FileName"), date, BoundingWorldExtent3D.Full());
      ss.IsSameAs(ss4).Should().BeFalse();
    }
  }
}
