using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Configuration;
using System.Net.Http;
using System.Windows.Forms;
using VSS.Map3D.Common;
using VSS.Map3D.DEM;
using VSS.Map3D.Mesh;
using VSS.Map3D.Models;
using VSS.Map3D.Models.QMTile;
using VSS.Map3D.Terrain;
using VSS.Map3D.Tiler;

/// <summary>
/// Developers Experimental Toolbox. Anything goes and nothing guaranteed to work :) 
/// </summary>
namespace MapClient
{
  public partial class frmMain : Form
  {

    private static string directoryPath = @".\temp";

    public frmMain()
    {
      InitializeComponent();
    }


    public static ushort ConvertRange2(double originalStart, double originalEnd, double value) // value to convert
    {
      double scale = (double) (32768) / (originalEnd - originalStart);
      return (ushort) ((value - originalStart) * scale);
    }

    public static ushort ConvertRange(double originalStart, double originalEnd, double value) // value to convert
    {
      double scale = (double) (32768 - 1) / (originalEnd - originalStart);
      return (ushort) (1 + ((value - originalStart) * scale));
    }


    private async void btnTestFakeTerrain_Click_1(object sender, EventArgs e)
    {


      listBox1.Items.Add("Fetching DEM...");
      // class to get digital elevation models(DEM)
      FakeDEMSource demSrc = new FakeDEMSource();

      demSrc.Initalize(new MapHeader() {GridSize = Int32.Parse(txtTileSize.Text)});

      var elevData = await demSrc.GetDemLL(1, 1, 1, 1); //) mp.GetTerrainTileAsync(x,y,z);
      listBox1.Items.Add($"DEM length {elevData.Elev.Length}");

      listBox1.Items.Add("Making Mesh...");
      // This class constructs a cesium quantized mesh from the dem
      var vertices = new Mesh().MakeFakeMesh(ref elevData);

      listBox1.Items.Add("Making Tile...");
      // This class constructs a quantized mesh tile mesh from the original mesh
      var tile = new Tiler().MakeTile(vertices,
        new TerrainTileHeader() {MinimumHeight = elevData.MinimumHeight, MaximumHeight = elevData.MaximumHeight},
        MapUtil.GridSizeToTriangleCount(elevData.GridSize), elevData.GridSize);

      listBox1.Items.Add($"Tile Ready. Size:{tile.Length} . Saving to {txtPath.Text}");

      var ms = new MemoryStream(tile);
      using (FileStream fs = new FileStream(txtPath.Text, FileMode.Create))
      using (GZipStream zipStream = new GZipStream(fs, CompressionMode.Compress, false))
      {
        zipStream.Write(ms.ToArray(), 0, ms.ToArray().Length); // .Write(bytes, 0, bytes.Length);
      }
    }

    private void button2_Click_1(object sender, EventArgs e)
    {
      ushort _u;
      ushort _v;
      ushort _height;
      ushort prev_u = 0;
      ushort prev_v = 0;
      ushort prev_height = 0;
      VertexData vData = new VertexData(1, 1);

      int k = 0;
      //     vData.AddVertex(k, 0, 0, ConvertRange(0,200,16384));
      vData.AddVertex(k, 0, 0, 16384);
      k = 1;
      vData.AddVertex(k, 0, 32767, 0);
      k = 2;
      vData.AddVertex(k, 32767, 0, 32767);
      k = 3;
      vData.AddVertex(k, 32767, 32767, 6384);
      for (int i = 0; i < vData.u.Length; i++)
      {
        listBox1.Items.Add($"uvh{i}: {vData.u[i]}, {vData.v[i]}, {vData.height[i]}");
      }

      listBox1.Items.Add($"Enccode");


      for (int i = 0; i < vData.u.Length; i++)
      {
        // work out delta of current value minus prev value and encode
        _u = (ushort) ZigZag.Encode(vData.u[i] - prev_u);
        _v = (ushort) ZigZag.Encode(vData.v[i] - prev_v);
        _height = (ushort) ZigZag.Encode(vData.height[i] - prev_height);

        prev_u = vData.u[i];
        prev_v = vData.v[i];
        prev_height = vData.height[i];

        vData.u[i] = _u;
        listBox1.Items.Add($"uvh{i}: {_u}, {_v}, {_height}");
        vData.v[i] = _v;
        vData.height[i] = _height;
      }

      listBox1.Items.Add($"Decode");


      //Decode
      // now decode deltas and place true value back into array
      _u = 0;
      _v = 0;
      _height = 0;

      for (int i = 0; i < 4; i++)
      {
        _u += (ushort) ZigZag.Decode(vData.u[i]);
        _v += (ushort) ZigZag.Decode(vData.v[i]);
        _height += (ushort) ZigZag.Decode(vData.height[i]);

        vData.u[i] = _u;
        listBox1.Items.Add($"uvh{i}: {_u}, {_v}, {_height}");
        vData.v[i] = _v;
        vData.height[i] = _height;
      }
    }

    private async void btnFetch_Click_1(object sender, EventArgs e)
    {
      // fetch an existing tile
      ITiler tiler = new Tiler();
      var tile = await tiler.FetchTile(txtDir.Text, Int32.Parse(txtX.Text), Int32.Parse(txtX.Text),Int32.Parse(txtX.Text));
      listBox1.Items.Add($"Tile returned. Size:{tile.Length}");
    }

