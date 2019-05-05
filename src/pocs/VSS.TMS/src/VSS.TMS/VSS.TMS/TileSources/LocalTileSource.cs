using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VSS.TMS.TileSources
{
  // This instance of the interface gets data from local disk
  class LocalTileSource : ITileSource
  {

    private TileSetConfiguration configuration;

    private readonly string contentType;

    public LocalTileSource(TileSetConfiguration configuration)
    {
      this.configuration = configuration;
      this.contentType = Utils.GetContentType(this.configuration.Format);
    }



    public static byte[] CreateGridImage(
      int x,
      int y,
      int z,
      int boxSize)
    {
      /*      MediaTypeNames.Image image = new Bitmap(2000, 1024);

          Bitmap bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Graphics graphics = Graphics.FromImage(bitmap);

            var pen = new Pen(lineColor, widthLine);

            graphics.FillRectangle(new SolidBrush(bgColor), new Rectangle(0, 0, width, height));
            */

      using (var bmp = new System.Drawing.Bitmap(boxSize, boxSize))
      {
        using (Graphics g = Graphics.FromImage(bmp))
        {
          if (z < 2 )
            g.Clear(Color.Yellow);
          else if (z < 10)
            g.Clear(Color.Blue);
          else if (z < 15)
            g.Clear(Color.Green);
          else
            g.Clear(Color.BurlyWood);

          Pen pen = new Pen(Color.Black);
          pen.Width = 1;

          g.DrawLine(pen, 0, 0, boxSize - 1, 0);
          g.DrawLine(pen, 0, 0, 0, boxSize - 1);

          RectangleF rectf = new RectangleF(70, 90, 90, 50);
          //g.DrawString("test", new Font("Tahoma", 8), Brushes.Black, rectf);

          //Draw cross
          //        ..       g.DrawLine(pen, 1,1, 1, boxSize);
          //     g.DrawLine(pen, 1,1, boxSize, 1);
          //    RectangleF rectf = new RectangleF(70, 90, 90, 50);


          // use cesium extend menu feature instead
         // g.DrawString(String.Format("X:{0}, Y:{1}, Z:{2}", x, y, z), new Font("Tahoma", 12), Brushes.Black, rectf);



          var memStream = new MemoryStream();
          bmp.Save(memStream, ImageFormat.Jpeg);

          //  bmp.Save("c:\temp\test.png");
          return memStream.ToArray();
        }
      }
    }

    /// <summary>
    /// Get Terrain Height tile
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    async Task<byte[]> ITileSource.GetTerrainTileAsync(int x, int y, int z)
    {

      //var path = String.Format(@"C:\map\terrain\myset\{0}\{1}\{2}.terrain",z, x,y);


      var path = String.Format(@"C:\map\terrain\dummy\0.terrain");

      var fileInfo = new FileInfo(path);
      if (fileInfo.Exists)
      {
        var buffer = new byte[fileInfo.Length];
        using (var fileStream = fileInfo.OpenRead())
        {
          await fileStream.ReadAsync(buffer, 0, buffer.Length);
          return buffer;
        }
      }
      else
      {
        return null;
      }
    }

    async Task<byte[]> ITileSource.GetTerrainQMTileAsync(int x, int y, int z, string path, int mode)
    {

    //  var path2 = String.Format(@"C:\map\data\mydata\tiles\{0}\{1}\{2}.terrain", z, x,y);
    string path2; 
      if (z > 10)
        path2 = Path.Combine(path, "fake.terrain");
      else
        path2 = Path.Combine(path, string.Format(@"{0}\{1}\{2}.terrain", z, x, y));

      //   var path = String.Format(@"C:\map\data\TestData\{0}\{1}\{2}.terrain", z, x, y);

      //  var path = String.Format(@"C:\map\qmtiles\1111.terrain");
      var fileInfo = new FileInfo(path2);
      if (!fileInfo.Exists && mode == 1)
      {
        path2 = Path.Combine(path, String.Format(@"{0}\{1}\{2}.terrain", 99, 0, 0));
        fileInfo = new FileInfo(path2);
      }

      if (fileInfo.Exists)
      {
        var buffer = new byte[fileInfo.Length];
        using (var fileStream = fileInfo.OpenRead())
        {
          await fileStream.ReadAsync(buffer, 0, buffer.Length);
          Console.WriteLine("Tile {0} sent",fileInfo);
          return buffer;
        }
      }
      else
      {
        Console.WriteLine("*** Tile {0} was NOT sent ***", fileInfo);
        return null;
      }
    }


    async Task<byte[]> ITileSource.GetImageTileAsync(int x, int y, int z)
    {



      try
      {
        return CreateGridImage(x, y, z, 256);
      }

      catch (ArgumentException e)
      {
        var msg = e.Message;
        return null;
      }


      /*
      var path = @"c:\map\Test3Image.jpg";
      var fileInfo = new FileInfo(path);
      if (fileInfo.Exists)
      {
        var buffer = new byte[fileInfo.Length];
        using (var fileStream = fileInfo.OpenRead())
        {
          await fileStream.ReadAsync(buffer, 0, buffer.Length);
          return buffer;
        }
      }
      else
      {
        return null;
      }
      */
    }


    TileSetConfiguration ITileSource.Configuration => this.configuration;

    string ITileSource.ContentType => this.contentType;

  }
}
