namespace VSS.Productivity3D.Models.ResultHandling
{
  public class SummaryVolumesProfileCell
  {
    public double Station { get; private set; }
  
    public double InterceptLength { get; private set; }

    public uint OTGCellX { get; private set; }

    public uint OTGCellY { get; private set; }

    public float DesignElev { get; private set; }

    public float LastCellPassElevation1 { get; private set; }

    public float LastCellPassElevation2 { get; private set; }

    // Default public constructor.
    public SummaryVolumesProfileCell()
    {

    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="station"></param>
    /// <param name="interceptLength"></param>
    /// <param name="otgCellX"></param>
    /// <param name="otgCellY"></param>
    /// <param name="designElev"></param>
    /// <param name="lastCellPassElevation1"></param>
    /// <param name="lastCellPassElevation2"></param>
    public SummaryVolumesProfileCell(
      double station, 
      double interceptLength, 
      uint otgCellX, 
      uint otgCellY, 
      float designElev, 
      float lastCellPassElevation1, 
      float lastCellPassElevation2)
    {
      Station = station;
      InterceptLength = interceptLength;
      OTGCellX = otgCellX;
      OTGCellY = otgCellY;
      DesignElev = designElev;
      LastCellPassElevation1 = lastCellPassElevation1;
      LastCellPassElevation2 = lastCellPassElevation2;
    }

    public void SetValues(
      double station,
      double interceptLength,
      uint otgCellX,
      uint otgCellY,
      float designElev,
      float lastCellPassElevation1,
      float lastCellPassElevation2)
    {
      Station = station;
      InterceptLength = interceptLength;
      OTGCellX = otgCellX;
      OTGCellY = otgCellY;
      DesignElev = designElev;
      LastCellPassElevation1 = lastCellPassElevation1;
      LastCellPassElevation2 = lastCellPassElevation2;
    }
  }
}
