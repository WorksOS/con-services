namespace VSS.TRex.Designs.SVL
{
  public class NFFElementFactory
  {
    public NFFLineworkEntity NewElement(NFFLineWorkElementType type)
    {
      switch (type)
      {
        case NFFLineWorkElementType.kNFFLineWorkLineElement:
          return null; // NFFLineworkLineEntity
        case NFFLineWorkElementType.kNFFLineWorkPolyLineElement:
          return new NffLineworkPolyLineEntity();
        case NFFLineWorkElementType.kNFFLineWorkPolygonElement:
          return null; // NFFLineworkPolygonEntity
        case NFFLineWorkElementType.kNFFLineWorkSmoothedPolyLineElement:
          return new NffLineworkSmoothedPolyLineEntity();
        case NFFLineWorkElementType.kNFFLineWorkArcElement:
          return new NffLineworkArcEntity();
        case NFFLineWorkElementType.kNFFLineWorkPointElement:
          return null; // NFFLineworkPointEntity
        case NFFLineWorkElementType.kNFFLineWorkTextElement:
          return null; // NFFLineworkTextEntity
        case NFFLineWorkElementType.kNFFLineWorkCircleElement:
          return null; // NFFLineworkCircleEntity
        case NFFLineWorkElementType.kNFFLineworkEndElement:
          return null; // No entity type for this element type
        default:
          return null;
      }
    }
  }
}
