//  This module defines the :class:`quantized_mesh_tile.terrain.TerrainTile`.
// More information about the format specification can be found here:
// https://github.com/AnalyticalGraphicsInc/quantized-mesh
// 
// Reference
// ---------
// 

using @absolute_import = @@__future__.absolute_import;

using @division = @@__future__.division;

using standard_library = future.standard_library;

using map = builtins.map;

using xrange = past.builtins.xrange;

using object = builtins.object;

using old_div = past.utils.old_div;

using os;

using gzip;

using io;

using OrderedDict = collections.OrderedDict;

using octEncode = utils.octEncode;

using octDecode = utils.octDecode;

using zigZagDecode = utils.zigZagDecode;

using zigZagEncode = utils.zigZagEncode;

using gzipFileObject = utils.gzipFileObject;

using ungzipFileObject = utils.ungzipFileObject;

using unpackEntry = utils.unpackEntry;

using decodeIndices = utils.decodeIndices;

using packEntry = utils.packEntry;

using packIndices = utils.packIndices;

using encodeIndices = utils.encodeIndices;

using BoundingSphere = bbsphere.BoundingSphere;

using TerrainTopology = topology.TerrainTopology;

using System.Collections.Generic;

using System.Collections;

using System;

using System.Linq;

public static class terrain {
    
    static terrain() {
        standard_library.install_aliases();
    }
    
    public static int TILEPXS = 65536;
    
    public static object lerp(object p, object q, object time) {
        return (1.0 - time) * p + time * q;
    }
    
