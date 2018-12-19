using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace VSS.TMS
{
  static class Utils
  {

    public static readonly string TerrainData = "application/octet-stream";
    //  public static readonly string TerrainData = "application/vnd.quantized-mesh; extensions=octvertexnormals";

    public static readonly string ImagePng = "image/png";

    public static readonly string ImageJpeg = "image/jpeg";

    public static readonly string TextXml = "text/xml";

    public static readonly string LocalFileScheme = "file:///";

    public static readonly string MBTilesScheme = "mbtiles:///";

    public static readonly string TileMapServiceVersion = "1.0.0";

    public static readonly string EPSG3857 = "EPSG:3857";


    public static IList<TileSetConfiguration> GetTileSetConfigurations(this IConfiguration configuration)
    {
      return configuration
        .GetSection("tilesets")
        .Get<IList<TileSetConfiguration>>();
    }

    public static string GetContentType(string tileFormat)
    {
      var mediaType = String.Empty;
      switch (tileFormat)
      {
        case "png": { mediaType = ImagePng; break; }
        case "jpg": { mediaType = ImageJpeg; break; }
        case "terrain": { mediaType = TerrainData; break; }

        default: throw new ArgumentException("tileFormat");
      }

      return mediaType;
    }

    public static bool IsMBTilesScheme(string source)
    {
      return (source.StartsWith(MBTilesScheme, StringComparison.Ordinal));
    }

    public static bool IsLocalFileScheme(string source)
    {
      return (source.StartsWith(LocalFileScheme, StringComparison.Ordinal));
    }


    /// <summary>
    /// Convert Y tile coordinate of TMS standard (flip)
    /// </summary>
    /// <param name="y"></param>
    /// <param name="zoom">Zoom level</param>
    /// <returns></returns>
    public static int FromTmsY(int tmsY, int zoom)
    {
      return (1 << zoom) - tmsY - 1;
    }
  }


}

