using System;
using System.Threading.Tasks;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using VSS.TRex.GridFabric.Interfaces;

namespace VSS.TRex.GridFabric.Requests
{
  /// <summary>
  /// The base class for requests. This provides common aspects such as the injected Ignite instance
  /// </summary>
  public abstract class BaseRequest<TArgument, TResponse> : BaseIgniteClass, IBaseRequest<TArgument, TResponse>
  {
    private const byte VERSION_NUMBER = 1;

    private const int TPAAS_REQUEST_TIMEOUT_SECONDS = 60;

    /// <summary>
    /// The time the request was emitted from the service platform context acting as a client to TRex.
    /// Any request arriving at an endpoint in TRex that may have this time examined to determined if it is too
    /// old to be considered. The prime motivation for this is the TPaaS request timeout (currently 60 seconds
    /// at the time of writing)
    /// </summary>
    public DateTime RequestEmissionDateUtc = DateTime.UtcNow;

    /// <summary>
    /// Indicates this request was emitted outside of the TPaaS request timeout
    /// </summary>
    public bool IsOutsideTPaaSTimeout { get; private set; }

    /// <summary>
    /// Constructor accepting a role for the request that may identify a cluster group of nodes in the grid
    /// </summary>
    public BaseRequest(string gridName, string role) : base(gridName, role)
    {
    }

    public virtual TResponse Execute(TArgument arg)
    {
      // No implementation in base class - complain if we are called
      throw new TRexException($"{nameof(Execute)} invalid to call.");
    }

    public virtual Task<TResponse> ExecuteAsync(TArgument arg)
    {
      // No implementation in base class - complain if we are called
      throw new TRexException($"{nameof(ExecuteAsync)} invalid to call.");
    }

    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteLong(RequestEmissionDateUtc.ToBinary());
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      base.InternalFromBinary(reader);

      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (version == 1)
      {
        RequestEmissionDateUtc = DateTime.FromBinary(reader.ReadLong());

        IsOutsideTPaaSTimeout = RequestEmissionDateUtc.AddSeconds(TPAAS_REQUEST_TIMEOUT_SECONDS) < DateTime.UtcNow;
      }
    }
  }
}
