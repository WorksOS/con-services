using Apache.Ignite.Core;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using Apache.Ignite.Core.Binary;
using VSS.Serilog.Extensions;
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

    private IIgnite _ignite;
    /// <summary>
    /// Ignite instance.
    /// Note: This was previous an [InstanceResource] but this does not work well with more than one Grid active in the process
    /// </summary>
    protected IIgnite Ignite { get => _ignite; private set => _ignite = value; }

    /// <summary>
    /// The cluster group of nodes in the grid that are available for responding to design/profile requests
    /// </summary>
    private IClusterGroup _Group;

    private ICompute _compute;
    /// <summary>
    /// The compute interface from the cluster group projection
    /// </summary>
    protected ICompute Compute { get => _compute; private set => _compute = value; }

    private readonly ITRexGridFactory _tRexGridFactory = DIContext.Obtain<ITRexGridFactory>();

    private string _role = string.Empty;
    public string Role { get => _role; }

    private string _gridName;
    public string GridName { get => _gridName; }

    private string _roleAttribute;

    /// <summary>
    /// Initializes the GridName and Role parameters and uses them to establish grid connectivity and compute projections
    /// </summary>
    /// <param name="gridName"></param>
    /// <param name="role"></param>
    private void InitialiseIgniteContext(string gridName, string role)
    {
      _gridName = gridName;
      _role = role;

      _roleAttribute = $"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{_role}";

      AcquireIgniteTopologyProjections();
    }

    public BaseIgniteClass()
    {
    }

    /// <summary>
    /// Constructor that sets up cluster and compute projections available for use
    /// </summary>
    public BaseIgniteClass(string gridName, string role)
    {
      InitialiseIgniteContext(gridName, role);
    }

    /// <summary>
    /// Acquires references to group and compute topology projections on the Ignite grid that may accept requests from this request
    /// </summary>
    public void AcquireIgniteTopologyProjections()
    {
      if (Log.IsTraceEnabled())
        Log.LogTrace($"Acquiring TRex topology projections for grid {_gridName}");

      if (string.IsNullOrEmpty(_gridName))
        throw new TRexException("GridName name not defined when acquiring topology projection");

      if (string.IsNullOrEmpty(_role))
        throw new TRexException("Role name not defined when acquiring topology projection");

      _ignite = _tRexGridFactory?.Grid(_gridName);

      if (_ignite == null)
        throw new TRexException("Ignite reference is null in AcquireIgniteTopologyProjections");

      _Group = _ignite?.GetCluster()?.ForAttribute(_roleAttribute, "True");

      if (_Group == null)
        throw new TRexException($"Cluster group reference is null in AcquireIgniteTopologyProjections for role {_role} on grid {_gridName}");

      if (_Group.GetNodes()?.Count == 0)
        throw new TRexException($"Group cluster topology is empty for role {_role} on grid {_gridName}");

      _compute = _Group.GetCompute();

      if (_compute == null)
        throw new TRexException($"Compute projection is null in AcquireIgniteTopologyProjections on grid {_gridName}");

      if (Log.IsTraceEnabled())
        Log.LogTrace($"Completed acquisition of TRex topology projections for grid {_gridName}");
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
