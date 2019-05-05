
using @absolute_import = @@__future__.absolute_import;

using @division = @@__future__.division;

using standard_library = future.standard_library;

using xrange = past.builtins.xrange;

using old_div = past.utils.old_div;

using math;

using gzip;

using io;

using np = numpy;

using pack = @struct.pack;

using unpack = @struct.unpack;

using calcsize = @struct.calcsize;

using System;

using System.Collections.Generic;

using System.Linq;

public static class utils {
    
    static utils() {
        standard_library.install_aliases();
    }
    
    public static double EPSILON6 = 1E-06;
    
    public static object packEntry(object type, object value) {
        return pack(String.Format("<%s", type), value);
    }
    
    public static object unpackEntry(object f, object entry) {
        return unpack(String.Format("<%s", entry), f.read(calcsize(entry)))[0];
    }
    
    public static object packIndices(object f, object type, object indices) {
        foreach (var i in indices) {
            f.write(packEntry(type, i));
        }
    }
    
    public static object decodeIndices(object indices) {
        var @out = new List<object>();
        var highest = 0;
        foreach (var i in indices) {
            @out.append(highest - i);
            if (i == 0) {
                highest += 1;
            }
        }
        return @out;
    }
    
    public static object encodeIndices(object indices) {
        var @out = new List<object>();
        var highest = 0;
        foreach (var i in indices) {
            var code = highest - i;
            @out.append(code);
            if (code == 0) {
                highest += 1;
            }
        }
        return @out;
    }
    
    // 
    //     ZigZag-Encodes a number:
    //        -1 = 1
    //        -2 = 3
    //         0 = 0
    //         1 = 2
    //         2 = 4
    //     
    public static object zigZagEncode(object n) {
        return n << 1 ^ n >> 31;
    }
    
    //  Reverses ZigZag encoding 
    public static object zigZagDecode(object z) {
        return z >> 1 ^ -(z & 1);
    }
    
    public static object clamp(object val, object minVal, object maxVal) {
        return max(min(val, maxVal), minVal);
    }
    
    public static object signNotZero(object v) {
        return v < 0.0 ? -1.0 : 1.0;
    }
    
    // Converts a scalar value in the range [-1.0, 1.0] to a 8-bit 2's complement number.
    public static object toSnorm(object v) {
        return round((clamp(v, -1.0, 1.0) * 0.5 + 0.5) * 255.0);
    }
    
    public static object fromSnorm(object v) {
        return clamp(v, 0.0, 255.0) / 255.0 * 2.0 - 1.0;
    }
    
    // Compress x, y, z 96-bit floating point into x, z 16-bit representation (2 snorm values)
    // https://github.com/AnalyticalGraphicsInc/cesium/blob/b161b6429b9201c99e5fb6f6e6283f3e8328b323/Source/Core/AttributeCompression.js#L43
    public static object octEncode(object vec) {
        if (abs(c3d.magnitudeSquared(vec) - 1.0) > EPSILON6) {
            throw new ValueError("Only normalized vectors are supported");
        }
        var res = new List<double> {
            0.0,
            0.0
        };
        var l1Norm = float(abs(vec[0]) + abs(vec[1]) + abs(vec[2]));
        res[0] = old_div(vec[0], l1Norm);
        res[1] = old_div(vec[1], l1Norm);
        if (vec[2] < 0.0) {
            var x = res[0];
            var y = res[1];
            res[0] = (1.0 - abs(y)) * signNotZero(x);
            res[1] = (1.0 - abs(x)) * signNotZero(y);
        }
        res[0] = Convert.ToInt32(toSnorm(res[0]));
        res[1] = Convert.ToInt32(toSnorm(res[1]));
        return res;
    }
    
    public static object octDecode(object x, object y) {
        if (x < 0 || x > 255 || y < 0 || y > 255) {
            throw new ValueError("x and y must be signed and normalized between 0 and 255");
        }
        var res = new List<double> {
            x,
            y,
            0.0
        };
        res[0] = fromSnorm(x);
        res[1] = fromSnorm(y);
        res[2] = 1.0 - (abs(res[0]) + abs(res[1]));
        if (res[2] < 0.0) {
            var oldX = res[0];
            res[0] = (1.0 - abs(res[1])) * signNotZero(oldX);
            res[1] = (1.0 - abs(oldX)) * signNotZero(res[1]);
        }
        return c3d.normalize(res);
    }
    
