// 
// This module provides high level utility functions to encode and decode a terrain tile.
// 
// Reference
// ---------
// 

using TerrainTile = terrain.TerrainTile;

using TerrainTopology = topology.TerrainTopology;

using speedups = shapely.speedups;

using System.Collections.Generic;

public static class @__init__ {
    
    static @__init__() {
        speedups.enable();
    }
    
    // Enable Shapely "speedups" if available
    // http://toblerity.org/shapely/manual.html#performance
    // 
    //     Function to convert geometries into a
    //     :class:`quantized_mesh_tile.terrain.TerrainTile` instance.
    // 
    //     Arguments:
    // 
    //     ``geometries``
    // 
    //         A list of shapely polygon geometries representing 3 dimensional triangles.
    //         or
    //         A list of WKT or WKB Polygons representing 3 dimensional triangles.
    //         or
    //         A list of triplet of vertices using the following structure:
    //         ``(((lon0/lat0/height0),(...),(lon2,lat2,height2)),(...))``
    // 
    //     ``bounds``
    // 
    //         The bounds of the terrain tile. (west, south, east, north)
    //         If not defined, the bounds will be computed from the provided geometries.
    // 
    //         Default is `[]`.
    // 
    //     ``autocorrectGeometries``
    // 
    //         When set to `True`, it will attempt to fix geometries that are not
    //         triangles. This often happens when geometries are clipped from an existing mesh.
    // 
    //         Default is `False`.
    // 
    //     ``hasLighting`` (Experimental)
    // 
    //         Indicate whether unit vectors should be computed for the lighting extension.
    // 
    //         Default is `False`.
    // 
    //     ``watermask``
    // 
    //         A water mask list (Optional). Adds rendering water effect.
    //         The water mask list is either one byte, `[0]` for land and `[255]` for
    //         water, either a list of 256*256 values ranging from 0 to 255.
    //         Values in the mask are defined from north-to-south and west-to-east.
    //         Per default no watermask is applied. Note that the water mask effect depends on
    //         the texture of the raster layer drapped over your terrain.
    // 
    //         Default is `[]`.
    // 
    //     
    public static object encode(
        object geometries,
        object bounds = new List<object>(),
        object autocorrectGeometries = false,
        object hasLighting = false,
        object watermask = new List<object>()) {
        object tile;
        var topology = TerrainTopology(geometries: geometries, autocorrectGeometries: autocorrectGeometries, hasLighting: hasLighting);
        if (bounds.Count == 4) {
            var _tup_1 = bounds;
            var west = _tup_1.Item1;
            var south = _tup_1.Item2;
            var east = _tup_1.Item3;
            var north = _tup_1.Item4;
            tile = TerrainTile(topology: topology, watermask: watermask, west: west, south: south, east: east, north: north);
        } else {
            tile = TerrainTile(topology: topology, watermask: watermask);
        }
        return tile;
    }
    
    // 
    //     Function to convert a quantized-mesh terrain tile file into a
    //     :class:`quantized_mesh_tile.terrain.TerrainTile` instance.
    // 
    //     Arguments:
    // 
    //     ``filePath``
    // 
    //         An absolute or relative path to write the terrain tile. (Required)
    // 
    //     ``bounds``
    // 
    //         The bounds of the terrain tile. (west, south, east, north) (Required).
    // 
    //     ``hasLighting`` (Experimental)
    // 
    //         Indicate whether the tile has the lighting extension.
    // 
    //         Default is `False`.
    // 
    //     ``hasWatermask``
    // 
    //         Indicate whether the tile has the water-mask extension.
    // 
    //         Default is `False`.
    // 
    //     
    public static object decode(
        object filePath,
        object bounds,
        object hasLighting = false,
        object hasWatermask = false,
        object gzipped = false) {
        var _tup_1 = bounds;
        var west = _tup_1.Item1;
        var south = _tup_1.Item2;
        var east = _tup_1.Item3;
        var north = _tup_1.Item4;
        var tile = TerrainTile(west: west, south: south, east: east, north: north);
        tile.fromFile(filePath, hasLighting: hasLighting, hasWatermask: hasWatermask, gzipped: gzipped);
        return tile;
    }
}
