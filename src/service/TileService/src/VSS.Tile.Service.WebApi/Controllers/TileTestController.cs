using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using VSS.Common.Abstractions.Http;
using VSS.Tile.Service.Common.Extensions;
using VSS.Tile.Service.Common.Filters;

namespace VSS.Tile.Service.WebApi.Controllers
{
  public class TileTestController : BaseController<TileTestController>
  {

    /// <summary>
    /// Used to watermark tiles
    /// </summary>
    private static byte numberOfCalls = 0;
    /// <summary>
    /// Random generator for simulating delays in processing
    /// </summary>
    private static Random rndGenerator = new Random();

    /// <summary>
    /// Default constructor.
    /// </summary>
    public TileTestController()
      : base()
    {
    }

    /// <summary>
    /// Generates test tile for performance testing. The contract definition matches a usual WMS layer
    /// </summary>
    /// <param name="service"></param>
    /// <param name="version"></param>
    /// <param name="request"></param>
    /// <param name="format"></param>
    /// <param name="transparent"></param>
    /// <param name="layers"></param>
    /// <param name="crs"></param>
    /// <param name="styles"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="bbox"></param>
    /// <param name="projectUid"></param>
    /// <param name="fileType"></param>
    /// <returns></returns>
    [ValidateTileParameters]
    [Route("api/v1/testtile/png")]
    [HttpGet]
    public async Task<FileResult> GetTestTileRaw(
      [FromQuery] string service,
      [FromQuery] string version,
      [FromQuery] string request,
      [FromQuery] string format,
      [FromQuery] string transparent,
      [FromQuery] string layers,
      [FromQuery] string crs,
      [FromQuery] string styles,
      [FromQuery] int width,
      [FromQuery] int height,
      [FromQuery] string bbox,
      [FromQuery] Guid projectUid,
      [FromQuery] string fileType)
    {
      Log.LogDebug("GetTestTile: " + Request.QueryString);
      numberOfCalls++;
      byte[] overlayData;
      
      //Overlay the tiles. Return an empty tile if none to overlay.
      using (Image<Rgba32> bitmap = new Image<Rgba32>(Configuration.Default, width, height,new Rgba32(100,100,0,30)))
      {
        bitmap.Mutate(context =>
        {
          ApplyScalingWaterMark(context, SystemFonts.CreateFont(SystemFonts.Collection.Families.First().Name, 10), numberOfCalls.ToString(), Rgba32.HotPink,
            5, false);
          context.DrawPolygon(GraphicsOptions.Default, new Rgba32(0, 255, 255), 5, new PointF(0, 0), new PointF(0, height),
            new PointF(width, height), new PointF(width, 0), new PointF(0, 0));
        });
        overlayData = bitmap.BitmapToByteArray();
      }

      await Task.Delay(rndGenerator.Next(100));

      return new FileStreamResult(new MemoryStream(overlayData), ContentTypeConstants.ImagePng);
    }

    private IImageProcessingContext<TPixel> ApplyScalingWaterMark<TPixel>(IImageProcessingContext<TPixel> processingContext, Font font, string text, TPixel color, float padding, bool wordwrap)
      where TPixel : struct, IPixel<TPixel>
    {
      if (wordwrap)
      {
        return ApplyScalingWaterMarkWordWrap(processingContext, font, text, color, padding);
      }
      else
      {
        return ApplyScalingWaterMarkSimple(processingContext, font, text, color, padding);
      }
    }

    private IImageProcessingContext<TPixel> ApplyScalingWaterMarkSimple<TPixel>(IImageProcessingContext<TPixel> processingContext, Font font, string text, TPixel color, float padding)
      where TPixel : struct, IPixel<TPixel>
    {
      return processingContext.Apply(img =>
      {
        float targetWidth = img.Width - (padding * 2);
        float targetHeight = img.Height - (padding * 2);

        // measure the text size
        SizeF size = TextMeasurer.Measure(text, new RendererOptions(font));

        //find out how much we need to scale the text to fill the space (up or down)
        float scalingFactor = Math.Min(img.Width / size.Width, img.Height / size.Height);

        //create a new font
        Font scaledFont = new Font(font, scalingFactor * font.Size);

        var center = new PointF(img.Width / 2, img.Height / 2);
        var textGraphicOptions = new TextGraphicsOptions(true)
        {
          HorizontalAlignment = HorizontalAlignment.Center,
          VerticalAlignment = VerticalAlignment.Center
        };
        img.Mutate(i => i.DrawText(textGraphicOptions, text, scaledFont, color, center));
      });
    }

    private IImageProcessingContext<TPixel> ApplyScalingWaterMarkWordWrap<TPixel>(IImageProcessingContext<TPixel> processingContext, Font font, string text, TPixel color, float padding)
            where TPixel : struct, IPixel<TPixel>
    {
      return processingContext.Apply(img =>
      {
        float targetWidth = img.Width - (padding * 2);
        float targetHeight = img.Height - (padding * 2);

        float targetMinHeight = img.Height - (padding * 3); // must be with in a margin width of the target height

        // now we are working i 2 dimensions at once and can't just scale because it will cause the text to
        // reflow we need to just try multiple times

        var scaledFont = font;
        SizeF s = new SizeF(float.MaxValue, float.MaxValue);

        float scaleFactor = (scaledFont.Size / 2);// everytime we change direction we half this size
        int trapCount = (int)scaledFont.Size * 2;
        if (trapCount < 10)
        {
          trapCount = 10;
        }

        bool isTooSmall = false;

        while ((s.Height > targetHeight || s.Height < targetMinHeight) && trapCount > 0)
        {
          if (s.Height > targetHeight)
          {
            if (isTooSmall)
            {
              scaleFactor = scaleFactor / 2;
            }

            scaledFont = new Font(scaledFont, scaledFont.Size - scaleFactor);
            isTooSmall = false;
          }

          if (s.Height < targetMinHeight)
          {
            if (!isTooSmall)
            {
              scaleFactor = scaleFactor / 2;
            }
            scaledFont = new Font(scaledFont, scaledFont.Size + scaleFactor);
            isTooSmall = true;
          }
          trapCount--;

          s = TextMeasurer.Measure(text, new RendererOptions(scaledFont)
          {
            WrappingWidth = targetWidth
          });
        }

        var center = new PointF(padding, img.Height / 2);
        var textGraphicOptions = new TextGraphicsOptions(true)
        {
          HorizontalAlignment = HorizontalAlignment.Left,
          VerticalAlignment = VerticalAlignment.Center,
          WrapTextWidth = targetWidth
        };
        img.Mutate(i => i.DrawText(textGraphicOptions, text, scaledFont, color, center));
      });
    }

  }
}