    private void DisplayTile(TerrainTile terrainTile)
    {
      listBox1.Items.Add("Header");
      string str = terrainTile.Header.ToString();
      string[] values = str.Split(',');
      foreach (string value in values)
      {
        if (value.Trim() == "")
          continue;
        listBox1.Items.Add(value.Trim());
      }

      listBox1.Items.Add(string.Empty);
      listBox1.Items.Add($"Number of vertices:{terrainTile.VertexData.vertexCount}");
      for (int x = 0; x < terrainTile.VertexData.vertexCount; x++)
      {
        listBox1.Items.Add(
          $"VertexId:{x}, u={terrainTile.VertexData.u[x]}, v={terrainTile.VertexData.v[x]}, hgt:{terrainTile.VertexData.height[x]}");
      }

      listBox1.Items.Add(string.Empty);
      listBox1.Items.Add($"Triangles:{terrainTile.IndexData16.triangleCount}");
      for (int x = 0; x < terrainTile.IndexData16.triangleCount; x++)
      {
        int y = x * 3;
        listBox1.Items.Add(
          $"#:{x}, ({terrainTile.IndexData16.indices[y]},{terrainTile.IndexData16.indices[y + 1]},{terrainTile.IndexData16.indices[y + 2]})");
      }

      listBox1.Items.Add(string.Empty);
      listBox1.Items.Add($"West Vertices: {terrainTile.EdgeIndices16.westVertexCount}");
      for (int x = 0; x < terrainTile.EdgeIndices16.westVertexCount; x++)
      {
        listBox1.Items.Add($"#:{x}, {terrainTile.EdgeIndices16.westIndices[x]}");
      }

      listBox1.Items.Add(string.Empty);
      listBox1.Items.Add($"South Vertices: {terrainTile.EdgeIndices16.southVertexCount}");
      for (int x = 0; x < terrainTile.EdgeIndices16.southVertexCount; x++)
      {
        listBox1.Items.Add($"#:{x}, u={terrainTile.EdgeIndices16.southIndices[x]}");
      }

      listBox1.Items.Add(string.Empty);
      listBox1.Items.Add($"East Vertices: {terrainTile.EdgeIndices16.eastVertexCount}");
      for (int x = 0; x < terrainTile.EdgeIndices16.eastVertexCount; x++)
      {
        listBox1.Items.Add($"#:{x}, u={terrainTile.EdgeIndices16.eastIndices[x]}");
      }

      listBox1.Items.Add(string.Empty);
      listBox1.Items.Add($"North Vertices: {terrainTile.EdgeIndices16.northVertexCount}");
      for (int x = 0; x < terrainTile.EdgeIndices16.northVertexCount; x++)
      {
        listBox1.Items.Add($"#:{x}, u={terrainTile.EdgeIndices16.northIndices[x]}");
      }

      listBox1.Items.Add(string.Empty);
      listBox1.Items.Add($"Normals: count {terrainTile.NormalExtensionData.vertexCount}");
      for (int x = 0; x < terrainTile.NormalExtensionData.vertexCount; x++)
      {
        listBox1.Items.Add($"#:{x}, u={terrainTile.NormalExtensionData.xy[x].ToString()}");
      }

    }


    public MemoryStream Decompress(FileInfo fileToDecompress)
    {
      using (FileStream originalFileStream = fileToDecompress.OpenRead())
      {
        using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
        {
          MemoryStream decompressedFileStream = new MemoryStream();
          decompressionStream.CopyTo(decompressedFileStream);
          return decompressedFileStream;
        }
      }
    }

    public static void Compress(DirectoryInfo directorySelected)
    {

      //  DirectoryInfo directorySelected = new DirectoryInfo(directoryPath);
      // Compress(directorySelected);

      foreach (FileInfo fileToCompress in directorySelected.GetFiles())
      {
        using (FileStream originalFileStream = fileToCompress.OpenRead())
        {
          if ((File.GetAttributes(fileToCompress.FullName) &
               FileAttributes.Hidden) != FileAttributes.Hidden & fileToCompress.Extension != ".gz")
          {
            using (FileStream compressedFileStream = File.Create(fileToCompress.FullName + ".gz"))
            {
              using (GZipStream compressionStream = new GZipStream(compressedFileStream,
                CompressionMode.Compress))
              {
                originalFileStream.CopyTo(compressionStream);

              }
            }

            FileInfo info = new FileInfo(directoryPath + Path.DirectorySeparatorChar + fileToCompress.Name + ".gz");
            Console.WriteLine(
              $"Compressed {fileToCompress.Name} from {fileToCompress.Length.ToString()} to {info.Length.ToString()} bytes.");
          }

        }
      }
    }

