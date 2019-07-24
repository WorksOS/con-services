using System.Diagnostics;
using System.IO;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.SVL
{
  public abstract class TNFFLineworkEntity
  {
    public TNFFLineWorkElementType ElementType { get; set; }

    public int ElementIndex { get; set; }
    public int Colour { get; set; }
    public byte HeaderFlags { get; set; }
    public byte EntityFlags { get; set; }
    public byte ControlFlags { get; set; }

    public bool Is3D
    {
      get => GetIs3D();
      set => SetIs3D(value);
    }

    public bool IsGuidable
    {
      get => GetIsGuidable();
      set => SetIsGuidable(value);
    }

    public bool IsCrossSloped
    {
      get => GetHasCrossSlopes();
      set => SetHasCrossSlopes(value);
    }

    public bool IsStationed
    {
      get => GetHasStations();
      set => SetHasStations(value);
    }

    public bool ControlFlag_NullHeightAllowed
    {
      get => GetControlFlag(0);
      set => SetControlFlag(0, value);
    }

    public bool ControlFlag_NullCrossSlopeAllowed
    {
      get => GetControlFlag(1);
      set => SetControlFlag(1, value);
    }

    private void InitObject()
    {
      Colour = 1;
      HeaderFlags = 0;
      EntityFlags = 0;
      ControlFlags = 0;
      //fSuppressAssertions = False;
    }

    //Procedure SaveToNFFStream(Stream : TStream;
    //const OriginX, OriginY : Double;
    //FileVersion : TNFFFileVersion); Overload; Virtual; Abstract;

    public virtual void LoadFromNFFStream(BinaryReader reader,
      double OriginX, double OriginY,
      bool HasGuidanceID,
      TNFFFileVersion FileVersion)
    {
    }

    public virtual bool HasValidHeight() => false;

    public virtual XYZ GetStartPoint() => new XYZ(Consts.NullDouble, Consts.NullDouble, Consts.NullDouble);

    public virtual XYZ GetStartTransitPoint() => GetStartPoint();

    public virtual XYZ GetEndPoint() => new XYZ(Consts.NullDouble, Consts.NullDouble, Consts.NullDouble);

    public virtual XYZ GetEndTransitPoint() => GetEndPoint();

    public TNFFLineworkEntity()
    {
      InitObject();
    }

    // public TNFFLineworkEntity(int colour)
    //  {
    //    InitObject();

    //     Colour = colour;
    //   }


    public XYZ ClosestEndPointTo(double AX, double AY,
      out bool AStartPoint,
      bool InTransitDirection)
    {
      XYZ StartPoint;
      XYZ EndPoint;

      if (InTransitDirection)
      {
        StartPoint = GetStartTransitPoint();
        EndPoint = GetEndTransitPoint();
      }
      else
      {
        StartPoint = GetStartPoint();
        EndPoint = GetEndPoint();
      }

      if (StartPoint.IsNullInPlan || EndPoint.IsNullInPlan)
      {
        AStartPoint = false;
        return new XYZ(Consts.NullDouble, Consts.NullDouble, Consts.NullDouble);
      }

      AStartPoint = MathUtilities.Hypot(AX - StartPoint.X, AY - StartPoint.Y) < MathUtilities.Hypot(AX - EndPoint.X, AY - EndPoint.Y);

      return AStartPoint ? StartPoint : EndPoint;
    }

    public virtual void ComputeGeometricStationing()
    {

    }

    public virtual void Assign(TNFFLineworkEntity source)
    {
      //Assert(Entity is ClassType);

      Colour = source.Colour;
      HeaderFlags = source.HeaderFlags;
      EntityFlags = source.EntityFlags;
      ControlFlags = source.ControlFlags;
    }

    /*
    public virtual void SaveToStream(Stream stream)
    {
      WriteByteToStream(stream, ElementTypeInFile(TNFFLastRealFileVersion));
      WriteByteToStream(stream, FHeaderFlags);
      WriteByteToStream(stream, FEntityFlags);
      WriteIntegerToStream(stream, FColour);
    }
    */


    private void SetControlFlag(int Index,
      bool Value)
    {
      if (Value)
        ControlFlags |= (byte) (1 << Index);
      else
        ControlFlags &= (byte) ~(1 << Index);
    }


    public virtual void SetDefaultStationing(double AStartStation, int AIndex)
    {
    }

    public virtual void SetEndCrossSlope(double Value)
    {
    }

    public virtual void SetEndPoint(XYZ Value)
    {
    }

    public virtual void SetEndTransitPoint(XYZ Value) => SetEndPoint(Value);
    public virtual void SetEntityFlags(byte Value) => EntityFlags = Value;

    protected virtual void SetHeaderFlags(byte Value)
    {
      // Base class cannot be Stationed or have GuidanceID
      Debug.Assert((Value & (NFFConsts.kNFFElementHeaderHasStationing | NFFConsts.kNFFElementHeaderHasGuidanceID)) == 0x0);
      HeaderFlags = Value;
    }

    public void SetHasCrossSlopes(bool Value)
    {
      if (Value)
        HeaderFlags |= NFFConsts.kNFFElementHeaderHasCrossSlope;
      else
        HeaderFlags = (byte) (HeaderFlags & ~NFFConsts.kNFFElementHeaderHasCrossSlope);
    }

    public void SetHasStations(bool Value)
    {
      if (Value)
        HeaderFlags |= NFFConsts.kNFFElementHeaderHasStationing;
      else
        HeaderFlags = (byte) (HeaderFlags & ~NFFConsts.kNFFElementHeaderHasStationing);
    }

    public virtual byte ElementFlagsInFile(TNFFFileVersion FileVersion)
    {
      Debug.Assert(FileVersion >= TNFFFileVersion.nffVersion1_5,
        "Separate element flags byte not valid for pre v1.5 NFF files");

      return 0;
    }


    public virtual double ElementLength(int Index)
    {
      Debug.Assert(false);
      return 0;
    }

    public virtual double ElementLength()
    {
      Debug.Assert(false);
      return 0;
    }


    public virtual byte ElementTypeInFile(TNFFFileVersion FileVersion)
    {
      // The element type in the file has two parts: The actual ordinal element type
      // value is the low order nibble, and flags in the high order nibble

      return (byte) ElementType;
    }

    public XYZ FarthestEndPointFrom(double AX, double AY,
      ref bool AStartPoint)
    {
      XYZ StartPoint = GetStartPoint();
      XYZ EndPoint = GetEndPoint();

      if (StartPoint.IsNullInPlan || EndPoint.IsNullInPlan)
        return new XYZ(Consts.NullDouble, Consts.NullDouble, Consts.NullDouble);

      AStartPoint = MathUtilities.Hypot(AX - StartPoint.X, AY - StartPoint.Y) < MathUtilities.Hypot(AX - EndPoint.X, AY - EndPoint.Y);

      return AStartPoint ? EndPoint : StartPoint;
    }

    public virtual bool GetControlFlag(int Index) => (ControlFlags & (1 << Index)) != 0;

    public virtual double GetEndCrossSlope() => Consts.NullDouble;

    public virtual bool GetHasCrossSlopes() => (HeaderFlags & NFFConsts.kNFFElementHeaderHasCrossSlope) != 0; // and HasCrossSlopes;

    public virtual bool GetHasStations() => (HeaderFlags & NFFConsts.kNFFElementHeaderHasStationing) != 0;

    public virtual bool GetIs3D() => ((HeaderFlags & NFFConsts.kNFFElementHeaderHasElevation) != 0) && HasValidHeight();

    public virtual bool GetIsGuidable() => (HeaderFlags & NFFConsts.kNFFElementHeaderHasGuidanceID) != 0;

    public virtual double GetStartCrossSlope() => Consts.NullDouble;

    public virtual TNFFLineworkPolyLineVertexEntity GetVertex(int VertexNum)
    {
      Debug.Assert(false, "TNFFLineworkEntity.GetVertex not implemented for this entity type");
      return null;
      // Base class does nothing
    }

    public virtual bool HasInternalStructure() => false;

    public virtual void InsertVertex(TNFFLineworkPolyLineVertexEntity Vertex, int InsertAt)
    {
      Vertex.Parent = this;
    }

//    public virtual bool IsSameAs(TNFFLineworkEntity Other)
//    {
//      Debug.Assert(false, $"IsSameAs() is not implemented for {this.GetType().Name}");
//      return false;
//    }

    public virtual void LoadFromStream(BinaryReader reader)
    {
// Note: The EntityTypeInFile written out in SaveToStream will have already
//       been read in to determine the particular class type to instantiate.
// However, in order to allow individual entities to be streamed in the caller
// will have repositioned the stream pointer to the EntityTypeInFile, which
// the entity will duly read and ignore

      reader.ReadByte();

      HeaderFlags = reader.ReadByte();
      EntityFlags = reader.ReadByte();
      Colour = reader.ReadInt16();
    }

    public virtual BoundingWorldExtent3D BoundingBox() => new BoundingWorldExtent3D();

    public virtual double MinDistToAnEndPoint(double AX, double AY, bool InTransitDirection)
    {
      XYZ Closest = ClosestEndPointTo(AX, AY, out bool IsStartPoint, InTransitDirection);

      return Closest.IsNullInPlan ? Consts.NullDouble : MathUtilities.Hypot(AX - Closest.X, AY - Closest.Y);
    }

    public virtual void Reverse(int StartIdx, int EndIdx)
    {
      Debug.Assert(false, "TNFFLineworkEntity.Reverse(const StartIdx, EndIdx: Integer) must never be called");
    }

    public virtual void Reverse()
    {
      // Base class does nothing
    }

    public void SetIs3D(bool Value)
    {
      if (Value)
        HeaderFlags |= NFFConsts.kNFFElementHeaderHasElevation;
      else
        HeaderFlags &= (byte) (~NFFConsts.kNFFElementHeaderHasElevation);
    }

    public void SetIsGuidable(bool Value)
    {
      if (Value)
        HeaderFlags |= NFFConsts.kNFFElementHeaderHasGuidanceID;
      else
        HeaderFlags &= (byte) ~NFFConsts.kNFFElementHeaderHasGuidanceID;
    }

    public virtual void SetStartCrossSlope(double Value)
    {
    }

    public virtual void SetStartPoint(XYZ Value)
    {
    }

    public virtual void SetStartTransitPoint(XYZ Value) => SetEndPoint(Value);

    public virtual bool UpdateHeight(bool UpdateIfNullOnly,
      XYZ Position,
      double Station,
      int Index)
    {
      Debug.Assert(false, "No UpdateHeight implementation in TNFFLineWorkEntity");
      return false;
    }

    public virtual int VertexCount() => 0;

    /*
public void WriteEntityTypeAndFlagsToNFFStream(Stream: TStream;
FileVersion: TNFFFileVersion);
begin
  // Write the entity type to the stream
  WriteByteToStream(Stream, ElementTypeInFile(FileVersion));

If FileVersion >= nffVersion1_5 then
    WriteByteToStream(Stream, ElementFlagsInFile(FileVersion));
end;
*/

  }


}
