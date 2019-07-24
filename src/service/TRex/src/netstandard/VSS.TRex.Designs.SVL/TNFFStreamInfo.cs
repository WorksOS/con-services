namespace VSS.TRex.Designs.SVL
{
  public class TNFFStreamInfo
  {
    public string Name {get; set; }
    public int Offset { get; set; }
    public int Length { get; set; }

    public TNFFStreamInfo(string name, int offset, int length)
    {
      Name = name;
      Offset = offset;
      Length = length;
    }
  }
}
