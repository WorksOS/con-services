using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace XnaFan.ImageComparison.Netcore
{
  public static class Graphics
  {
    public static Pen<Rgba32> RedPen = new Pen<Rgba32>(Rgba32.Red, 1);
    public static Pen<Rgba32> GreenPen = new Pen<Rgba32>(Rgba32.Green, 1);
    public static Pen<Rgba32> BluePen = new Pen<Rgba32>(Rgba32.Blue, 1);

  }
}
