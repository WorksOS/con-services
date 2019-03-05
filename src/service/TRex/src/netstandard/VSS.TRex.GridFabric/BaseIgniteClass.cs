using Apache.Ignite.Core;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;

namespace VSS.TRex.GridFabric
{
  public class BaseIgniteClass : IBinarizable, IFromToBinary
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<BaseIgniteClass>();

    /// <summary>
    /// Ignite instance.
    /// Note: This was previous an [InstanceResource] but this does not work well with more than one Grid active in the process
    /// </summary>
    protected IIgnite Ignite { get; private set; }

    /// <summary>
    /// The cluster group of nodes in the grid that are available for responding to design/profile requests
    /// </summary>
    private IClusterGroup _Group { get; set; }

    /// <summary>
    /// The compute interface from the cluster group projection
    /// </summary>
    protected ICompute Compute { get; private set; }

    public string Role { get; set; } = "";

    public string GridName { get; set; }

    /// <summary>
    /// Initializes the GridName and Role parameters and uses them to establish grid connectivity and compute projections
    /// </summary>
    /// <param name="gridName"></param>
    /// <param name="role"></param>
    private void InitialiseIgniteContext(string gridName, string role)
    {
      GridName = gridName;
      Role = role;

      AcquireIgniteTopologyProjections();
    }

    public BaseIgniteClass()
    {
    }

    /// <summary>
    /// Constructor that sets up cluster and compute projections available for use
    /// </summary>
    protected BaseIgniteClass(string gridName, string role)
    {
      InitialiseIgniteContext(gridName, role);
    }

    /// <summary>
    /// Acquires references to group and compute topology projections on the Ignite grid that may accept requests from this request
    /// </summary>
    public void AcquireIgniteTopologyProjections()
    {
      Log.LogInformation($"Acquiring TRex topology projections for grid {GridName}");

      if (string.IsNullOrEmpty(GridName))
        throw new TRexException("GridName name not defined when acquiring topology projection");

      if (string.IsNullOrEmpty(Role))
        throw new TRexException("Role name not defined when acquiring topology projection");

      Ignite = DIContext.Obtain<ITRexGridFactory>()?.Grid(GridName);

      if (Ignite == null)
        throw new TRexException("Ignite reference is null in AcquireIgniteTopologyProjections");

      _Group = Ignite?.GetCluster()?.ForAttribute($"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{Role}", "True");

      if (_Group == null)
        throw new TRexException($"Cluster group reference is null in AcquireIgniteTopologyProjections for role {Role} on grid {GridName}");

      if (_Group?.GetNodes()?.Count == 0)
        throw new TRexException($"Group cluster topology is empty for role {Role} on grid {GridName}");

      Compute = _Group?.GetCompute();

      if (Compute == null)
        throw new TRexException($"Compute projection is null in AcquireIgniteTopologyProjections on grid {GridName}");

      Log.LogInformation($"Completed acquisition of TRex topology projections for grid {GridName}");
    }

    public virtual void ToBinary(IBinaryRawWriter writer)
    {
      // No implementation for base class (and none may ever be required...)
    }

    public virtual void FromBinary(IBinaryRawReader reader)
    {
      // No implementation for base class (and none may ever be required...)
    }

    /// <summary>
    /// Implements the Ignite IBinarizable.WriteBinary interface Ignite will call to serialise this object.
    /// </summary>
    /// <param name="writer"></param>
    public void WriteBinary(IBinaryWriter writer) => ToBinary(writer.GetRawWriter());

    /// <summary>
    /// Implements the Ignite IBinarizable.ReadBinary interface Ignite will call to serialise this object.
    /// </summary>
    public void ReadBinary(IBinaryReader reader) => FromBinary(reader.GetRawReader());
  }
}
