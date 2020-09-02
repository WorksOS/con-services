using System;
using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions.Exceptions;
using VSS.TRex.GridFabric.Arguments;

namespace VSS.TRex.GridFabric.ComputeFuncs
{
  /// <summary>
  /// The base class for compute functions. This provides common aspects such as the injected Ignite instance
  /// </summary>
  public class BaseComputeFunc : VersionCheckedBinarizableSerializationBase
  {
    private const byte VERSION_NUMBER = 1;

    public BaseComputeFunc()
    {
    }

    /// <summary>
    /// By convention in TRex, compute functions derive their state from the supplied argument to
    /// their Invoke() method. State derived from BaseIgniteClass is intended to allow the representation
    /// of the compute function on the invoking side the ability to target appropriate grid resources
    /// for execution of the function. Thus, IBinarizable serialization for base compute func in TRex is a
    /// null function.
    /// </summary>
    public override void InternalToBinary(IBinaryRawWriter writer)
    {
      // Version the serialization, even if nothing additional is serialized
      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);
    }

    public override void InternalFromBinary(IBinaryRawReader reader)
    {
      // Version the serialization, even if nothing additional is serialized
      var version = VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);
    }

    /// <summary>
    /// Check the argument supplied to the compute func to ensure that it is received within the TPaaS request timeout of 60 seconds
    /// from the point in time the request was created.
    /// This is an optional check for requests to perform based on their business rules and use cases.
    /// </summary>
    public void PerformTPaaSRequestLivelinessCheck(BaseRequestArgument arg)
    {
      if (arg.IsOutsideTPaaSTimeout)
      {
        throw new TRexColorPaletteException($"Request '{arg.ExternalDescriptor}' failed liveliness check. Request emitted at {arg.RequestEmissionDateUtc}, which is {DateTime.UtcNow - arg.RequestEmissionDateUtc}");
      }
    }
  }
}