    private void btnView_Click(object sender, EventArgs e)
    {
      listBox1.Items.Clear();
      if (txtView.Text != String.Empty)
      {
        // reader terrain from disk
        if (chkGzip.Checked)
        {

          FileInfo myFile = new FileInfo(txtView.Text);
          var tileStream = Decompress(myFile);
          tileStream.Position = 0;
          var terrainTile = TerrainTileParser.Parse(tileStream);
          Vector3 vECEF = new Vector3(terrainTile.Header.CenterX, terrainTile.Header.CenterY, terrainTile.Header.CenterZ);
          var v3 = Coord.ecef_to_geo(vECEF);
          ToDisplay($"Tile Center XY ({MapUtil.Rad2Deg(v3.X)} , {MapUtil.Rad2Deg(v3.Y)} ,{v3.Z})");
          DisplayTile(terrainTile);
        }
        else
        {
          using (FileStream fileToDecompressAsStream = new FileStream(txtView.Text, FileMode.Open, FileAccess.Read))
          {
            byte[] bytesDisk = new byte[fileToDecompressAsStream.Length];
            fileToDecompressAsStream.Read(bytesDisk, 0, (int) fileToDecompressAsStream.Length);
            var tileStream = new MemoryStream(bytesDisk);
            var terrainTile = TerrainTileParser.Parse(tileStream);
            Vector3 vECEF = new Vector3(terrainTile.Header.CenterX, terrainTile.Header.CenterY, terrainTile.Header.CenterZ);
            var v3 = Coord.ecef_to_geo(vECEF);
            ToDisplay($"Tile Center XY ({MapUtil.Rad2Deg(v3.X)} , {MapUtil.Rad2Deg(v3.Y)} ,{v3.Z})");
            DisplayTile(terrainTile);
          }
        }
      }
    }

    private void btnClear_Click(object sender, EventArgs e)
    {
      listBox1.Items.Clear();
    }

    private void btnClipboard_Click(object sender, EventArgs e)
    {
      Clipboard.SetText(string.Join(Environment.NewLine, listBox1.Items.OfType<string>().ToArray()));
    }

    private void btnURLView_Click(object sender, EventArgs e)
    {
      var client = new HttpClient();
      var bytes = client.GetByteArrayAsync(txtURL.Text).Result;
      var stream = new MemoryStream(bytes);
      // save to disk
      using (FileStream file = new FileStream("c:\\temp\\url.terrain", FileMode.Create, FileAccess.Write))
      {
        stream.WriteTo(file);
      }

      FileInfo myFile = new FileInfo("c:\\temp\\url.terrain");
      var tileStream = Decompress(myFile);
      tileStream.Position = 0;
      var terrainTile = TerrainTileParser.Parse(tileStream);
      Vector3 vECEF = new Vector3(terrainTile.Header.CenterX, terrainTile.Header.CenterY, terrainTile.Header.CenterZ);
      var v3 = Coord.ecef_to_geo(vECEF);
      ToDisplay($"Tile Center XY ({MapUtil.Rad2Deg(v3.X)} , {MapUtil.Rad2Deg(v3.Y)} ,{v3.Z})");
      DisplayTile(terrainTile);

    }

    private void button1_Click(object sender, EventArgs e)
    {
      if (openFileDialog1.ShowDialog() == DialogResult.OK)
      {
        txtView.Text = openFileDialog1.FileName;
        btnView_Click(sender, e);
      }
    }

    private void btnLoadBM_Click(object sender, EventArgs e)
    {
      Bitmap image1;
      string filePath;
      try
      {
        // Retrieve the image.
        if (openFileDialog1.ShowDialog() == DialogResult.OK)
        {
          filePath = openFileDialog1.FileName;
        }
        else
        {
          return;
        }

        image1 = new Bitmap(filePath, true);

        int x, y;

        // Loop through the images pixels to reset color.
        for (x = 0; x < image1.Width /3; x++)
        {
          for (y = 0; y < image1.Height / 2; y++)
          {
            Color pixelColor = image1.GetPixel(x, y);
            Color newColor = Color.FromArgb(pixelColor.R, 0, 0);
            image1.SetPixel(x, y, newColor);
          }
        }

        // Set the PictureBox to display the image.
        pictureBox1.Image = image1;

        // Display the pixel format in Label1.
        listBox1.Items.Add("Pixel format: " + image1.PixelFormat.ToString());

      }
      catch (ArgumentException)
      {
        MessageBox.Show("There was an error. Check the path to the image file.");
      }

    }




    private void btnSplit_Click(object sender, EventArgs e)
    {
      var tiler = new ImageTile("c:\\map\\heightmap\\newzealand.jpg", 8, 8);
      tiler.GenerateTiles("c:\\temp\\SplitTiles");
    }



    private void btnTileCount_Click(object sender, EventArgs e)
    {
      listBox1.Items.Add($"level {txtLevel.Text} tiles:{MapUtil.NumberOfTiles(Convert.ToInt32(txtLevel.Text))}");

      listBox1.Items.Add(
        $"level {txtLevel.Text} total tiles:{MapUtil.NumberOfTotalTiles(Convert.ToInt32(txtLevel.Text))}");

    }

    private void button2_Click(object sender, EventArgs e)
    {
      listBox1.Items.Add($"Maxlevel for {txtLevel.Text} tiles:{MapUtil.MaximumLevel(Convert.ToInt32(txtLevel.Text))}");

    }

    private void btnLonLatToXY_Click(object sender, EventArgs e)
    {
      // Works 
      double lon = Convert.ToDouble(txtX4.Text);
      double lat = Convert.ToDouble(txtY4.Text);
      int zoom = Convert.ToInt32(txtZ4.Text);

      listBox1.Items.Add(Geographic.GetTileNumber(lat, lon, zoom)+ ", Geographic.GetTileNumber");
    }

    private void btnTestTileToLL_Click(object sender, EventArgs e)
    {

    }