    // 
    //     The main class to read and write a terrain tile.
    // 
    //     Constructor arguments:
    // 
    //     ``west``
    // 
    //         The longitude at the western edge of the tile. Default is `-1.0`.
    // 
    //     ``east``
    // 
    //         The longitude at the eastern edge of the tile. Default is `1.0`.
    // 
    //     ``south``
    // 
    //         The latitude at the southern edge of the tile. Default is `-1.0`.
    // 
    //     ``north``
    // 
    //         The latitude at the northern edge of the tile. Default is `1.0`.
    // 
    //     ``topology``
    // 
    //         The topology of the mesh which but be an instance of
    //         :class:`quantized_mesh_tile.topology.TerrainTopology`. Default is `None`.
    // 
    //     ``watermask``
    //         A water mask list (Optional). Adds rendering water effect.
    //         The water mask list is either one byte, `[0]` for land and `[255]` for
    //         water, either a list of 256*256 values ranging from 0 to 255.
    //         Values in the mask are defined from north-to-south and west-to-east.
    //         Per default no watermask is applied. Note that the water mask effect depends on
    //         the texture of the raster layer drapped over your terrain.
    //         Default is `[]`.
    // 
    //     Usage examples::
    // 
    //         from quantized_mesh_tile.terrain import TerrainTile
    //         from quantized_mesh_tile.topology import TerrainTopology
    //         from quantized_mesh_tile.global_geodetic import GlobalGeodetic
    // 
    //         # The tile coordinates
    //         x = 533
    //         y = 383
    //         z = 9
    //         geodetic = GlobalGeodetic(True)
    //         [west, south, east, north] = geodetic.TileBounds(x, y, z)
    // 
    //         # Read a terrain tile (unzipped)
    //         tile = TerrainTile(west=west, south=south, east=east, north=north)
    //         tile.fromFile('mytile.terrain')
    // 
    //         # Write a terrain tile locally from scratch (lon/lat/height)
    //         wkts = [
    //             'POLYGON Z ((7.3828125 44.6484375 303.3, ' +
    //                         '7.3828125 45.0 320.2, ' +
    //                         '7.5585937 44.82421875 310.2, ' +
    //                         '7.3828125 44.6484375 303.3))',
    //             'POLYGON Z ((7.3828125 44.6484375 303.3, ' +
    //                         '7.734375 44.6484375 350.3, ' +
    //                         '7.5585937 44.82421875 310.2, ' +
    //                         '7.3828125 44.6484375 303.3))',
    //             'POLYGON Z ((7.734375 44.6484375 350.3, ' +
    //                         '7.734375 45.0 330.3, ' +
    //                         '7.5585937 44.82421875 310.2, ' +
    //                         '7.734375 44.6484375 350.3))',
    //             'POLYGON Z ((7.734375 45.0 330.3, ' +
    //                         '7.5585937 44.82421875 310.2, ' +
    //                         '7.3828125 45.0 320.2, ' +
    //                         '7.734375 45.0 330.3))'
    //         ]
    //         topology = TerrainTopology(geometries=wkts)
    //         tile = TerrainTile(topology=topology)
    //         tile.toFile('mytile.terrain')
    // 
    //     
    public class TerrainTile
        : object {
        
        public object _deltaHeight;
        
        public object _east;
        
        public List<object> _heights;
        
        public List<object> _lats;
        
        public List<object> _longs;
        
        public object _north;
        
        public object _south;
        
        public List<object> _triangles;
        
        public object _west;
        
        public object _workingUnitLatitude;
        
        public object _workingUnitLongitude;
        
        public int BYTESPLIT;
        
        public List<object> eastI;
        
        public object EdgeIndices16;
        
        public object EdgeIndices32;
        
        public int EPSG;
        
        public object ExtensionHeader;
        
        public list h;
        
        public object hasLighting;
        
        public bool hasWatermask;
        
        public object header;
        
        public object indexData16;
        
        public object indexData32;
        
        public object indices;
        
        public double MAX;
        
        public double MIN;
        
        public List<object> northI;
        
        public object OctEncodedVertexNormals;
        
        public object quantizedMeshHeader;
        
        public List<object> southI;
        
        public list u;
        
        public list v;
        
        public object vertexData;
        
        public object vLight;
        
        public object watermask;
        
        public object WaterMask;
        
        public List<object> westI;
        
        public object quantizedMeshHeader = OrderedDict(new List<List<string>> {
            new List<string> {
                "centerX",
                "d"
            },
            new List<string> {
                "centerY",
                "d"
            },
            new List<string> {
                "centerZ",
                "d"
            },
            new List<string> {
                "minimumHeight",
                "f"
            },
            new List<string> {
                "maximumHeight",
                "f"
            },
            new List<string> {
                "boundingSphereCenterX",
                "d"
            },
            new List<string> {
                "boundingSphereCenterY",
                "d"
            },
            new List<string> {
                "boundingSphereCenterZ",
                "d"
            },
            new List<string> {
                "boundingSphereRadius",
                "d"
            },
            new List<string> {
                "horizonOcclusionPointX",
                "d"
            },
            new List<string> {
                "horizonOcclusionPointY",
                "d"
            },
            new List<string> {
                "horizonOcclusionPointZ",
                "d"
            }
        });
        
        public object vertexData = OrderedDict(new List<List<string>> {
            new List<string> {
                "vertexCount",
                "I"
            },
            new List<string> {
                "uVertexCount",
                "H"
            },
            new List<string> {
                "vVertexCount",
                "H"
            },
            new List<string> {
                "heightVertexCount",
                "H"
            }
        });
        
        public object indexData16 = OrderedDict(new List<List<string>> {
            new List<string> {
                "triangleCount",
                "I"
            },
            new List<string> {
                "indices",
                "H"
            }
        });
        
        public object indexData32 = OrderedDict(new List<List<string>> {
            new List<string> {
                "triangleCount",
                "I"
            },
            new List<string> {
                "indices",
                "I"
            }
        });
        
        public object EdgeIndices16 = OrderedDict(new List<List<string>> {
            new List<string> {
                "westVertexCount",
                "I"
            },
            new List<string> {
                "westIndices",
                "H"
            },
            new List<string> {
                "southVertexCount",
                "I"
            },
            new List<string> {
                "southIndices",
                "H"
            },
            new List<string> {
                "eastVertexCount",
                "I"
            },
            new List<string> {
                "eastIndices",
                "H"
            },
            new List<string> {
                "northVertexCount",
                "I"
            },
            new List<string> {
                "northIndices",
                "H"
            }
        });
        
        public object EdgeIndices32 = OrderedDict(new List<List<string>> {
            new List<string> {
                "westVertexCount",
                "I"
            },
            new List<string> {
                "westIndices",
                "I"
            },
            new List<string> {
                "southVertexCount",
                "I"
            },
            new List<string> {
                "southIndices",
                "I"
            },
            new List<string> {
                "eastVertexCount",
                "I"
            },
            new List<string> {
                "eastIndices",
                "I"
            },
            new List<string> {
                "northVertexCount",
                "I"
            },
            new List<string> {
                "northIndices",
                "I"
            }
        });
        
        public object ExtensionHeader = OrderedDict(new List<List<string>> {
            new List<string> {
                "extensionId",
                "B"
            },
            new List<string> {
                "extensionLength",
                "I"
            }
        });
        
        public object OctEncodedVertexNormals = OrderedDict(new List<List<string>> {
            new List<string> {
                "xy",
                "B"
            }
        });
        
        public object WaterMask = OrderedDict(new List<List<string>> {
            new List<string> {
                "xy",
                "B"
            }
        });
        
        public int BYTESPLIT = 65636;
        
        public double MIN = 0.0;
        
        public double MAX = 32767.0;
        
        public TerrainTile(Hashtable kwargs, params object [] args) {
            this._west = kwargs.get("west", -1.0);
            this._east = kwargs.get("east", 1.0);
            this._south = kwargs.get("south", -1.0);
            this._north = kwargs.get("north", 1.0);
            this._longs = new List<object>();
            this._lats = new List<object>();
            this._heights = new List<object>();
            this._triangles = new List<object>();
            this._workingUnitLongitude = null;
            this._workingUnitLatitude = null;
            this._deltaHeight = null;
            this.EPSG = 4326;
            // Extensions
            this.vLight = new List<object>();
            this.watermask = kwargs.get("watermask", new List<object>());
            this.hasWatermask = kwargs.get("hasWatermask", @bool(this.watermask));
            this.header = OrderedDict();
            foreach (var k in TerrainTile.quantizedMeshHeader.keys()) {
                this.header[k] = 0.0;
            }
            this.u = new List<object>();
            this.v = new List<object>();
            this.h = new List<object>();
            this.indices = new List<object>();
            this.westI = new List<object>();
            this.southI = new List<object>();
            this.eastI = new List<object>();
            this.northI = new List<object>();
            var topology = kwargs.get("topology");
            if (topology != null) {
                this.fromTerrainTopology(topology);
            }
        }
        
        public virtual object @__repr__() {
            var msg = String.Format("Header: %s\n", this.header);
            // Output intermediate structure
            msg += String.Format("\nVertexCount: %s", this.u.Count);
            msg += String.Format("\nuVertex: %s", this.u);
            msg += String.Format("\nvVertex: %s", this.v);
            msg += String.Format("\nhVertex: %s", this.h);
            msg += String.Format("\nindexDataCount: %s", this.indices.Count);
            msg += String.Format("\nindexData: %s", this.indices);
            msg += String.Format("\nwestIndicesCount: %s", this.westI.Count);
            msg += String.Format("\nwestIndices: %s", this.westI);
            msg += String.Format("\nsouthIndicesCount: %s", this.southI.Count);
            msg += String.Format("\nsouthIndices: %s", this.southI);
            msg += String.Format("\neastIndicesCount: %s", this.eastI.Count);
            msg += String.Format("\neastIndices: %s", this.eastI);
            msg += String.Format("\nnorthIndicesCount: %s", this.northI.Count);
            msg += String.Format("\nnorthIndices: %s\n", this.northI);
            // Output coordinates
            msg += String.Format("\nNumber of triangles: %s", old_div(this.indices.Count, 3));
            msg += String.Format("\nTriangles coordinates in EPSG %s", this.EPSG);
            msg += String.Format("\n%s", this.getTrianglesCoordinates());
            return msg;
        }
        
        // 
        //         A method to determine the content type of a tile.
        //         
        public virtual object getContentType() {
            var baseContent = "application/vnd.quantized-mesh";
            if (this.hasLighting && this.hasWatermask) {
                return baseContent + ";extensions=octvertexnormals-watermask";
            } else if (this.hasLighting) {
                return baseContent + ";extensions=octvertexnormals";
            } else if (this.hasWatermask) {
                return baseContent + ";extensions=watermask";
            } else {
                return baseContent;
            }
        }
        
        // 
        //         A method to retrieve the coordinates of the vertices in lon,lat,height.
        //         
        public virtual object getVerticesCoordinates() {
            this._computeVerticesCoordinates();
            var coordinates = new List<object>();
            foreach (var _tup_1 in this._longs.Select((_p_1,_p_2) => Tuple.Create(_p_2, _p_1))) {
                var i = _tup_1.Item1;
                var lon = _tup_1.Item2;
                coordinates.append(Tuple.Create(lon, this._lats[i], this._heights[i]));
            }
            return coordinates;
        }
        
        // 
        //         A method to retrieve triplet of coordinates representing the triangles
        //         in lon,lat,height.
        //         
        public virtual object getTrianglesCoordinates() {
            this._computeVerticesCoordinates();
            var triangles = new List<object>();
            var nbTriangles = this.indices.Count;
            if (nbTriangles % 3 != 0) {
                throw new Exception("Corrupted tile");
            }
            foreach (var i in xrange(0, nbTriangles - 1, 3)) {
                var vi1 = this.indices[i];
                var vi2 = this.indices[i + 1];
                var vi3 = this.indices[i + 2];
                var triangle = Tuple.Create(Tuple.Create(this._longs[vi1], this._lats[vi1], this._heights[vi1]), Tuple.Create(this._longs[vi2], this._lats[vi2], this._heights[vi2]), Tuple.Create(this._longs[vi3], this._lats[vi3], this._heights[vi3]));
                triangles.append(triangle);
            }
            return triangles;
        }
        
        // 
        //         A private method to compute the vertices coordinates.
        //         
        public virtual object _computeVerticesCoordinates() {
            if (!this._longs) {
                foreach (var u in this.u) {
                    this._longs.append(lerp(this._west, this._east, old_div(float(u), this.MAX)));
                }
                foreach (var v in this.v) {
                    this._lats.append(lerp(this._south, this._north, old_div(float(v), this.MAX)));
                }
                foreach (var h in this.h) {
                    this._heights.append(lerp(this.header["minimumHeight"], this.header["maximumHeight"], old_div(float(h), this.MAX)));
                }
            }
        }
        
        // 
        //         A method to read a terrain tile content.
        // 
        //         Arguments:
        // 
        //         ``f``
        // 
        //             An instance of io.BytesIO containing the terrain data. (Required)
        // 
        //         ``hasLighting``
        // 
        //             Indicate if the tile contains lighting information. Default is ``False``.
        // 
        //         ``hasWatermask``
        // 
        //             Indicate if the tile contains watermask information. Default is ``False``.
        //         
        public virtual object fromBytesIO(object f, object hasLighting = false, object hasWatermask = false) {
            object extensionLength;
            object extensionId;
            this.hasLighting = hasLighting;
            this.hasWatermask = hasWatermask;
            // Header
            foreach (var _tup_1 in TerrainTile.quantizedMeshHeader.items()) {
                var k = _tup_1.Item1;
                var v = _tup_1.Item2;
                this.header[k] = unpackEntry(f, v);
            }
            // Vertices
            var vertexCount = unpackEntry(f, TerrainTile.vertexData["vertexCount"]);
            foreach (var ud in this._iterUnpackAndDecodeVertices(f, vertexCount, TerrainTile.vertexData["uVertexCount"])) {
                this.u.append(ud);
            }
            foreach (var vd in this._iterUnpackAndDecodeVertices(f, vertexCount, TerrainTile.vertexData["vVertexCount"])) {
                this.v.append(vd);
            }
            foreach (var hd in this._iterUnpackAndDecodeVertices(f, vertexCount, TerrainTile.vertexData["heightVertexCount"])) {
                this.h.append(hd);
            }
            // Indices
            var meta = TerrainTile.indexData16;
            if (vertexCount > TerrainTile.BYTESPLIT) {
                meta = TerrainTile.indexData32;
            }
            var triangleCount = unpackEntry(f, meta["triangleCount"]);
            var ind = (from index in this._iterUnpackIndices(f, triangleCount * 3, meta["indices"])
                select index).ToList();
            this.indices = decodeIndices(ind);
            meta = TerrainTile.EdgeIndices16;
            if (vertexCount > TerrainTile.BYTESPLIT) {
                meta = TerrainTile.indexData32;
            }
            // Edges (vertices on the edge of the tile)
            var westIndicesCount = unpackEntry(f, meta["westVertexCount"]);
            foreach (var wi in this._iterUnpackIndices(f, westIndicesCount, meta["westIndices"])) {
                this.westI.append(wi);
            }
            var southIndicesCount = unpackEntry(f, meta["southVertexCount"]);
            foreach (var si in this._iterUnpackIndices(f, southIndicesCount, meta["southIndices"])) {
                this.southI.append(si);
            }
            var eastIndicesCount = unpackEntry(f, meta["eastVertexCount"]);
            foreach (var ei in this._iterUnpackIndices(f, eastIndicesCount, meta["eastIndices"])) {
                this.eastI.append(ei);
            }
            var northIndicesCount = unpackEntry(f, meta["northVertexCount"]);
            foreach (var ni in this._iterUnpackIndices(f, northIndicesCount, meta["northIndices"])) {
                this.northI.append(ni);
            }
            if (this.hasLighting) {
                // One byte of padding
                // Light extension header
                meta = TerrainTile.ExtensionHeader;
                extensionId = unpackEntry(f, meta["extensionId"]);
                if (extensionId == 1) {
                    extensionLength = unpackEntry(f, meta["extensionLength"]);
                    foreach (var xy in this._iterUnpackAndDecodeLight(f, extensionLength, TerrainTile.OctEncodedVertexNormals["xy"])) {
                        this.vLight.append(xy);
                    }
                }
            }
            if (this.hasWatermask) {
                meta = TerrainTile.ExtensionHeader;
                extensionId = unpackEntry(f, meta["extensionId"]);
                if (extensionId == 2) {
                    extensionLength = unpackEntry(f, meta["extensionLength"]);
                    foreach (var row in this._iterUnpackWatermaskRow(f, extensionLength, TerrainTile.WaterMask["xy"])) {
                        this.watermask.append(row);
                    }
                }
            }
            var data = f.read(1);
            if (data) {
                throw new Exception("Should have reached end of file, but didn\'t");
            }
        }
        
        // 
        //         A private method to itertatively unpack and decode indices.
        //         
        [staticmethod]
        public static object _iterUnpackAndDecodeVertices(object f, object vertexCount, object structType) {
            var i = 0;
            // Delta decoding
            var delta = 0;
            while (i != vertexCount) {
                delta += zigZagDecode(unpackEntry(f, structType));
                yield return delta;
                i += 1;
            }
        }
        
        // 
        //         A private method to iteratively unpack indices
        //         
        [staticmethod]
        public static object _iterUnpackIndices(object f, object indicesCount, object structType) {
            var i = 0;
            while (i != indicesCount) {
                yield return unpackEntry(f, structType);
                i += 1;
            }
        }
        
        // 
        //         A private method to iteratively unpack light vector.
        //         
        [staticmethod]
        public static object _iterUnpackAndDecodeLight(object f, object extensionLength, object structType) {
            var i = 0;
            var xyCount = old_div(extensionLength, 2);
            while (i != xyCount) {
                yield return octDecode(unpackEntry(f, structType), unpackEntry(f, structType));
                i += 1;
            }
        }
        
        // 
        //         A private method to iteratively unpack watermask rows
        //         
        [staticmethod]
        public static object _iterUnpackWatermaskRow(object f, object extensionLength, object structType) {
            var i = 0;
            var xyCount = 0;
            var row = new List<object>();
            while (xyCount != extensionLength) {
                row.append(unpackEntry(f, structType));
                if (i == 255) {
                    yield return row;
                    i = 0;
                    row = new List<object>();
                } else {
                    i += 1;
                }
                xyCount += 1;
            }
            if (row) {
                yield return row;
            }
        }
        
        // 
        //         A method to read a terrain tile file. It is assumed that the tile unzipped.
        // 
        //         Arguments:
        // 
        //         ``filePath``
        // 
        //             An absolute or relative path to a quantized-mesh terrain tile. (Required)
        // 
        //         ``hasLighting``
        // 
        //             Indicate if the tile contains lighting information. Default is ``False``.
        // 
        //         ``hasWatermask``
        // 
        //             Indicate if the tile contains watermask information. Default is ``False``.
        // 
        //         ``gzipped``
        // 
        //             Indicate if the tile content is gzipped. Default is ``False``.
        //         
        public virtual object fromFile(object filePath, object hasLighting = false, object hasWatermask = false, object gzipped = false) {
            using (var f = open(filePath, "rb")) {
                if (gzipped) {
                    f = ungzipFileObject(f);
                }
                this.fromBytesIO(f, hasLighting: hasLighting, hasWatermask: hasWatermask);
            }
        }
        
        // 
        //         A method to write the terrain tile data to a file-like object (a string buffer).
        // 
        //         Arguments:
        // 
        //         ``gzipped``
        // 
        //             Indicate if the content should be gzipped. Default is ``False``.
        //         
        public virtual object toBytesIO(object gzipped = false) {
            var f = io.BytesIO();
            this._writeTo(f);
            if (gzipped) {
                f = gzipFileObject(f);
            }
            return f;
        }
        
        // 
        //         A method to write the terrain tile data to a physical file.
        // 
        //         Argument:
        // 
        //         ``filePath``
        // 
        //             An absolute or relative path to write the terrain tile. (Required)
        // 
        //         ``gzipped``
        // 
        //             Indicate if the content should be gzipped. Default is ``False``.
        //         
        public virtual object toFile(object filePath, object gzipped = false) {
            if (os.path.isfile(filePath)) {
                throw new IOError(String.Format("File %s already exists", filePath));
            }
            if (!gzipped) {
                using (var f = open(filePath, "wb")) {
                    this._writeTo(f);
                }
            } else {
                using (var f = gzip.open(filePath, "wb")) {
                    this._writeTo(f);
                }
            }
        }
        
        public virtual object _getWorkingUnitLatitude() {
            if (!this._workingUnitLatitude) {
                this._workingUnitLatitude = old_div(this.MAX, this._north - this._south);
            }
            return this._workingUnitLatitude;
        }
        
        public virtual object _getWorkingUnitLongitude() {
            if (!this._workingUnitLongitude) {
                this._workingUnitLongitude = old_div(this.MAX, this._east - this._west);
            }
            return this._workingUnitLongitude;
        }
        
        public virtual object _getDeltaHeight() {
            if (!this._deltaHeight) {
                var maxHeight = this.header["maximumHeight"];
                var minHeight = this.header["minimumHeight"];
                this._deltaHeight = maxHeight - minHeight;
            }
            return this._deltaHeight;
        }
        
        public virtual object _quantizeLatitude(object latitude) {
            return Convert.ToInt32(round((latitude - this._south) * this._getWorkingUnitLatitude()));
        }
        
        public virtual object _quantizeLongitude(object longitude) {
            return Convert.ToInt32(round((longitude - this._west) * this._getWorkingUnitLongitude()));
        }
        
        public virtual object _quantizeHeight(object height) {
            object h;
            var deniv = this._getDeltaHeight();
            // In case a tile is completely flat
            if (deniv == 0) {
                h = 0;
            } else {
                var workingUnitHeight = old_div(this.MAX, deniv);
                h = Convert.ToInt32(round((height - this.header["minimumHeight"]) * workingUnitHeight));
            }
            return h;
        }
        
        // 
        //         Private helper method to convert quantized tile (h) values to real world height
        //         values
        //         :param h: the quantized height value
        //         :return: the height in ground units (meter)
        //         
        public virtual object _dequantizeHeight(object h) {
            return lerp(this.header["minimumHeight"], this.header["maximumHeight"], old_div(float(h), this.MAX));
        }
        
        // 
        //         A private method to write the terrain tile to a file or file-like object.
        //         
        public virtual object _writeTo(object f) {
            object x;
            // Header
            foreach (var _tup_1 in TerrainTile.quantizedMeshHeader.items()) {
                var k = _tup_1.Item1;
                var v = _tup_1.Item2;
                f.write(packEntry(v, this.header[k]));
            }
            // Delta decoding
            var vertexCount = this.u.Count;
            // Vertices
            f.write(packEntry(TerrainTile.vertexData["vertexCount"], vertexCount));
            // Move the initial value
            f.write(packEntry(TerrainTile.vertexData["uVertexCount"], zigZagEncode(this.u[0])));
            foreach (var i in xrange(0, vertexCount - 1)) {
                var ud = this.u[i + 1] - this.u[i];
                f.write(packEntry(TerrainTile.vertexData["uVertexCount"], zigZagEncode(ud)));
            }
            f.write(packEntry(TerrainTile.vertexData["uVertexCount"], zigZagEncode(this.v[0])));
            foreach (var i in xrange(0, vertexCount - 1)) {
                var vd = this.v[i + 1] - this.v[i];
                f.write(packEntry(TerrainTile.vertexData["vVertexCount"], zigZagEncode(vd)));
            }
            f.write(packEntry(TerrainTile.vertexData["uVertexCount"], zigZagEncode(this.h[0])));
            foreach (var i in xrange(0, vertexCount - 1)) {
                var hd = this.h[i + 1] - this.h[i];
                f.write(packEntry(TerrainTile.vertexData["heightVertexCount"], zigZagEncode(hd)));
            }
            // Indices
            var meta = TerrainTile.indexData16;
            if (vertexCount > TerrainTile.BYTESPLIT) {
                meta = TerrainTile.indexData32;
            }
            f.write(packEntry(meta["triangleCount"], old_div(this.indices.Count, 3)));
            var ind = encodeIndices(this.indices);
            packIndices(f, meta["indices"], ind);
            meta = TerrainTile.EdgeIndices16;
            if (vertexCount > TerrainTile.BYTESPLIT) {
                meta = TerrainTile.EdgeIndices32;
            }
            f.write(packEntry(meta["westVertexCount"], this.westI.Count));
            foreach (var wi in this.westI) {
                f.write(packEntry(meta["westIndices"], wi));
            }
            f.write(packEntry(meta["southVertexCount"], this.southI.Count));
            foreach (var si in this.southI) {
                f.write(packEntry(meta["southIndices"], si));
            }
            f.write(packEntry(meta["eastVertexCount"], this.eastI.Count));
            foreach (var ei in this.eastI) {
                f.write(packEntry(meta["eastIndices"], ei));
            }
            f.write(packEntry(meta["northVertexCount"], this.northI.Count));
            foreach (var ni in this.northI) {
                f.write(packEntry(meta["northIndices"], ni));
            }
            // Extension header for light
            if (this.vLight) {
                this.hasLighting = true;
                meta = TerrainTile.ExtensionHeader;
                // Extension header ID is 1 for lightening
                f.write(packEntry(meta["extensionId"], 1));
                // Unsigned char size len is 1
                f.write(packEntry(meta["extensionLength"], 2 * vertexCount));
                var metaV = TerrainTile.OctEncodedVertexNormals;
                foreach (var i in xrange(0, vertexCount)) {
                    var _tup_2 = octEncode(this.vLight[i]);
                    x = _tup_2.Item1;
                    var y = _tup_2.Item2;
                    f.write(packEntry(metaV["xy"], x));
                    f.write(packEntry(metaV["xy"], y));
                }
            }
            if (this.watermask) {
                this.hasWatermask = true;
                // Extension header ID is 2 for watermark
                meta = TerrainTile.ExtensionHeader;
                f.write(packEntry(meta["extensionId"], 2));
                // Extension header meta
                var nbRows = this.watermask.Count;
                if (nbRows > 1) {
                    // Unsigned char size len is 1
                    f.write(packEntry(meta["extensionLength"], TILEPXS));
                    if (nbRows != 256) {
                        throw new Exception(String.Format("Unexpected number of rows for the watermask: %s", nbRows));
                    }
                    // From North to South
                    foreach (var i in xrange(0, nbRows)) {
                        x = this.watermask[i];
                        if (x.Count != 256) {
                            throw new Exception(String.Format("Unexpected number of columns for the watermask: %s", x.Count));
                        }
                        // From West to East
                        foreach (var y in x) {
                            f.write(packEntry(TerrainTile.WaterMask["xy"], Convert.ToInt32(y)));
                        }
                    }
                } else {
                    f.write(packEntry(meta["extensionLength"], 1));
                    if (this.watermask[0][0] == null) {
                        this.watermask[0][0] = 0;
                    }
                    f.write(packEntry(TerrainTile.WaterMask["xy"], Convert.ToInt32(this.watermask[0][0])));
                }
            }
        }
        
        // 
        //         A method to prepare a terrain tile data structure.
        // 
        //         Arguments:
        // 
        //         ``topology``
        // 
        //             The topology of the mesh which must be an instance of
        //             :class:`quantized_mesh_tile.topology.TerrainTopology`. (Required)
        // 
        //         ``bounds``
        // 
        //             The bounds of a the terrain tile. (west, south, east, north)
        //             If not defined, the bounds defined during initialization will be used.
        //             If no bounds are provided, then the bounds
        //             are extracted from the topology object.
        // 
        //         
        public virtual object fromTerrainTopology(object topology, object bounds = null) {
            if (!(topology is TerrainTopology)) {
                throw new Exception("topology object must be an instance of TerrainTopology");
            }
            // If the bounds are not provided use
            // topology extent instead
            if (bounds != null) {
                this._west = bounds[0];
                this._east = bounds[2];
                this._south = bounds[1];
                this._north = bounds[3];
            } else if (new HashSet<object>(new List<object> {
                this._west,
                this._south,
                this._east,
                this._north
            }).difference(new HashSet<object>(new List<double> {
                -1.0,
                -1.0,
                1.0,
                1.0
            }))) {
                // Bounds already defined earlier
            } else {
                // Set tile bounds
                this._west = topology.minLon;
                this._east = topology.maxLon;
                this._south = topology.minLat;
                this._north = topology.maxLat;
            }
            var bSphere = BoundingSphere();
            bSphere.fromPoints(topology.cartesianVertices);
            var ecefMinX = topology.ecefMinX;
            var ecefMinY = topology.ecefMinY;
            var ecefMinZ = topology.ecefMinZ;
            var ecefMaxX = topology.ecefMaxX;
            var ecefMaxY = topology.ecefMaxY;
            var ecefMaxZ = topology.ecefMaxZ;
            // Center of the bounding box 3d
            var centerCoords = new List<double> {
                ecefMinX + (ecefMaxX - ecefMinX) * 0.5,
                ecefMinY + (ecefMaxY - ecefMinY) * 0.5,
                ecefMinZ + (ecefMaxZ - ecefMinZ) * 0.5
            };
            var occlusionPCoords = occ.fromPoints(topology.cartesianVertices, bSphere);
            foreach (var k in TerrainTile.quantizedMeshHeader.keys()) {
                if (k == "centerX") {
                    this.header[k] = centerCoords[0];
                } else if (k == "centerY") {
                    this.header[k] = centerCoords[1];
                } else if (k == "centerZ") {
                    this.header[k] = centerCoords[2];
                } else if (k == "minimumHeight") {
                    this.header[k] = topology.minHeight;
                } else if (k == "maximumHeight") {
                    this.header[k] = topology.maxHeight;
                } else if (k == "boundingSphereCenterX") {
                    this.header[k] = bSphere.center[0];
                } else if (k == "boundingSphereCenterY") {
                    this.header[k] = bSphere.center[1];
                } else if (k == "boundingSphereCenterZ") {
                    this.header[k] = bSphere.center[2];
                } else if (k == "boundingSphereRadius") {
                    this.header[k] = bSphere.radius;
                } else if (k == "horizonOcclusionPointX") {
                    this.header[k] = occlusionPCoords[0];
                } else if (k == "horizonOcclusionPointY") {
                    this.header[k] = occlusionPCoords[1];
                } else if (k == "horizonOcclusionPointZ") {
                    this.header[k] = occlusionPCoords[2];
                }
            }
            // High watermark encoding performed during toFile
            this.u = map(this._quantizeLongitude, topology.uVertex).ToList();
            this.v = map(this._quantizeLatitude, topology.vVertex).ToList();
            this.h = map(this._quantizeHeight, topology.hVertex).ToList();
            this.indices = topology.indexData;
            // List all the vertices on the edge of the tile
            // Use quantized values to determine if an indice belong to a tile edge
            foreach (var indice in this.indices) {
                var x = this.u[indice];
                var y = this.v[indice];
                if (x == this.MIN && !this.westI.Contains(indice)) {
                    this.westI.append(indice);
                } else if (x == this.MAX && !this.eastI.Contains(indice)) {
                    this.eastI.append(indice);
                }
                if (y == this.MIN && !this.southI.Contains(indice)) {
                    this.southI.append(indice);
                } else if (y == this.MAX && !this.northI.Contains(indice)) {
                    this.northI.append(indice);
                }
            }
            this.hasLighting = topology.hasLighting;
            if (this.hasLighting) {
                this.vLight = topology.verticesUnitVectors;
            }
        }
    }
}
