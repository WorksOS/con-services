using System;
using FluentAssertions;
using VSS.TRex.Common.Models;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.Tests.TestFixtures;
using VSS.TRex.Types;
using VSS.TRex.Volumes;
using Xunit;

namespace VSS.TRex.Tests.Volumes
{
  public class ProgressiveVolumesSubGridRetrieverTests : IClassFixture<DITAGFileAndSubGridRequestsWithIgniteFixture>
  {
    private ProgressiveVolumesSubGridRetriever MakeANewRetriever(ISiteModel siteModel)
    {
      var retriever = new ProgressiveVolumesSubGridRetriever
      (siteModel,
        GridDataType.ProgressiveVolumes, 
        siteModel.PrimaryStorageProxy,
        new CombinedFilter(),
        new CellPassAttributeFilterProcessingAnnex(),
        false,
        BoundingIntegerExtent2D.Inverted(),
        true,
        int.MaxValue,
        new AreaControlSet(),
        new FilteredValuePopulationControl(),
        new SubGridTreeBitMask(),
        new OverrideParameters(),
        new LiftParameters()
      );

      retriever.StartDate = new DateTime(2020, 1, 1, 0, 0, 0);
      retriever.EndDate = new DateTime(2020, 1, 1, 1, 0, 0);
      retriever.Interval = new TimeSpan(0, 10, 0);

      return retriever;
    }

    [Fact]
    public void Creation()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var retriever = MakeANewRetriever(siteModel);

      retriever.Should().NotBeNull();
    }

    [Fact]
    public void RetrieveSubGrid_FailWithSubGridNotFound()
    {
      var siteModel = DITAGFileAndSubGridRequestsWithIgniteFixture.NewEmptyModel();
      var retriever = MakeANewRetriever(siteModel);

      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.ProgressiveVolumes);

      var result = retriever.RetrieveSubGrid(clientGrid, SubGridTreeBitmapSubGridBits.FullMask);

      result.Should().Be(ServerRequestResult.SubGridNotFound);
    }
  }
}