    private async void btnBMLoad_Click(object sender, EventArgs e)
    {
      // Test to create a tile from a world terrain bitmap

      listBox1.Items.Add("Fetching DEM...");
      // class to get digital elevation models(DEM)
      BitmapDEMSource demSrc = new BitmapDEMSource();
      demSrc.Initalize(new MapHeader() {GridSize = 56});

      var elevData = await demSrc.GetDemLL(173, -43, 174, -42); //) mp.GetTerrainTileAsync(x,y,z);
      listBox1.Items.Add($"DEM length {elevData.Elev.Length}");

      var pt1 = demSrc.MapProject(-42, 173);
      var pt2 = demSrc.MapProject(-43, 174);
      listBox1.Items.Add($"Bitmap pixels:({pt1.X},{pt1.Y}) - ({pt2.X},{pt2.Y})");


      pt1 = demSrc.MapProject(-83, -180);
      pt2 = demSrc.MapProject(83, 180);
      listBox1.Items.Add($"Bitmap pixels:({pt1.X},{pt1.Y}) - ({pt2.X},{pt2.Y})");

      /*
      listBox1.Items.Add("Making Mesh...");
      // This class constructs a cesium quantized mesh from the dem
      var vertices = new Mesh().MakeFakeMesh(ref elevData);

      listBox1.Items.Add("Making Tile...");
      // This class constructs a quantized mesh tile mesh from the original mesh
      var tile = new Tiler().MakeTile(vertices,
        new TerrainTileHeader() { MinimumHeight = elevData.MinElevation, MaximumHeight = elevData.MaxElevation },
        (uint)(elevData.GridSize * elevData.GridSize * 2), elevData.GridSize);

      listBox1.Items.Add($"Tile Ready. Size:{tile.Length} . Saving to {txtPath.Text}");

      var ms = new MemoryStream(tile);
      using (FileStream fs = new FileStream(txtPath.Text, FileMode.Create))
      using (GZipStream zipStream = new GZipStream(fs, CompressionMode.Compress, false))
      {
        zipStream.Write(ms.ToArray(), 0, ms.ToArray().Length); // .Write(bytes, 0, bytes.Length);
      }
      */
    }

    /// <summary>
    /// Full Test on Bitmap tiler
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void btnMakeBMTile_Click(object sender, EventArgs e)
    {
      int x = Convert.ToInt32(txtX2.Text);
      int y = Convert.ToInt32(txtY2.Text);
      int zoom = Convert.ToInt32(txtZ2.Text);
      listBox1.Items.Add("Fetching DEM...");

      // class to get digital elevation models(DEM)

      //BitmapDEMSource demSrc = new BitmapDEMSource();
     TRexDEMSource demSrc = new TRexDEMSource();

      demSrc.Initalize(new MapHeader()
        {GridSize = Convert.ToInt32(txtGridSizeBM.Text), MinElevation = 0, MaxElevation = 1000});
      var elevData = await demSrc.GetDemXYZ(x, y, zoom);

      listBox1.Items.Add($"DEM length {elevData.Elev.Length}");
      listBox1.Items.Add("Making Mesh...");

      // This class constructs a cesium quantized mesh from the dem
      var vertices = new Mesh().MakeFakeMesh(ref elevData);

      listBox1.Items.Add("Making Tile...");

      // This class constructs a quantized mesh tile mesh from the original mesh

      var hdr = new TerrainTileHeader()
      {
        BoundingSphereCenterX = elevData.BoundingSphereCenterX,
        BoundingSphereCenterY = elevData.BoundingSphereCenterY,
        BoundingSphereCenterZ = elevData.BoundingSphereCenterZ,
        CenterX = elevData.CenterX,
        CenterY = elevData.CenterY,
        CenterZ = elevData.CenterZ,
        HorizonOcclusionPointX = elevData.HorizonOcclusionPointX,
        HorizonOcclusionPointY = elevData.HorizonOcclusionPointY,
        HorizonOcclusionPointZ = elevData.HorizonOcclusionPointZ,
        BoundingSphereRadius = elevData.BoundingSphereRadius,
        MinimumHeight = elevData.MinimumHeight,
        MaximumHeight = elevData.MaximumHeight
      };

      var tile = new Tiler().MakeTile(vertices, hdr, MapUtil.GridSizeToTriangleCount(elevData.GridSize),
        elevData.GridSize);

      listBox1.Items.Add($"Tile Ready. Size:{tile.Length} . Saving to {txtBMPath.Text}");

      var ms = new MemoryStream(tile);
      using (FileStream fs = new FileStream(txtBMPath.Text, FileMode.Create))
      using (GZipStream zipStream = new GZipStream(fs, CompressionMode.Compress, false))
      {
        zipStream.Write(ms.ToArray(), 0, ms.ToArray().Length); // .Write(bytes, 0, bytes.Length);
      }

      // Open View
      txtView.Text = txtBMPath.Text;
      btnView_Click(sender, e);

    }


    public ushort QuantizeHeight(float min, float max, float value)
    {
      // We cant have values higher or lower then min max
      if (value >= max)
        return 32767; // max tile height

      if (value <= min)
        return 0; // min tile height

      return (ushort) ((value - min) * (32768 / (max - min)));
    }



    public static int QuantizeHeight2(
      float originalStart, float originalEnd, // original range
      int newStart, int newEnd, // desired range
      float value) // value to convert
    {
      if (value >= originalEnd)
        return 32767; // max tile height

      if (value <= originalStart)
        return 0; // min tile height

      double scale = (double) (newEnd - newStart) / (originalEnd - originalStart);
      return (int) (newStart + ((value - originalStart) * scale));
    }


    public static ushort QuantizeHeight3(float origStart, float origEnd, // original range
      float value) // value to convert
    {
      if (value >= origEnd)
        return 32767; // max tile height
      if (value <= origStart)
        return 0; // min tile height
      // both scales are zero based
      return (ushort) ((value - origStart) * ((double) 32767 / (origEnd - origStart)));
    }