    public static object centroid(object a, object b, object c) {
        return new List<object> {
            old_div(Tuple.Create(a[0], b[0], c[0]).Sum(), 3),
            old_div(Tuple.Create(a[1], b[1], c[1]).Sum(), 3),
            old_div(new List<object> {
                a[2],
                b[2],
                c[2]
            }.Sum(), 3)
        };
    }
    
    // Based on the vectors defining the plan
    public static object triangleArea(object a, object b) {
        var i = math.pow(a[1] * b[2] - a[2] * b[1], 2);
        var j = math.pow(a[2] * b[0] - a[0] * b[2], 2);
        var k = math.pow(a[0] * b[1] - a[1] * b[0], 2);
        return 0.5 * math.sqrt(i + j + k);
    }
    
    // Inspired by
    // https://github.com/AnalyticalGraphicsInc/cesium/wiki/Geometry-and-Appearances
    // https://github.com/AnalyticalGraphicsInc/cesium/blob/master/
    //     Source/Core/GeometryPipeline.js#L1071
    public static object computeNormals(object vertices, object faces) {
        object face;
        var numVertices = vertices.Count;
        var numFaces = faces.Count;
        var normalsPerFace = new List<None> {
            null
        } * numFaces;
        var areasPerFace = new List<double> {
            0.0
        } * numFaces;
        var normalsPerVertex = np.zeros(vertices.shape, dtype: vertices.dtype);
        foreach (var i in xrange(0, numFaces)) {
            face = faces[i];
            var v0 = vertices[face[0]];
            var v1 = vertices[face[1]];
            var v2 = vertices[face[2]];
            var normal = np.cross(c3d.subtract(v1, v0), c3d.subtract(v2, v0));
            var area = triangleArea(v0, v1);
            areasPerFace[i] = area;
            normalsPerFace[i] = normal;
        }
        foreach (var i in xrange(0, numFaces)) {
            face = faces[i];
            var weightedNormal = (from c in normalsPerFace[i]
                select (c * areasPerFace[i])).ToList();
            foreach (var j in face) {
                normalsPerVertex[j] = c3d.add(normalsPerVertex[j], weightedNormal);
            }
        }
        foreach (var i in xrange(0, numVertices)) {
            normalsPerVertex[i] = c3d.normalize(normalsPerVertex[i]);
        }
        return normalsPerVertex;
    }
    
    public static object gzipFileObject(object data) {
        var compressed = io.BytesIO();
        var gz = gzip.GzipFile(fileobj: compressed, mode: "wb", compresslevel: 5);
        gz.write(data.getvalue());
        gz.close();
        compressed.seek(0);
        return compressed;
    }
    
    public static object ungzipFileObject(object data) {
        var buff = io.BytesIO(data.read());
        var f = gzip.GzipFile(fileobj: buff);
        return f;
    }
    
    public static object getCoordsIndex(object n, object i) {
        return n - 1 != i ? i + 1 : 0;
    }
    
    // Creates all the potential pairs of coords
    public static object createCoordsPairs(object l) {
        var coordsPairs = new List<object>();
        foreach (var i in xrange(0, l.Count)) {
            coordsPairs.append(new List<object> {
                l[i],
                l[(i + 2) % l.Count]
            });
        }
        return coordsPairs;
    }
    
    public static object squaredDistances(object coordsPairs) {
        var sDistances = new List<object>();
        foreach (var coordsPair in coordsPairs) {
            sDistances.append(c3d.distanceSquared(coordsPair[0], coordsPair[1]));
        }
        return sDistances;
    }
    
    public static object collapseIntoTriangles(object coords) {
        var triangles = new List<object>();
        while (coords.Count > 3) {
            // Create all possible pairs of coordinates
            var coordsPairs = createCoordsPairs(coords);
            var sDistances = squaredDistances(coordsPairs);
            var index = sDistances.index(min(sDistances));
            var i = getCoordsIndex(coords.Count, index);
            var triangle = coordsPairs[index] + new List<object> {
                coords[i]
            };
            triangles.append(triangle);
            // Remove the converging point
            // As this point is not available to create a new triangle anymore
            var convergingPoint = coords.index(coords[i]);
            coords.pop(convergingPoint);
        }
        return triangles + new List<object> {
            coords
        };
    }
}
