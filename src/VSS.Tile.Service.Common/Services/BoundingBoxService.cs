using System;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Models;
using VSS.Tile.Service.Common.Helpers;
using VSS.Tile.Service.Common.Models;


namespace VSS.Tile.Service.Common.Services
{
  /// <summary>
  /// Used to calculate the map bounding box for the report.
  /// </summary>
  public class BoundingBoxService : IBoundingBoxService
  {
    private readonly ILogger log;

    public BoundingBoxService(ILoggerFactory logger)
    {
      log = logger.CreateLogger<ProjectTileService>();
    }

    /// <summary>
    /// Adjust the bounding box to fit the requested tile size.
    /// </summary>
    /// <param name="parameters">The map parameters icluding the bounding box.</param>
    public void AdjustBoundingBoxToFit(MapParameters parameters)
    {
      log.LogInformation($"AdjustBoundingBoxToFit: requestedWidth={parameters.mapWidth}, requestedHeight={parameters.mapHeight}, bbox={parameters.bbox}");

      TryZoomIn(parameters, out int requiredWidth, out int requiredHeight, out var pixelMin, out var pixelMax);

      bool adjust = false;

      const double MARGIN_FACTOR = 1.1; //10% margin
      var adjustedRequiredWidth = parameters.addMargin ? (int)Math.Round(requiredWidth * MARGIN_FACTOR) : requiredWidth;
      var adjustedRequiredHeight = parameters.addMargin ? (int)Math.Round(requiredHeight * MARGIN_FACTOR) : requiredHeight;

      //Is it bigger or smaller than the requested size?
      if (adjustedRequiredWidth > parameters.mapWidth || adjustedRequiredHeight > parameters.mapHeight)
      {
        log.LogDebug("AdjustBoundingBoxToFit: scaling down");

        //We'll make the bounding box bigger in the same shape as the requested tile and scale down once the tile has been drawn. 
        adjust = true;
        //Need to maintain aspect ratio. Figure out the ratio.
        double ratioX = (double)requiredWidth / (double)parameters.mapWidth;
        double ratioY = (double)requiredHeight / (double)parameters.mapHeight;
        // use whichever multiplier is bigger
        double ratio = ratioX > ratioY ? ratioX : ratioY;

        // now we can get the new height and width
        var factor = parameters.addMargin ? MARGIN_FACTOR : 1.0; 
        int newHeight = Convert.ToInt32(parameters.mapHeight * ratio * factor);
        int newWidth = Convert.ToInt32(parameters.mapWidth * ratio * factor);

        var xDiff = Math.Abs(newWidth - requiredWidth) / 2;
        var yDiff = Math.Abs(newHeight - requiredHeight) / 2;

        //Pixel origin is top left
        pixelMin.x -= xDiff;
        pixelMax.x += xDiff;
        pixelMin.y += yDiff;
        pixelMax.y -= yDiff;

        //Adjust the tile width & height
        parameters.mapWidth = (int)Math.Abs(pixelMax.x - pixelMin.x);
        parameters.mapHeight = (int)Math.Abs(pixelMax.y - pixelMin.y);
      }
      else
      {
        log.LogDebug("AdjustBoundingBoxToFit: expanding to fill tile ");
  
        //Expand the bounding box to fill the requested tile size
        if (adjustedRequiredWidth < parameters.mapWidth)
        {
          double scaleWidth = (double)parameters.mapWidth / adjustedRequiredWidth;
          adjustedRequiredWidth = (int)(scaleWidth * adjustedRequiredWidth);
          double pixelCenterX = pixelMin.x + (pixelMax.x - pixelMin.x) / 2.0;
          //Pixel origin is top left
          pixelMin.x = pixelCenterX - adjustedRequiredWidth / 2.0;
          pixelMax.x = pixelCenterX + adjustedRequiredWidth / 2.0;
          adjust = true;
        }

        if (adjustedRequiredHeight < parameters.mapHeight)
        {
          double scaleHeight = (double)parameters.mapHeight / adjustedRequiredHeight;
          adjustedRequiredHeight = (int)(scaleHeight * adjustedRequiredHeight);
          double pixelCenterY = pixelMin.y + (pixelMax.y - pixelMin.y) / 2.0;
          //Pixel origin is top left
          pixelMin.y = pixelCenterY + adjustedRequiredHeight / 2.0;
          pixelMax.y = pixelCenterY - adjustedRequiredHeight / 2.0;
          adjust = true;
        }
      }

      if (adjust)
      {
        //Convert the adjusted bbox to lat/lng
        var minLatLngDegrees = WebMercatorProjection.PixelToLatLng(pixelMin, parameters.numTiles);
        var maxLatLngDegrees = WebMercatorProjection.PixelToLatLng(pixelMax, parameters.numTiles);
        parameters.bbox.minLat = minLatLngDegrees.Latitude.LatDegreesToRadians();
        parameters.bbox.maxLat = maxLatLngDegrees.Latitude.LatDegreesToRadians();
        parameters.bbox.minLng = minLatLngDegrees.Longitude.LonDegreesToRadians();
        parameters.bbox.maxLng = maxLatLngDegrees.Longitude.LonDegreesToRadians();
      }
      log.LogInformation($"AdjustBoundingBoxToFit: returning mapWidth={parameters.mapWidth}, mapHeight={parameters.mapHeight}, bbox={parameters.bbox}");
    }