    private void btnQHgt_Click(object sender, EventArgs e)
    {
      var hgt = QuantizeHeight3(0, 8000, Convert.ToSingle(txtHgt.Text));
      // var hgt = QuantizeHeight2(0, 8000, 0,32767,Convert.ToSingle(txtHgt.Text));
      listBox1.Items.Add($"Range:0-8000, Qheight:{hgt}");

    }

    private void btnXYZ_Click(object sender, EventArgs e)
    {
      int lon = Convert.ToInt32(txtX5.Text);
      int lat = Convert.ToInt32(txtY5.Text);
      int zoom = Convert.ToInt32(txtZ5.Text);
      var bb = Geographic.TileXYToRectangleLL(lon, lat,zoom);
      ToDisplay(bb.ToDisplay());

      var midPt = bb.GetCenter();
      ToDisplay($"Mo=idPoint XY ({midPt.Longitude} , {midPt.Latitude})");
      var pt =  Coord.geo_to_ecef(new Vector3( MapUtil.Deg2Rad(midPt.Longitude), MapUtil.Deg2Rad(midPt.Latitude),1)); // zero elevation for now
      //var pt = MapUtil.LatLonToEcef(midPt.Latitude, midPt.Longitude, 0); // zero elevation for now

      ToDisplay($"CenterXYZ {pt.X}, {pt.Y}, {pt.Z}");

    }


    private int getNumberOfXTilesAtLevel(int zoom)
    {
      return (2 << zoom);
      //return this._numberOfLevelZeroTilesX << level;
    }

    private int getNumberOfYTilesAtLevel(int zoom)
    {
      return (1 << zoom);
      //return this._numberOfLevelZeroTilesX << level;
    }


    private void btnNZ_Click(object sender, EventArgs e)
    {

    }


    public Double FindDifference(Double nr1, Double nr2)
    {
      return Math.Abs(nr1 - nr2);
    }

    private void button4_Click(object sender, EventArgs e)
    {

      int lon = Convert.ToInt32(txtX6.Text);
      int lat = Convert.ToInt32(txtY6.Text);
      int zoom = Convert.ToInt32(txtZ6.Text);


      // crap  var t = CesiumMerc.TileXYToNativeRectangle(lon, lat, zoom);
      //  listBox1.Items.Add($" CesiumMerc (WS ,EN ({t.West} , {t.South}) - ({t.East} , {t.North})");
 //     var long1 = MapUtil.tile2long(lon, zoom);
  //    var lat1 = MapUtil.tile2lat(lat, zoom);
  //    listBox1.Items.Add($" Please Lat lon {lat1} , {long1})");

      var bb2 = Geographic.TileXYToRectangleLL(lon, lat, zoom);
      listBox1.Items.Add($"Geographic.TileXYToRectangleLL WS ,EN ({bb2.West} , {bb2.South}) - ({bb2.East} , {bb2.North})");

      var bb = MapGeo.TileXYZToRectLL(lon, lat, zoom);
      listBox1.Items.Add($"MapGeo.TileXYZToRectLL WS ,EN ({bb.West} , {bb.South}) - ({bb.East} , {bb.North})");
      listBox1.Items.Add("");

      // Now convert to ECEF coords
      var res2 = Coord.geo_to_ecef(new Vector3(MapUtil.Deg2Rad(bb.West), MapUtil.Deg2Rad(bb.South), 10));
      listBox1.Items.Add($"WS to Meters EcEf XYZ:{res2.X} , {res2.Y} , {res2.Z}");
      var res3 = Coord.geo_to_ecef(new Vector3(MapUtil.Deg2Rad(bb.East), MapUtil.Deg2Rad(bb.North), 10));
      listBox1.Items.Add($"EN to Meters EcEf XYZ:{res3.X} , {res3.Y} , {res3.Z}");

      var x = res2.X + FindDifference(res3.X, res2.X) / 2;
      var y = res2.Y + FindDifference(res3.Y, res2.Y) / 2;

      listBox1.Items.Add($"Center XY:{x} , {y}");



    }

    private void button5_Click(object sender, EventArgs e)
    {
      /* max boundry in radians
      double south = -1.4844222297453324;
      double north = 1.4844222297453322;
      double east = 3.141592653589793;
      double west = -3.141592653589793;
      listBox1.Items.Add($"Val {MapUtil.Rad2Deg(west)}");
      listBox1.Items.Add($"Val {MapUtil.Rad2Deg(south)}");
      listBox1.Items.Add($"Val {MapUtil.Rad2Deg(east)}");
      listBox1.Items.Add($"Val {MapUtil.Rad2Deg(north)}");
      */
      double lon = MapUtil.Deg2Rad(Convert.ToDouble(txtX4.Text));
      double lat = MapUtil.Deg2Rad(Convert.ToDouble(txtY4.Text));
      int zoom = Convert.ToInt32(txtZ4.Text);

//      var res = CesiumMerc.PositionToTileXY(new PointD(lon, lat), zoom);
      var res = CesiumMerc.PositionToTileXY(new PointD(lat,lon), zoom);

      listBox1.Items.Add($"ZXY {(zoom)} / {res.X} / {res.Y}"+ ",  CesiumMerc.PositionToTileXY");

    }

