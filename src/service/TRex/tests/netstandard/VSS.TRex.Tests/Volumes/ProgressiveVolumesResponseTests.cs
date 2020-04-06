using System;
using FluentAssertions;
using VSS.TRex.Types;
using VSS.TRex.Volumes.GridFabric.Responses;
using Xunit;

namespace VSS.TRex.Tests.Volumes
{
  public class ProgressiveVolumesResponseTests
  {
    [Fact]
    public void Creation()
    {
      var response = new ProgressiveVolumesResponse();

      Assert.NotNull(response);
    }

    [Fact]
    public void AggregateWith()
    {
      var date = DateTime.UtcNow;

      var response1 = new ProgressiveVolumesResponse
      {
        ResultStatus = RequestErrorStatus.OK,
        Volumes = new[]
        {
          new ProgressiveVolumeResponseItem
          {
            Date = date, 
            Volume = new SimpleVolumesResponse()
          }
        }
      };
      var response2 = new ProgressiveVolumesResponse
      {
        ResultStatus = RequestErrorStatus.OK,
        Volumes = new[]
        {
          new ProgressiveVolumeResponseItem
          {
            Date = date,
            Volume = new SimpleVolumesResponse
            {
              Cut = 10.0,
              Fill = 20.0,
              BoundingExtentGrid = new TRex.Geometry.BoundingWorldExtent3D(1.0, 2.0, 3.0, 4.0, 5.0, 6.0),
              CutArea = 30.0,
              FillArea = 40.0,
              TotalCoverageArea = 100.0
            }
          }
        }
      };

      response1.AggregateWith(response2);

      response2.Should().BeEquivalentTo(response1);
    }

    [Fact]
    public void AggregateWith_FailWithUnequalListSizes()
    {
      // Allow this to be null, it will receive all the aggregations
      var response1 = new ProgressiveVolumesResponse();
      var response2 = new ProgressiveVolumesResponse
      {
        ResultStatus = RequestErrorStatus.OK,
        Volumes = new[]
        {
          new ProgressiveVolumeResponseItem()
        }
      };

      Action act = () => response1.AggregateWith(response2);
      act.Should().Throw<ArgumentException>().WithMessage("Progressive volumes series should have same length*");
    }

    [Fact]
    public void AggregateWith_FailWithUnequalDates()
    {
      // Allow this to be null, it will receive all the aggregations
      var response1 = new ProgressiveVolumesResponse
      {
        ResultStatus = RequestErrorStatus.OK,
        Volumes = new[]
        {
          new ProgressiveVolumeResponseItem
          {
            Date = DateTime.UtcNow,
            Volume = new SimpleVolumesResponse()
          }
        }
      };

      var response2 = new ProgressiveVolumesResponse
      {
        ResultStatus = RequestErrorStatus.OK,
        Volumes = new[]
        {
          new ProgressiveVolumeResponseItem
          {
            Date = DateTime.UtcNow.AddMinutes(1),
            Volume = new SimpleVolumesResponse()
          }
        }
      };

      Action act = () => response1.AggregateWith(response2);
      act.Should().Throw<ArgumentException>().WithMessage("Dates of aggregating progressive volume pair are not the same*");
    }
  }
}
