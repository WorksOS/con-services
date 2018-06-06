using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Client
{
  /// <summary>
  /// The factory used to create the client subgrid creation factory. This abstracts the factory creation aspect away
  /// fromt he depednecy injection aspect.
  /// </summary>
  public static class ClientLeafSubgridFactoryFactory
  {
    /// <summary>
    /// Local instance variable for the singleton factory isntance that is provided to all callers
    /// </summary>
    private static IClientLeafSubgridFactory instance;

    /// <summary>
    /// Gets the subgrid client factory to use. Replace this with an implementation that 
    /// returns an appropriate element from the Dependency Injection container when this is implemented
    /// </summary>
    /// <returns></returns>
    public static IClientLeafSubgridFactory GetClientLeafSubGridFactory()
    {
      if (instance == null)
      {
        instance = new ClientLeafSubGridFactory();

        // Hardwiring registration of client data types here. May want to make this more dependency injection controlled....
        instance.RegisterClientLeafSubGridType(GridDataType.Height, typeof(ClientHeightLeafSubGrid));
        instance.RegisterClientLeafSubGridType(GridDataType.HeightAndTime, typeof(ClientHeightAndTimeLeafSubGrid));
        instance.RegisterClientLeafSubGridType(GridDataType.CompositeHeights, typeof(ClientCompositeHeightsLeafSubgrid));
        instance.RegisterClientLeafSubGridType(GridDataType.MachineSpeed, typeof(ClientMachineSpeedLeafSubGrid));
                instance.RegisterClientLeafSubGridType(GridDataType.MachineSpeedTarget, typeof(ClientMachineTargetSpeedLeafSubGrid));
        instance.RegisterClientLeafSubGridType(GridDataType.CCV, typeof(ClientCMVLeafSubGrid));
        instance.RegisterClientLeafSubGridType(GridDataType.MDP, typeof(ClientMDPLeafSubGrid));
        instance.RegisterClientLeafSubGridType(GridDataType.Temperature, typeof(ClientTemperatureLeafSubGrid));
        instance.RegisterClientLeafSubGridType(GridDataType.TemperatureDetail, typeof(ClientTemperatureLeafSubGrid));
      }

      return instance;
    }
  }
}
