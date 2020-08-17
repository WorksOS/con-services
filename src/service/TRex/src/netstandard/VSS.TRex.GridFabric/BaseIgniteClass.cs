using Apache.Ignite.Core;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Compute;
using Microsoft.Extensions.Logging;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Models.Servers;
using VSS.Serilog.Extensions;
using VSS.TRex.Common.Extensions;
using System.Linq;
using System;

namespace VSS.TRex.GridFabric
{
  public class BaseIgniteClass : IBinarizable, IFromToBinary
  {
    private static readonly ILogger _log = Logging.Logger.CreateLogger<BaseIgniteClass>();

    /// <summary>
    /// The name of the custom thread pool that progressive queries should be run within to avoid deadlock with
    /// the primary public thread pool
    /// </summary>
    public const string TREX_PROGRESSIVE_QUERY_CUSTOM_THREAD_POOL_NAME = "TRexProgressiveQueryResponsePool";

    private IIgnite _ignite;
    /// <summary>
    /// Ignite instance.
    /// Note: This was previous an [InstanceResource] but this does not work well with more than one Grid active in the process
    /// </summary>
    protected IIgnite Ignite { get => _ignite; }

    /// <summary>
    /// The cluster group of nodes in the grid that are available for responding to design/profile requests
    /// </summary>
    private IClusterGroup _group;

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

    protected void DumpClusterStateToLog()
    {
      try
      {
        _log.LogInformation("#In# DumpClusterStateToLog: Starting topolgy state dump");

        var numClusterNodes = _ignite?.GetCluster()?.GetNodes()?.Count ?? 0;

        // Log the known state of the cluster
        _log.LogInformation($"Node attribute selected for: {_roleAttribute}. Num nodes in cluster ({_gridName} [Ignite reported:{_ignite?.Name ?? "NULL!"}]): {numClusterNodes}");

        if (numClusterNodes > 0)
        {
          _ignite?.GetCluster()?.GetNodes().ForEach(x =>
          {
            _log.LogInformation($"Node ID {x.Id}, ConsistentID: {x.ConsistentId}, Version:{x.Version}, IsClient?:{x.IsClient}, IsDaemon?:{x.IsDaemon}, IsLocal?:{x.IsLocal}, Order:{x.Order}, Addresses#: {x.Addresses?.Count ?? 0}, HostNames:{((x.HostNames?.Count ?? 0) > 0 ? x.HostNames.Aggregate((s, o) => s + o) : "No Host Names")}");

            var roleAttributes = x.Attributes?.Where(x => x.Key.StartsWith("Role-"));
            if ((roleAttributes?.Count() ?? 0) > 0)
              _log.LogInformation($"Roles: {roleAttributes.Select(x => $" K:V={x.Key}:{x.Value}").Aggregate((s, o) => s + o)}");
            else
              _log.LogError("No role attributes present");
          });
        }
      }
      catch (Exception e)
      {
        _log.LogError($"Exception {e.Message} occurred during {nameof(DumpClusterStateToLog)}");
      }
      finally
      {
        _log.LogInformation("#Out# DumpClusterStateToLog: Completed topolgy state dump");
      }
    }

    /// <summary>
    /// Acquires references to group and compute topology projections on the Ignite grid that may accept requests from this request
    /// </summary>
    public void AcquireIgniteTopologyProjections()
    {
      if (_log.IsTraceEnabled())
        _log.LogTrace($"Acquiring TRex topology projections for grid {_gridName}");

      if (string.IsNullOrEmpty(_gridName))
        throw new TRexException("GridName name not defined when acquiring topology projection");

      if (string.IsNullOrEmpty(_role))
        throw new TRexException("Role name not defined when acquiring topology projection");

      try
      {
        _ignite = _tRexGridFactory?.Grid(_gridName);
      }
      catch (IgniteException e)
      {
        throw new TRexException($"Failed to find Grid {_gridName} due to Ignite Exception", e);
      }

      if (_ignite == null)
        throw new TRexException("Ignite reference is null in AcquireIgniteTopologyProjections");

      try
      {
        _group = _ignite?.GetCluster()?.ForAttribute(_roleAttribute, "True");
      }
      catch (IgniteException e)
      {
        throw new TRexException($"Failed to find Node on Grid {_gridName} with Role {_roleAttribute} due to Ignite Exception", e);
      }

      if (_group == null)
        throw new TRexException($"Cluster group reference is null in AcquireIgniteTopologyProjections for role {_role} on grid {_gridName}");

      if ((_group.GetNodes()?.Count ?? 0) == 0)
      {
        DumpClusterStateToLog();

        throw new TRexException($"Group cluster topology is empty for role {_role} on grid {_gridName}");
      }

      try
      {
        _compute = _group.GetCompute();
      }
      catch (IgniteException e)
      {
        throw new TRexException($"Failed to find Compute for Grid {_gridName} due to Ignite Exception", e);
      }

      if (_compute == null)
      {
        _log.LogError($"Cluster group for derived compute topology projection is null for request on grid {_gridName}");
        DumpClusterStateToLog();
        throw new TRexException($"Compute projection is null in AcquireIgniteTopologyProjections on grid {_gridName}");
      }

      if ((_compute.ClusterGroup.GetNodes()?.Count ?? 0) == 0)
      {
        _log.LogError($"Cluster group for derived compute topology projection is empty for request on grid {_gridName}");
        DumpClusterStateToLog();
      }

      if (_log.IsTraceEnabled())
        _log.LogTrace($"Completed acquisition of TRex topology projections for grid {_gridName}");
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
