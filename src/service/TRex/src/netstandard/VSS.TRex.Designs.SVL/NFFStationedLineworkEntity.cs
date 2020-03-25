using System;
using VSS.TRex.Common;
using VSS.TRex.Common.Exceptions;
using Range = VSS.TRex.Common.Utilities.Range;

namespace VSS.TRex.Designs.SVL
{
  /*---------------------------------------------------------------------------
Name: NFFStationedLineworkEntity

Comments: Abstract base class for linework entities that MAY have stationing
information associated with them.

NOTE: <EndStation> is calculated as an entity geometry dictated offset from
<StartStation>, thus it is exposed as a read-only property and does not need
to be streamed to/from file.

NOTE: Stationed linework entities will only stream their<StartStation> value
to/from file if the kNFFElementHeaderHasStationing flag is set in their<headeerFlags> byte.
---------------------------------------------------------------------------*/
  public abstract class NFFStationedLineworkEntity : NFFGuidableLineworkEntity
  {
    private double _startStation;

    public NFFStationedLineworkEntity()
    {
      _startStation = Consts.NullDouble;
    }

    protected virtual double GetStartStation() => _startStation;
    protected virtual void SetStartStation(double Value) => _startStation = Value;

    protected abstract double GetEndStation();

    protected override void SetHeaderFlags(byte Value) => _headerFlags = Value;

    protected virtual double GetVertexElevation(int VertexNum)
    {
      throw new TRexException("GetVertexElevation not implemented in base class");
    }

    protected virtual void SetVertexElevation(int VertexNum,
      double Value)
    {
      throw new TRexException("SetVertexElevation not implemented in base class");
    }

    protected virtual double GetVertexStation(int VertexNum)
    {
      throw new TRexException("GetVertexStation not implemented in base class");
    }

    protected virtual void SetVertexStation(int VertexNum,
      double Value)
    {
      throw new TRexException("SetVertexStation not implemented in base class");
    }

    /*
      function GetVertexLeftCrossSlope(VertexNum: Integer): Double; Virtual;
      procedure SetVertexLeftCrossSlope(VertexNum: Integer;
const Value: Double); Virtual;

      function GetVertexRightCrossSlope(VertexNum: Integer): Double; Virtual;
      procedure SetVertexRightCrossSlope(VertexNum: Integer;
const Value: Double); Virtual;
      */

    // public
    public override void Assign(NFFLineworkEntity Entity)
    {
      base.Assign(Entity);

      StartStation = (Entity as NFFStationedLineworkEntity).StartStation;
    }

    public double StartStation
    {
      get => GetStartStation();
      set => SetStartStation(value);
    }

    public double EndStation => GetEndStation();

    // property VertexElevations[VertexNum : Integer] : Double read GetVertexElevation write SetVertexElevation;
    // property VertexStations[VertexNum : Integer] : Double read GetVertexStation write SetVertexStation;

    /*
      property VertexLeftCrossSlopes[VertexNum : Integer] : Double read GetVertexLeftCrossSlope write SetVertexLeftCrossSlope;
      property VertexRightCrossSlopes[VertexNum : Integer] : Double read GetVertexRightCrossSlope write SetVertexRightCrossSlope;
      */

    // Procedure SaveToStream(Stream : TStream); Override;
    /*
    public override void LoadFromStream(BinaryReader reader)
    {
      base.LoadFromStream(reader);

      if ((HeaderFlags & NFFConsts.kNFFElementHeaderHasStationing) != 0)
        _startStation = reader.ReadDouble();
    }
    */

    public virtual void ComputeStnOfs(double X, double Y, out double Stn, out double Ofs)
    {
      Stn = Consts.NullDouble;
      Ofs = Consts.NullDouble;

      throw new TRexException("NFFStationedLineworkEntity.ComputeStnOfs has null implementation");
    }

    public virtual void ComputeXY(double Stn, double Ofs, out double X, out double Y)
    {
      X = Consts.NullDouble;
      Y = Consts.NullDouble;

      throw new TRexException("NFFStationedLineworkEntity.ComputeXY has null implementation");
    }

    // ResetStartStation sets a new start station for this element, including
    // bubbling the change in start station value through any sub-members of the
    // element.
    //public virtual void ResetStartStation(double NewStartStation) => _startStation = NewStartStation;

    // RepresentsStation determines if the given station value falls within the
    // station range of this element
    public bool RepresentsStation(double AStation)
    {
      bool result = false;
      if (StartStation != Consts.NullDouble && EndStation != Consts.NullDouble)
        return Range.InRange(AStation, StartStation, EndStation);

      return result;
    }

    // HasVertexAtStation determines if the given station value falls on the
    // start or end of the element, or on any intermediary vertex of the entity
    public virtual bool HasVertexAtStation(double AStation, double Tolerance)
    {
      bool result = false;

      if (StartStation != Consts.NullDouble && EndStation != Consts.NullDouble)
        result = ((Math.Abs(AStation - StartStation) < Tolerance) || (Math.Abs(AStation - EndStation) < Tolerance));

      return result;
    }

    // CreateVertexAtStation creates a new vertex at the requested station. The station value must
    // lie between the station values of two surrounding vertices. The other values for the vertex are
    // calculated from those of the surrounding vertices.
    public virtual NFFLineworkPolyLineVertexEntity CreateVertexAtStation(double Chainage)
    {
      throw new TRexException("NFFStationedLineworkEntity.CreateVertexAtStation should be considered abstract");
    }

    public virtual NFFLineworkPolyLineVertexEntity CreateNewVertex()
    {
      throw new TRexException("NFFStationedLineworkEntity.CreateNewVertex should be considered abstract");
    }
  }
}
