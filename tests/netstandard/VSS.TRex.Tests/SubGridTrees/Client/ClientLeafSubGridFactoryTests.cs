using System;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.SubGridTrees.Client.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees.Client
{
  public class ClientLeafSubGridFactoryTests
  {
    [Fact]
    public void Test_ClientLeafSubGridFactoryTests_Creation()
    {
      var factory = new ClientLeafSubGridFactory();
      Assert.NotNull(factory);
    }

    [Fact]
    public void Test_ClientLeafSubGridFactoryTests_RegisterClientLeafSubGridType()
    {
      var factory = new ClientLeafSubGridFactory();

      factory.RegisterClientLeafSubGridType(GridDataType.Height, typeof(ClientHeightLeafSubGrid));

      var newClient = factory.GetSubGrid(GridDataType.Height);

      Assert.NotNull(newClient);
      Assert.True(newClient is ClientHeightLeafSubGrid, "Returned subgrid unexpected type");
    }

    [Fact]
    public void Test_ClientLeafSubGridFactoryTests_GetSubGrid()
    {
      var factory = new ClientLeafSubGridFactory();

      factory.RegisterClientLeafSubGridType(GridDataType.Height, typeof(ClientHeightLeafSubGrid));

      var newClient = factory.GetSubGrid(GridDataType.Height);

      Assert.NotNull(newClient);
      Assert.True(newClient is ClientHeightLeafSubGrid, "Returned subgrid unexpected type");
    }

    [Fact]
    public void Test_ClientLeafSubGridFactoryTests_ReturnClientGrid()
    {
      var factory = new ClientLeafSubGridFactory();

      factory.RegisterClientLeafSubGridType(GridDataType.Height, typeof(ClientHeightLeafSubGrid));

      var newClient = factory.GetSubGrid(GridDataType.Height);
      factory.ReturnClientSubGrid(ref newClient);

      Assert.True(null == newClient, "Factory did not accept subgrid back");

      // Attempt repatriating it twice - nothign should happen
      factory.ReturnClientSubGrid(ref newClient);
    }

    [Fact]
    public void Test_ClientLeafSubGridFactoryTests_ReturnClientGrids1()
    {
      var factory = new ClientLeafSubGridFactory();

      factory.RegisterClientLeafSubGridType(GridDataType.Height, typeof(ClientHeightLeafSubGrid));

      IClientLeafSubGrid[] newClients = new []
      {
        factory.GetSubGrid(GridDataType.Height),
        factory.GetSubGrid(GridDataType.Height)
      };
      factory.ReturnClientSubGrids(newClients, newClients.Length);

      Assert.True(null == newClients[0], "Factory did not accept first subgrid back");
      Assert.True(null == newClients[1], "Factory did not accept second subgrid back");

      Assert.Throws<ArgumentException>(() => factory.ReturnClientSubGrids(newClients, 10));

      // Attempt repatriating it twice - nothign should happen
      factory.ReturnClientSubGrids(newClients, newClients.Length);
    }

    [Fact]
    public void Test_ClientLeafSubGridFactoryTests_ReturnClientGrids2()
    {
      var factory = new ClientLeafSubGridFactory();

      factory.RegisterClientLeafSubGridType(GridDataType.Height, typeof(ClientHeightLeafSubGrid));

      IClientLeafSubGrid[][] newClients = new[]
      {
        new[] {factory.GetSubGrid(GridDataType.Height), factory.GetSubGrid(GridDataType.Height)},
        new[] {factory.GetSubGrid(GridDataType.Height), factory.GetSubGrid(GridDataType.Height)}
      };

      factory.ReturnClientSubGrids(newClients, newClients.Length);

      Assert.True(null == newClients[0][0], "Factory did not accept first/first subgrid back");
      Assert.True(null == newClients[0][1], "Factory did not accept first/second subgrid back");

      Assert.True(null == newClients[1][0], "Factory did not accept second/first subgrid back");
      Assert.True(null == newClients[1][1], "Factory did not accept second/second subgrid back");

      Assert.Throws<ArgumentException>(() => factory.ReturnClientSubGrids(newClients, 10));

      // Attempt repatriating it twice - nothign should happen
      factory.ReturnClientSubGrids(newClients, newClients.Length);
    }
  }
}