    private void button6_Click(object sender, EventArgs e)
    {
      // Slippymap Lat Lon to Tile
      double lon = Convert.ToDouble(txtX4.Text);
      double lat = Convert.ToDouble(txtY4.Text);
      int zoom = Convert.ToInt32(txtZ4.Text);
      var res = MapUtil.WorldToTilePos(lon, lat, zoom);

      listBox1.Items.Add($"ZXY {(zoom)} / {res.X} / {res.Y}"+ ", MapUtil.WorldToTilePos");
      
    }

    private void button7_Click(object sender, EventArgs e)
    {
      int lon = Convert.ToInt32(txtX6.Text);
      int lat = Convert.ToInt32(txtY6.Text);
      int zoom = Convert.ToInt32(txtZ6.Text);

      var bb = MapUtil.TileToWorldPos(lon, lat, zoom);

      listBox1.Items.Add($"WS ,EN ({bb.X} , {bb.Y})");
    }

    private void button8_Click(object sender, EventArgs e)
    {
      /*
       * BoundingSphere bs = new BoundingSphere();
       */
      /*
        Matrix mt = new Matrix(10.0,10.0,10.0,10.0,10.0,10.0,100.0,100.0,100.0,100.0,100.0,100.0,100.0,10.0,10.0,10.0);
        BoundingSphere bs2 = new BoundingSphere();
  
        bs.Transform(ref mt,out bs2);
        listBox1.Items.Add($"Center:{bs2.Center}, Radius:{bs2.Radius}");
        */
  //    double x;
   //   double y;
    //  double z;

      listBox1.Items.Add("Test2 MapUtil.LatLonToEcef");
      // This works
      var v3 = MapUtil.LatLonToEcef(-43, 173, 0);
      listBox1.Items.Add($"-43.0,173.0 to Meters EcEf:{v3.X},{v3.Y},{v3.Z}");

      // This works
      listBox1.Items.Add("Test3 Coord.geo_to_ecef");
      var res2 = Coord.geo_to_ecef(new Vector3(MapUtil.Deg2Rad(173.0), MapUtil.Deg2Rad(-43.0), 0));
      listBox1.Items.Add($"173.0,-43.0, 0 to Meters EcEf XYZ:{res2.X},{res2.Y},{res2.Z}");

      listBox1.Items.Add("And back Coord.ecef_to_geo");
      var res3 = Coord.ecef_to_geo(res2);
      listBox1.Items.Add($"EcEf to LLA:{MapUtil.Rad2Deg(res3.X)},{MapUtil.Rad2Deg(res3.Y)},{res3.Z}");
    }

    private async void button9_Click(object sender, EventArgs e)
    {
      // Make Bitmap Tile
      listBox1.Items.Add("Fetching DEM...");
      // class to get digital elevation models(DEM)
      BitmapDEMSource demSrc = new BitmapDEMSource();

      demSrc.Initalize(new MapHeader() {GridSize = Int32.Parse(txtTileSize.Text)});

      var elevData = await demSrc.GetDemXYZ(501, 190, 8); //) mp.GetTerrainTileAsync(x,y,z);
      listBox1.Items.Add($"DEM length {elevData.Elev.Length}");

      listBox1.Items.Add("Making Mesh...");
      // This class constructs a cesium quantized mesh from the dem
      var vertices = new Mesh().MakeFakeMesh(ref elevData);

      listBox1.Items.Add("Making Tile...");
      // This class constructs a quantized mesh tile mesh from the original mesh
      var tile = new Tiler().MakeTile(vertices,
        new TerrainTileHeader() {MinimumHeight = elevData.MinimumHeight, MaximumHeight = elevData.MaximumHeight},
        MapUtil.GridSizeToTriangleCount(elevData.GridSize), elevData.GridSize);

      listBox1.Items.Add($"Tile Ready. Size:{tile.Length} . Saving to {txtPath.Text}");

      var ms = new MemoryStream(tile);
      using (FileStream fs = new FileStream(txtPath.Text, FileMode.Create))
      using (GZipStream zipStream = new GZipStream(fs, CompressionMode.Compress, false))
      {
        zipStream.Write(ms.ToArray(), 0, ms.ToArray().Length); // .Write(bytes, 0, bytes.Length);
      }
    }

    private void button10_Click(object sender, EventArgs e)
    {
      // Calculate Header Info
      LLBoundingBox bb = new LLBoundingBox(173, -44, 174, -43,false);
    //  Double Alt = 0;

      var v1 = MapUtil.LatLonToEcef(bb.South, bb.West, 0);
      listBox1.Items.Add($"WS Meters:{v1.X} , {v1.Y}");

      var v2 = MapUtil.LatLonToEcef(bb.North, bb.East, 0);
      listBox1.Items.Add($"EN Meters:{v2.X} , {v2.Y}");

      var v3 = MapUtil.LatLonToEcef(-43.5, 173.5, 0);
      listBox1.Items.Add($"EN Meters:{v3.X} , {v3.Y}");

      //var midpt = MapUtil.MidPointLL(new MapPoint(bb.West, bb.South), new MapPoint(bb.East, bb.North));
     // listBox1.Items.Add($"Midpoint:{MapUtil.Rad2Deg(midpt.Longitude)} , {MapUtil.Rad2Deg(midpt.Latitude)}");

      //var v4 = MapUtil.LatLonToEcef(MapUtil.Rad2Deg(midpt.Latitude), MapUtil.Rad2Deg(midpt.Longitude), 0);

      //listBox1.Items.Add($"Midpoint Meters:{v4.X} , {v4.Y}");

    }

