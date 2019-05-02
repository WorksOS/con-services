//  This module defines the :class:`quantized_mesh_tile.topology.TerrainTopology`.
// 
// Reference
// ---------
// 

using @division = @@__future__.division;

using object = builtins.object;

using old_div = past.utils.old_div;

using math;

using np = numpy;

using LLH2ECEF = llh_ecef.LLH2ECEF;

using computeNormals = utils.computeNormals;

using collapseIntoTriangles = utils.collapseIntoTriangles;

using BaseGeometry = shapely.geometry.@base.BaseGeometry;

using Polygon = shapely.geometry.polygon.Polygon;

using load_wkb = shapely.wkb.loads;

using load_wkt = shapely.wkt.loads;

using System.Collections.Generic;

using System;

using System.Linq;

public static class topology {
    
    // 
    //     This class is used to build the terrain tile topology.
    // 
    //     Contructor arguments:
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
    //         Default is `[]`.
    // 
    //     ``autocorrectGeometries``
    // 
    //         When set to `True`, it will attempt to fix geometries that are not
    //         triangles. This often happens when geometries are clipped from an existing mesh.
    // 
    //         Default is `False`.
    // 
    //     ``hasLighting``
    // 
    //         Indicate whether unit vectors should be computed for the lighting extension.
    // 
    //         Default is `False`.
    // 
    //     Usage example::
    // 
    //         from quantized_mesh_tile.topology import TerrainTopology
    //         triangles = [
    //             [[7.3828125, 44.6484375, 303.3],
    //              [7.3828125, 45.0, 320.2],
    //              [7.5585937, 44.82421875, 310.2]],
    //             [[7.3828125, 44.6484375, 303.3],
    //              [7.734375, 44.6484375, 350.3],
    //              [7.734375, 44.6484375, 350.3]],
    //             [[7.734375, 44.6484375, 350.3],
    //              [7.734375, 45.0, 330.3],
    //              [7.5585937, 44.82421875, 310.2]],
    //             [[7.734375, 45.0, 330.3],
    //              [7.5585937, 44.82421875, 310.2],
    //              [7.3828125, 45.0, 320.2]]
    //         ]
    //         topology = TerrainTopology(geometries=triangles)
    //         print topology
    // 
    //     
    public class TerrainTopology
        : object {
        
        public object autocorrectGeometries;
        
        public object cartesianVertices;
        
        public object faces;
        
        public object geometries;
        
        public object hasLighting;
        
        public object vertices;
        
        public Dictionary<object, object> verticesLookup;
        
        public object verticesUnitVectors;
        
        public TerrainTopology(object geometries = new List<object>(), object autocorrectGeometries = false, object hasLighting = false) {
            this.geometries = geometries;
            this.autocorrectGeometries = autocorrectGeometries;
            this.hasLighting = hasLighting;
            this.vertices = new List<object>();
            this.cartesianVertices = new List<object>();
            this.faces = new List<object>();
            this.verticesLookup = new Dictionary<object, object> {
            };
            if (this.geometries) {
                this.addGeometries(this.geometries);
            }
        }
        
        public virtual object @__repr__() {
            var msg = "Min height:";
            msg += String.Format("\n%s", this.minHeight);
            msg += "\nMax height:";
            msg += String.Format("\n%s", this.maxHeight);
            msg += "\nuVertex length:";
            msg += String.Format("\n%s", this.uVertex.Count);
            msg += "\nuVertex list:";
            msg += String.Format("\n%s", this.uVertex);
            msg += "\nvVertex length:";
            msg += String.Format("\n%s", this.vVertex.Count);
            msg += "\nuVertex list:";
            msg += String.Format("\n%s", this.vVertex);
            msg += "\nhVertex length:";
            msg += String.Format("\n%s", this.hVertex.Count);
            msg += "\nhVertex list:";
            msg += String.Format("\n%s", this.hVertex);
            msg += "\nindexData length:";
            msg += String.Format("\n%s", this.indexData.Count);
            msg += "\nindexData list:";
            msg += String.Format("\n%s", this.indexData);
            msg += String.Format("\nNumber of triangles: %s", old_div(this.indexData.Count, 3));
            return msg;
        }
        
        // 
        //         Method to add geometries to the terrain tile topology.
        // 
        //         Arguments:
        // 
        //         ``geometries``
        // 
        //             A list of shapely polygon geometries representing 3 dimensional triangles.
        //             or
        //             A list of WKT or WKB Polygons representing 3 dimensional triangles.
        //             or
        //             A list of triplet of vertices using the following structure:
        //             ``(((lon0/lat0/height0),(...),(lon2,lat2,height2)),(...))``
        //         
        public virtual object addGeometries(object geometries) {
            if ((geometries is list || geometries is tuple) && geometries) {
                foreach (var geometry in geometries) {
                    if (geometry is str || geometry is bytes) {
                        var geometry = this._loadGeometry(geometry);
                        var vertices = this._extractVertices(geometry);
                    } else if (geometry is BaseGeometry) {
                        vertices = this._extractVertices(geometry);
                    } else {
                        vertices = geometry;
                    }
                    if (this.autocorrectGeometries) {
                        if (vertices.Count > 3) {
                            var triangles = collapseIntoTriangles(vertices);
                            foreach (var triangle in triangles) {
                                this._addVertices(triangle);
                            }
                        } else {
                            this._addVertices(vertices);
                        }
                    } else {
                        this._addVertices(vertices);
                    }
                }
                this._create();
            }
        }
        
        // 
        //         Method to extract the triangle vertices from a Shapely geometry.
        //         ``((lon0/lat0/height0),(...),(lon2,lat2,height2))``
        // 
        //         You should normally never use this method directly.
        //         
        public virtual object _extractVertices(object geometry) {
            if (!geometry.has_z) {
                throw new ValueError("Missing z dimension.");
            }
            if (!(geometry is Polygon)) {
                throw new ValueError("Only polygons are accepted.");
            }
            var vertices = geometry.exterior.coords.ToList();
            if (vertices.Count != 4 && !this.autocorrectGeometries) {
                throw new ValueError("None triangular shape has beeen found.");
            }
            return vertices[::(len(vertices)  -  1)];
        }
        
        // 
        //         A private method to convert a (E)WKB or (E)WKT to a Shapely geometry.
        //         
        public virtual object _loadGeometry(object geometrySpec) {
            object geometry;
            if (object.ReferenceEquals(type(geometrySpec), str) && geometrySpec.startswith("POLYGON Z")) {
                try {
                    geometry = load_wkt(geometrySpec);
                } catch (Exception) {
                    geometry = null;
                }
            } else {
                try {
                    geometry = load_wkb(geometrySpec);
                } catch (Exception) {
                    geometry = null;
                }
            }
            if (geometry == null) {
                throw new ValueError("Failed to convert WKT or WKB to a Shapely geometry");
            }
            return geometry;
        }
        
        // 
        //         A private method to add vertices to the terrain tile topology.
        //         
        public virtual object _addVertices(object vertices) {
            vertices = this._assureCounterClockWise(vertices);
            var face = new List<object>();
            foreach (var vertex in vertices) {
                var lookupKey = ",".join(new List<string> {
                    "{:.14f}".format(vertex[0]),
                    "{:.14f}".format(vertex[1]),
                    "{:.14f}".format(vertex[2])
                });
                var faceIndex = this._lookupVertexIndex(lookupKey);
                if (faceIndex != null) {
                    // Sometimes we can have triangles with zero area
                    // (due to unfortunate clipping)
                    // In that case skip them
                    // if faceIndex in face:
                    //    break
                    face.append(faceIndex);
                } else {
                    this.vertices.append(vertex);
                    this.cartesianVertices.append(LLH2ECEF(vertex[0], vertex[1], vertex[2]));
                    faceIndex = this.vertices.Count - 1;
                    this.verticesLookup[lookupKey] = faceIndex;
                    face.append(faceIndex);
                }
            }
            // if len(face) == 3:
            this.faces.append(face);
        }
        
        // 
        //         A private method to create the final terrain data structure.
        //         
        public virtual object _create() {
            this.vertices = np.array(this.vertices, dtype: "float");
            this.cartesianVertices = np.array(this.cartesianVertices, dtype: "float");
            this.faces = np.array(this.faces, dtype: "int");
            if (this.hasLighting) {
                this.verticesUnitVectors = computeNormals(this.cartesianVertices, this.faces);
            }
            this.verticesLookup = new Dictionary<object, object> {
            };
        }
        
        // 
        //         A private method to determine if the vertex has already been discovered
        //         and return its index (or None if not found).
        //         
        public virtual object _lookupVertexIndex(object lookupKey) {
            if (this.verticesLookup.Contains(lookupKey)) {
                return this.verticesLookup[lookupKey];
            }
        }
        
        // 
        //         Private method to make sure vertices unwind in counterwise order.
        //         Inspired by:
        //         http://stackoverflow.com/questions/1709283/\
        //         how-can-i-sort-a-coordinate-list-for-a-rectangle-counterclockwise
        //         
        public virtual object _assureCounterClockWise(object vertices) {
            var mlat = old_div((from coord in vertices
                select coord[0]).Sum(), float(vertices.Count));
            var mlon = old_div((from coord in vertices
                select coord[1]).Sum(), float(vertices.Count));
            Func<object, object> algo = coord => {
                return (math.atan2(coord[0] - mlat, coord[1] - mlon) + 2 * math.pi) % (2 * math.pi);
            };
            vertices = vertices.OrderByDescending(algo).ToList();
            return vertices;
        }
        
        // 
        //         A class property returning the horizontal coordinates of the vertices
        //         in the tile. Normally never used directly.
        //         
        public object uVertex {
            get {
                if (this.vertices is np.ndarray) {
                    return this.vertices[":",0];
                }
            }
        }
        
        // 
        //         A class property returning the vertical coordinates of the vertices
        //         in the tile. Normally never used directly.
        //         
        public object vVertex {
            get {
                if (this.vertices is np.ndarray) {
                    return this.vertices[":",1];
                }
            }
        }
        
        // 
        //         A class property returning the height of the vertices in the tile.
        //         Normally never used directly.
        //         
        public object hVertex {
            get {
                if (this.vertices is np.ndarray) {
                    return this.vertices[":",2];
                }
            }
        }
        
        // 
        //         A class property returning the minimal longitude in the tile.
        //         Normally never used directly.
        //         
        public object minLon {
            get {
                if (this.vertices is np.ndarray) {
                    return np.min(this.vertices[":",0]);
                }
            }
        }
        
        // 
        //         A class property returning the minimal latitude in the tile.
        //         Normally never used directly.
        //         
        public object minLat {
            get {
                if (this.vertices is np.ndarray) {
                    return np.min(this.vertices[":",1]);
                }
            }
        }
        
        // 
        //         A class property returning the minimal height in the tile.
        //         Normally never used directly.
        //         
        public object minHeight {
            get {
                if (this.vertices is np.ndarray) {
                    return np.min(this.vertices[":",2]);
                }
            }
        }
        
        // 
        //         A class property returning the maximal longitude in the tile.
        //         Normally never used directly.
        //         
        public object maxLon {
            get {
                if (this.vertices is np.ndarray) {
                    return np.max(this.vertices[":",0]);
                }
            }
        }
        
        // 
        //         A class property returning the maximal latitude in the tile.
        //         Normally never used directly.
        //         
        public object maxLat {
            get {
                if (this.vertices is np.ndarray) {
                    return np.max(this.vertices[":",1]);
                }
            }
        }
        
        // 
        //         A class property returning the maximal height in the tile.
        //         Normally never used directly.
        //         
        public object maxHeight {
            get {
                if (this.vertices is np.ndarray) {
                    return np.max(this.vertices[":",2]);
                }
            }
        }
        
        // 
        //         A class property returning the minimal x value in ECEF
        //         coordinate system. Normally never used directly.
        //         
        public object ecefMinX {
            get {
                if (this.cartesianVertices is np.ndarray) {
                    return np.min(this.cartesianVertices[":",0]);
                }
            }
        }
        
        // 
        //         A class property returning the minimal y value in ECEF
        //         coordinate system. Normally never used directly.
        //         
        public object ecefMinY {
            get {
                if (this.cartesianVertices is np.ndarray) {
                    return np.min(this.cartesianVertices[":",1]);
                }
            }
        }
        
        // 
        //         A class property returning the minimal z value in ECEF
        //         coordinate system. Normally never used directly.
        //         
        public object ecefMinZ {
            get {
                if (this.cartesianVertices is np.ndarray) {
                    return np.min(this.cartesianVertices[":",2]);
                }
            }
        }
        
        // 
        //         A class property returning the maximal x value in ECEF
        //         coordinate system. Normally never used directly.
        //         
        public object ecefMaxX {
            get {
                if (this.cartesianVertices is np.ndarray) {
                    return np.max(this.cartesianVertices[":",0]);
                }
            }
        }
        
        // 
        //         A class property returning the maximal y value in ECEF
        //         coordinate system. Normally never used directly.
        //         
        public object ecefMaxY {
            get {
                if (this.cartesianVertices is np.ndarray) {
                    return np.max(this.cartesianVertices[":",1]);
                }
            }
        }
        
        // 
        //         A class property returning the maximal z value in ECEF
        //         coordinate system. Normally never used directly.
        //         
        public object ecefMaxZ {
            get {
                if (this.cartesianVertices is np.ndarray) {
                    return np.max(this.cartesianVertices[":",2]);
                }
            }
        }
        
        // 
        //         A class property retuning a list specifying how the vertices are linked together.
        //         These indices refer to the values in `uVertex`, `vVertex` and `hVertex` of
        //         this class. Normally never used directly.
        //         
        public object indexData {
            get {
                if (this.faces is np.ndarray) {
                    return this.faces.flatten();
                }
            }
        }
    }
}
