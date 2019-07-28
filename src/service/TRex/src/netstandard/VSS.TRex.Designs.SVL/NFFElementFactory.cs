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
          return new NFFLineworkPolyLineEntity();
        case NFFLineWorkElementType.kNFFLineWorkPolygonElement:
          return null; // NFFLineworkPolygonEntity
        case NFFLineWorkElementType.kNFFLineWorkSmoothedPolyLineElement:
          return new NFFLineworkSmoothedPolyLineEntity();
        case NFFLineWorkElementType.kNFFLineWorkArcElement:
          return new NFFLineworkArcEntity();
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