    private void button3_Click(object sender, EventArgs e)
    {
    }

    private void ToDisplay(String msg)
    {
       listBox1.Items.Add(msg);
    }

    private void btnTileInfo_Click(object sender, EventArgs e)
    {

      Vector3[] v3 = new Vector3[5];

     // python demo data in ECEF coordinates
      v3[0] = new Vector3(4480363.47619142 , 580530.98724711, 4487574.82445706);
      v3[1] = new Vector3(4507665.32245697,  584068.55018155, 4459851.33224541);
      v3[2] = new Vector3(4492226.98875439,  596087.14785771, 4473732.87770479);
      v3[3] = new Vector3(4504029.82280181,  611720.56972904,4459884.3617184);
      v3[4] = new Vector3(4476724.14024925,  608012.01353714, 4487581.96623555);
      
      
      TileInfo tf = new TileInfo();
      var hdr = tf.CalculateHeaderInfo(ref v3, false);
      var hop = HorizonOcclusionPoint.FromPoints(v3, tf.BoundingSphere);


// python demo      var hdr = tf.CalculateHeaderInfo(ref v3, false);

      listBox1.Items.Add($"Header Info CenterXYZ:{hdr.CenterX} , {hdr.CenterY}, {hdr.CenterZ}");
      listBox1.Items.Add($"Header Info HorizonOcclusionPoint: {hop.X} , {hop.Y} , {hop.Z}");

    }

    private void btnMyData_Click(object sender, EventArgs e)
    {
      // Array of Lat Long
      Vector3[] v3 = new Vector3[16];

      v3[0] = new Vector3(7.3828125, 44.6484375, 303.3);
      v3[1] = new Vector3(7.3828125, 45.0, 320.2);
      v3[2] = new Vector3(7.5585937, 44.82421875, 310.2);
      v3[3] = new Vector3(7.3828125, 44.6484375, 303.3);
      v3[4] = new Vector3(7.3828125, 44.6484375, 303.3);
      v3[5] = new Vector3(7.734375, 44.6484375, 350.3);
      v3[6] = new Vector3(7.5585937, 44.82421875, 310.2);
      v3[7] = new Vector3(7.3828125, 44.6484375, 303.3);
      v3[8] = new Vector3(7.734375, 44.6484375, 350.3);
      v3[9] = new Vector3(7.734375, 45.0, 330.3);
      v3[10] = new Vector3(7.5585937, 44.82421875, 310.2);
      v3[11] = new Vector3(7.734375, 44.6484375, 350.3);
      v3[12] = new Vector3(7.734375, 45.0, 330.3);
      v3[13] = new Vector3(7.5585937, 44.82421875, 310.2);
      v3[14] = new Vector3(7.3828125, 45.0, 320.2);
      v3[15] = new Vector3(7.734375, 45.0, 330.3);

      // Convert to ECEF coords
      Vector3[] points = new Vector3[5];
      for (int i = 0; i < 5; i++)
      {
        points[i] = Coord.geo_to_ecef(v3[i]);
      }

      // Now get tile header info
      TileInfo tf = new TileInfo();
      var hdr = tf.CalculateHeaderInfo(ref points, false);
      var hop = HorizonOcclusionPoint.FromPoints(points, tf.BoundingSphere);

      listBox1.Items.Add($"Header Info CenterXYZ:{hdr.CenterX} , {hdr.CenterY}, {hdr.CenterZ}");
      listBox1.Items.Add($"Header Info HorizonOcclusionPoint: {hop.X} , {hop.Y} , {hop.Z}");
    }

    private void btnCenter_Click(object sender, EventArgs e)
    {
      LLBoundingBox bb = new LLBoundingBox(170,-43,180,-44,false);
      var t1 =bb.GetCenter();
      listBox1.Items.Add($"BB Center Test1 Not Radians {t1.Longitude} , {t1.Latitude}");
      LLBoundingBox bb2 = new LLBoundingBox(MapUtil.Deg2Rad(170), MapUtil.Deg2Rad(-43), MapUtil.Deg2Rad(180), MapUtil.Deg2Rad(-44));
      var t2 = bb2.GetCenter();
      listBox1.Items.Add($"BB Center Test2 Not Radians {MapUtil.Rad2Deg( t2.Longitude)} , {MapUtil.Rad2Deg(t2.Latitude)}");

      var t3 = MapUtil.MidPointRad(new MapPoint(MapUtil.Deg2Rad(170.0), MapUtil.Deg2Rad(-43)), new MapPoint(MapUtil.Deg2Rad(180.0), MapUtil.Deg2Rad(-44.0)));
      listBox1.Items.Add($"BB Center Test3 Not Radians {MapUtil.Rad2Deg(t3.Longitude)} , {MapUtil.Rad2Deg(t3.Latitude)}");

    }