    private void TryZoomIn(MapParameters parameters, out int requiredWidth, out int requiredHeight, out Point pixelMin, out Point pixelMax)
    {
      pixelMin = TileServiceUtils.LatLngToPixel(parameters.bbox.minLat, parameters.bbox.minLng, parameters.numTiles);
      pixelMax = TileServiceUtils.LatLngToPixel(parameters.bbox.maxLat, parameters.bbox.maxLng, parameters.numTiles);

      requiredWidth = (int)Math.Abs(pixelMax.x - pixelMin.x);
      requiredHeight = (int)Math.Abs(pixelMax.y - pixelMin.y);

      //See if we can zoom in - occurs when the requested tile size is much larger than the bbox
      var zoomedWidth = requiredWidth;
      var zoomedHeight = requiredHeight;
      int zoomLevel = parameters.zoomLevel;
      Point zoomedPixelMin = pixelMin;
      Point zoomedPixelMax = pixelMax;
      long numTiles = parameters.numTiles;

      //allow a 15% margin extra otherwise if the tile is only a few pixels bigger than the calculated zoom
      //we use the smaller zoom level and end up with lots of space around the data.
      //AdjustBoundingBoxToFit handles the bigger size.
      var mapWidth = parameters.mapWidth * 1.15;
      var mapHeight = parameters.mapHeight * 1.15;

      while (zoomedWidth < mapWidth && zoomedHeight < mapHeight)
      {
        parameters.zoomLevel = zoomLevel;
        parameters.numTiles = numTiles;
        requiredWidth = zoomedWidth;
        requiredHeight = zoomedHeight;
        pixelMin = zoomedPixelMin;
        pixelMax = zoomedPixelMax;

        zoomLevel++;
        numTiles = TileServiceUtils.NumberOfTiles(zoomLevel);

        zoomedPixelMin = TileServiceUtils.LatLngToPixel(parameters.bbox.minLat, parameters.bbox.minLng, numTiles);
        zoomedPixelMax = TileServiceUtils.LatLngToPixel(parameters.bbox.maxLat, parameters.bbox.maxLng, numTiles);

        zoomedWidth = (int)Math.Abs(zoomedPixelMax.x - zoomedPixelMin.x);
        zoomedHeight = (int)Math.Abs(zoomedPixelMax.y - zoomedPixelMin.y);
      }
    }

  }


  public interface IBoundingBoxService
  {
    void AdjustBoundingBoxToFit(MapParameters parameters);
  }

  
}
