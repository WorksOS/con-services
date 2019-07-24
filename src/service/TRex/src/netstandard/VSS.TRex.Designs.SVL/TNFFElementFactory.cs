namespace VSS.TRex.Designs.SVL
{
  public class TNFFElementFactory
  {
    public TNFFLineworkEntity NewElement(TNFFLineWorkElementType type)
    {
      switch (type)
      {
        case TNFFLineWorkElementType.kNFFLineWorkLineElement:
          return null; // TNFFLineworkLineEntity
        case TNFFLineWorkElementType.kNFFLineWorkPolyLineElement:
          return new TNFFLineworkPolyLineEntity();
        case TNFFLineWorkElementType.kNFFLineWorkPolygonElement:
          return null; // TNFFLineworkPolygonEntity
        case TNFFLineWorkElementType.kNFFLineWorkSmoothedPolyLineElement:
          return new TNFFLineworkSmoothedPolyLineEntity();
        case TNFFLineWorkElementType.kNFFLineWorkArcElement:
          return new TNFFLineworkArcEntity();
        case TNFFLineWorkElementType.kNFFLineWorkPointElement:
          return null; // TNFFLineworkPointEntity
        case TNFFLineWorkElementType.kNFFLineWorkTextElement:
          return null; // TNFFLineworkTextEntity
        case TNFFLineWorkElementType.kNFFLineWorkCircleElement:
          return null; // TNFFLineworkCircleEntity
        case TNFFLineWorkElementType.kNFFLineworkEndElement:
          return null; // No entity type for this element type
        default:
          return null;
      }
    }
  }
}