    private void btnDistance_Click(object sender, EventArgs e)
    {
      // Winner is  MapUtil.GetDistance for pole to pole
      listBox1.Items.Add("");
      var bb = new LLBoundingBox(0,0,0,0,false);

      bb.West = -180.0;bb.East = -179.0;bb.South = 0.0; bb.North = 0.0;
      var d1 = MapUtil.GetDistance(bb.West,bb.South,bb.East,bb.North);
      listBox1.Items.Add($"Distance1:{d1}, (W,S - E,N) ,({bb.West} , {bb.South}) - ({bb.East} , {bb.North})");
      Char myChar = 'K';
      var d2 = MapUtil.Distance3(bb.West, bb.South, bb.East, bb.South, myChar);
      listBox1.Items.Add($"Distance2:{d2}, (W,S - E,N) ,({bb.West} , {bb.South}) - ({bb.East} , {bb.North})");
      var d3 = MapUtil.DistanceTo(bb.West, bb.South, bb.East, bb.South, myChar);
      listBox1.Items.Add($"Distance3:{d3}, (W,S - E,N) ,({bb.West} , {bb.South}) - ({bb.East} , {bb.North})");

      bb.West = -180.0; bb.East = -179.0; bb.South = -45.0; bb.North = -45.0;
      d1 = MapUtil.GetDistance(bb.West, bb.South, bb.East, bb.North);
      listBox1.Items.Add($"Distance1:{Math.Round(d1,6)}, (W,S - E,N) ,({bb.West} , {bb.South}) - ({bb.East} , {bb.North})");
      d2 = MapUtil.Distance3(bb.West, bb.South, bb.East, bb.South, myChar);
      listBox1.Items.Add($"Distance2:{Math.Round(d2,6)}, (W,S - E,N) ,({bb.West} , {bb.South}) - ({bb.East} , {bb.North})");
      d3 = MapUtil.DistanceTo(bb.West, bb.South, bb.East, bb.South, myChar);
      listBox1.Items.Add($"Distance3:{Math.Round(d3,6)}, (W,S - E,N) ,({bb.West} , {bb.South}) - ({bb.East} , {bb.North})");

      bb.West = 0.0; bb.East = 1.0; bb.South = 0.0; bb.North = 0.0;
      d1 = MapUtil.GetDistance(bb.West, bb.South, bb.East, bb.North);
      listBox1.Items.Add($"Distance1:{Math.Round(d1, 6)}, (W,S - E,N) ,({bb.West} , {bb.South}) - ({bb.East} , {bb.North})");
      d2 = MapUtil.Distance3(bb.West, bb.South, bb.East, bb.South, myChar);
      listBox1.Items.Add($"Distance2:{Math.Round(d2, 6)}, (W,S - E,N) ,({bb.West} , {bb.South}) - ({bb.East} , {bb.North})");
      d3 = MapUtil.DistanceTo(bb.West, bb.South, bb.East, bb.South, myChar);
      listBox1.Items.Add($"Distance3:{Math.Round(d3, 6)}, (W,S - E,N) ,({bb.West} , {bb.South}) - ({bb.East} , {bb.North})");

      bb.West = -179.0; bb.East = 170.0; bb.South = 0.0; bb.North = 0.0;
      d1 = MapUtil.GetDistance(bb.West, bb.South, bb.East, bb.North);
      listBox1.Items.Add($"Distance1:{Math.Round(d1, 6)}, (W,S - E,N) ,({bb.West} , {bb.South}) - ({bb.East} , {bb.North})");
      d2 = MapUtil.Distance3(bb.West, bb.South, bb.East, bb.South, myChar);
      listBox1.Items.Add($"Distance2:{Math.Round(d2, 6)}, (W,S - E,N) ,({bb.West} , {bb.South}) - ({bb.East} , {bb.North})");
      d3 = MapUtil.DistanceTo(bb.West, bb.South, bb.East, bb.South, myChar);
      listBox1.Items.Add($"Distance3:{Math.Round(d3, 6)}, (W,S - E,N) ,({bb.West} , {bb.South}) - ({bb.East} , {bb.North})");

      bb.West = -90.0; bb.East = 90.0; bb.South = 0.0; bb.North = 0.0;
      d1 = MapUtil.GetDistance(bb.West, bb.South, bb.East, bb.North);
      listBox1.Items.Add($"Distance1:{Math.Round(d1, 6)}, (W,S - E,N) ,({bb.West} , {bb.South}) - ({bb.East} , {bb.North})");
      d2 = MapUtil.Distance3(bb.West, bb.South, bb.East, bb.South, myChar);
      listBox1.Items.Add($"Distance2:{Math.Round(d2, 6)}, (W,S - E,N) ,({bb.West} , {bb.South}) - ({bb.East} , {bb.North})");
      d3 = MapUtil.DistanceTo(bb.West, bb.South, bb.East, bb.South, myChar);
      listBox1.Items.Add($"Distance3:{Math.Round(d3, 6)}, (W,S - E,N) ,({bb.West} , {bb.South}) - ({bb.East} , {bb.North})");

      bb.West = 1.0; bb.East = 1.1; bb.South = 0.0; bb.North = 1.0;
      d1 = MapUtil.GetDistance(bb.West, bb.South, bb.East, bb.North);
      listBox1.Items.Add($"Distance1:{Math.Round(d1, 6)}, (W,S - E,N) ,({bb.West} , {bb.South}) - ({bb.East} , {bb.North})");
      d2 = MapUtil.Distance3(bb.West, bb.South, bb.East, bb.South, myChar);
      listBox1.Items.Add($"Distance2:{Math.Round(d2, 6)}, (W,S - E,N) ,({bb.West} , {bb.South}) - ({bb.East} , {bb.North})");
      d3 = MapUtil.DistanceTo(bb.West, bb.South, bb.East, bb.South, myChar);
      listBox1.Items.Add($"Distance3:{Math.Round(d3, 6)}, (W,S - E,N) ,({bb.West} , {bb.South}) - ({bb.East} , {bb.North})");
    }
  }
}


