using System.IO;
using FluentAssertions;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Types;
using VSS.TRex.SubGridTrees.Core.Utilities;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  public class ClientCellProfileAllPassesLeafSubgridTests
  {
    [Fact]
    public void Test_NullCells()
    {
      var cell = new ClientCellProfileAllPassesLeafSubgridRecord();
      cell.Clear();

      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.CellPasses) as ClientCellProfileAllPassesLeafSubgrid;
      SubGridUtilities.SubGridDimensionalIterator((x, y) => Assert.True(clientGrid.Cells[x, y].GetHashCode() == cell.GetHashCode()));
    }

    [Fact]
    public void Test_NullCell()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.CellPasses) as ClientCellProfileAllPassesLeafSubgrid;

      clientGrid.Cells[0, 0] = clientGrid.NullCell();
      Assert.False(clientGrid.CellHasValue(0, 0), "Cell not set to correct null value");
    }

    [Fact]
    public void ReadWriteBinary()
    {
      var clientGrid = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.CellPasses) as ClientCellProfileAllPassesLeafSubgrid;

      clientGrid.Cells[10, 10] = new ClientCellProfileAllPassesLeafSubgridRecord
      {
        TotalPasses = 2,
        CellPasses = new []
        {
          new ClientCellProfileLeafSubgridRecord
          {
            Height = 11.1f
          },
          new ClientCellProfileLeafSubgridRecord
          {
            Height = 12.2f
          }
        }
      };

      var writer = new BinaryWriter(new MemoryStream());
      clientGrid.Write(writer, null);

      (writer.BaseStream as MemoryStream).Position = 0;
      var clientGrid2 = ClientLeafSubGridFactoryFactory.CreateClientSubGridFactory().GetSubGrid(GridDataType.CellPasses) as ClientCellProfileAllPassesLeafSubgrid;

      clientGrid2.Read(new BinaryReader(writer.BaseStream as MemoryStream), null);

      clientGrid.Should().BeEquivalentTo(clientGrid2, options => options.ComparingByMembers<ClientCellProfileAllPassesLeafSubgridRecord>());
    }
  }
}
