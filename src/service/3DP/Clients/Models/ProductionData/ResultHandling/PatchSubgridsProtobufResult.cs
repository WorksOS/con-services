using System.ComponentModel;
using Newtonsoft.Json;
using ProtoBuf;

namespace VSS.Productivity3D.Productivity3D.Models.ProductionData.ResultHandling
{
#pragma warning disable 1570
  /// <summary>
  /// A structured representation of the data returned by the Patch request
  /// </summary>
  /// <remarks>
  /// In order for a consumer to use the type it's best to create a .proto file that defines the message object.
  /// e.g. string proto = Serializer.GetProto<PatchResult>();
  /// See https://github.com/mgravell/protobuf-net/blob/master/src/Examples/ProtoGeneration.cs.
  /// </remarks>
#pragma warning restore 1570
  [ProtoContract (SkipConstructor = true)]
  public class PatchSubgridsProtobufResult
  {
    protected PatchSubgridsProtobufResult()
    { }

    /// <summary>
    /// All cells in the patch are of this size. All measurements relating to the cell in the patch are made at the center point of each cell.
    /// </summary>
    [ProtoMember(1, IsRequired = false)]
    [JsonProperty(PropertyName = "cellSize")]
    public double? CellSize { get; protected set; }

    /// <summary>
    /// The collection of subgrids returned in this patch request result.
    /// </summary>
    [ProtoMember(4, IsRequired = false)]
    [JsonProperty(PropertyName = "subgrids")]
    public PatchSubgridOriginProtobufResult[] Subgrids { get; protected set; }

    public const string DefaultMessage = "success";

    [ProtoMember(5, IsRequired = true)]
    [JsonProperty(PropertyName = "code", Required = Required.Always)]
    [DefaultValue(0)]
    public int Code { get; protected set; }

    [ProtoMember(6, IsRequired = false)]
    [JsonProperty(PropertyName = "message")]
    public string Message { get; protected set; }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static PatchSubgridsProtobufResult Create(double cellSize, PatchSubgridOriginProtobufResult[] subgrids)
    {
      return new PatchSubgridsProtobufResult
      {
        CellSize = cellSize,        
        Subgrids = subgrids,
        Code = 0
      };
    }

    public PatchSubgridsProtobufResult(int code, string message = DefaultMessage)
    {
      Code = code;
      Message = message;
    }
  }
}
