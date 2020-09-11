using System.Collections.Generic;
using System.Threading.Tasks;
using CoreX.Interfaces;
using CoreX.Types;
using CoreX.Wrapper.UnitTests.Types;
using CoreXModels;
using Xunit;

namespace CoreX.Wrapper.UnitTests.Tests
{
  public class ThreadingTests : IClassFixture<UnitTestBaseFixture>
  {
    private readonly ICoreXWrapper _convertCoordinates;

    public ThreadingTests(UnitTestBaseFixture testFixture)
    {
      _convertCoordinates = testFixture.CoreXWrapper;
    }

    [Fact]
    public void LLHToNEE_should_handle_many_concurrent_threads()
    {
      var inputs = new List<LLH[]>
      {
         new [] {
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 },
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 },
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 },
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 }
        },
        new [] {
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 },
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 },
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 },
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 }
        },
        new [] {
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 },
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 },
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 },
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 }
        },
        new [] {
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 },
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 },
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 },
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 }
        },
        new [] {
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 },
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 },
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 },
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 }
        },
        new [] {
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 },
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 },
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 },
          new LLH { Latitude = 0000, Longitude = 0000, Height = 0000 }
        }
      };

      Parallel.ForEach(inputs, (llh) =>
      {
        _ = _convertCoordinates.LLHToNEE(CSIB.DIMENSIONS_2012_WITHOUT_VERT_ADJUST, llh, InputAs.Radians);
      });
    }

    [Fact]
    public void NEEToLLH_should_handle_many_concurrent_threads()
    {
      var inputs = new List<NEE[]>
      {
         new [] {
          new NEE { North = 0000, East = 0000, Elevation = 0000 },
          new NEE { North = 0000, East = 0000, Elevation = 0000 },
          new NEE { North = 0000, East = 0000, Elevation = 0000 },
          new NEE { North = 0000, East = 0000, Elevation = 0000 }
        },
        new [] {
          new NEE { North = 0000, East = 0000, Elevation = 0000 },
          new NEE { North = 0000, East = 0000, Elevation = 0000 },
          new NEE { North = 0000, East = 0000, Elevation = 0000 },
          new NEE { North = 0000, East = 0000, Elevation = 0000 }
        },
        new [] {
          new NEE { North = 0000, East = 0000, Elevation = 0000 },
          new NEE { North = 0000, East = 0000, Elevation = 0000 },
          new NEE { North = 0000, East = 0000, Elevation = 0000 },
          new NEE { North = 0000, East = 0000, Elevation = 0000 }
        },
        new [] {
          new NEE { North = 0000, East = 0000, Elevation = 0000 },
          new NEE { North = 0000, East = 0000, Elevation = 0000 },
          new NEE { North = 0000, East = 0000, Elevation = 0000 },
          new NEE { North = 0000, East = 0000, Elevation = 0000 }
        },
        new [] {
          new NEE { North = 0000, East = 0000, Elevation = 0000 },
          new NEE { North = 0000, East = 0000, Elevation = 0000 },
          new NEE { North = 0000, East = 0000, Elevation = 0000 },
          new NEE { North = 0000, East = 0000, Elevation = 0000 }
        },
        new [] {
          new NEE { North = 0000, East = 0000, Elevation = 0000 },
          new NEE { North = 0000, East = 0000, Elevation = 0000 },
          new NEE { North = 0000, East = 0000, Elevation = 0000 },
          new NEE { North = 0000, East = 0000, Elevation = 0000 }
        }
      };

      Parallel.ForEach(inputs, (nee) =>
      {
        _ = _convertCoordinates.NEEToLLH(CSIB.DIMENSIONS_2012_WITHOUT_VERT_ADJUST, nee);
      });
    }
  }
}
