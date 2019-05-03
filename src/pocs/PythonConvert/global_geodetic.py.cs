//  This module defines the :class:`quantized_mesh_tile.global_geodetic.GlobalGeodetic`.
// Initial code from:
// https://svn.osgeo.org/gdal/trunk/gdal/swig/python/scripts/gdal2tiles.py
// Functions necessary for generation of global tiles in Plate Carre projection,
// EPSG:4326, unprojected profile.
// Pixel and tile coordinates are in TMS notation (origin [0,0] in bottom-left).
// What coordinate conversions do we need for TMS Global Geodetic tiles?
// Global Geodetic tiles are using geodetic coordinates (latitude,longitude)
// directly as planar coordinates XY (it is also called Unprojected or Plate
// Carre). We need only scaling to pixel pyramid and cutting to tiles.
// Pyramid has on top level two tiles, so it is not square but rectangle.
// Area [-180,-90,180,90] is scaled to 512x256 pixels.
// TMS has coordinate origin (for pixels and tiles) in bottom-left corner.
// 
// Reference
// ---------
// 

using @division = @@__future__.division;

using range = builtins.range;

using object = builtins.object;

using old_div = past.utils.old_div;

using math;

using System;

using System.Linq;

public static class global_geodetic {
    
    public static int MAXZOOMLEVEL = 32;
    
    // 
    //     Contructor arguments:
    // 
    //     ``tmscompatible``
    // 
    //         If set to True, defaults the resolution factor to 0.703125 (2 tiles @ level 0)
    //         Adhers to OSGeo TMS spec and therefore Cesium.
    //         http://wiki.osgeo.org/wiki/Tile_Map_Service_Specification#global-geodetic
    //         If set to False, defaults the resolution factor to 1.40625 (1 tile @ level 0)
    //         Adheres OpenLayers, MapProxy, etc default resolution for WMTS.
    // 
    //     ``tileSize``
    // 
    //         The size of the tile in pixel. Default is `256`.
    // 
    //     
    public class GlobalGeodetic
        : object {
        
        public int _numberOfLevelZeroTilesX;
        
        public int _numberOfLevelZeroTilesY;
        
        public object resFact;
        
        public object tileSize;
        
        public GlobalGeodetic(object tmscompatible, object tileSize = 256) {
            this.tileSize = tileSize;
            if (tmscompatible != null) {
                this.resFact = old_div(180.0, this.tileSize);
                this._numberOfLevelZeroTilesX = 2;
                this._numberOfLevelZeroTilesY = 1;
            } else {
                this.resFact = old_div(360.0, this.tileSize);
                this._numberOfLevelZeroTilesX = 1;
                this._numberOfLevelZeroTilesY = 1;
            }
        }
        
        // Converts lon/lat to pixel coordinates in given zoom of the EPSG:4326 pyramid
        public virtual object LonLatToPixels(object lon, object lat, object zoom) {
            var res = old_div(this.resFact, Math.Pow(2, zoom));
            var px = old_div(180 + lon, res);
            var py = old_div(90 + lat, res);
            return Tuple.Create(px, py);
        }
        
        // Returns coordinates of the tile covering region in pixel coordinates
        public virtual object PixelsToTile(object px, object py) {
            var tx = px > 0 ? Convert.ToInt32(math.ceil(old_div(px, float(this.tileSize))) - 1) : 0;
            var ty = py > 0 ? Convert.ToInt32(math.ceil(old_div(py, float(this.tileSize))) - 1) : 0;
            return Tuple.Create(tx, ty);
        }
        
        // Returns the tile for zoom which covers given lon/lat coordinates
        public virtual object LonLatToTile(object lon, object lat, object zoom) {
            var _tup_1 = this.LonLatToPixels(lon, lat, zoom);
            var px = _tup_1.Item1;
            var py = _tup_1.Item2;
            return this.PixelsToTile(px, py);
        }
        
        // Resolution (arc/pixel) for given zoom level (measured at Equator)
        public virtual object Resolution(object zoom) {
            return old_div(this.resFact, Math.Pow(2, zoom));
            // return 180 / float( 1 << (8+zoom) )
        }
        
        // Maximal scaledown zoom of the pyramid closest to the pixelSize.
        public virtual object ZoomForPixelSize(object pixelSize) {
            foreach (var i in Enumerable.Range(0, MAXZOOMLEVEL)) {
                if (pixelSize > this.Resolution(i)) {
                    if (i != 0) {
                        return i - 1;
                    } else {
                        return 0;
                    }
                }
            }
        }
        
        // Returns bounds of the given tile
        public virtual object TileBounds(object tx, object ty, object zoom) {
            var res = old_div(this.resFact, Math.Pow(2, zoom));
            return Tuple.Create(tx * this.tileSize * res - 180, ty * this.tileSize * res - 90, (tx + 1) * this.tileSize * res - 180, (ty + 1) * this.tileSize * res - 90);
        }
        
        // Returns bounds of the given tile in the SWNE form
        public virtual object TileLatLonBounds(object tx, object ty, object zoom) {
            var b = this.TileBounds(tx, ty, zoom);
            return Tuple.Create(b[1], b[0], b[3], b[2]);
        }
        
        // Returns the number of tiles over x at a given zoom level (only 256px)
        public virtual object GetNumberOfXTilesAtZoom(object zoom) {
            return this._numberOfLevelZeroTilesX << zoom;
        }
        
        // Returns the number of tiles over y at a given zoom level (only 256px)
        public virtual object GetNumberOfYTilesAtZoom(object zoom) {
            return this._numberOfLevelZeroTilesY << zoom;
        }
    }
}
