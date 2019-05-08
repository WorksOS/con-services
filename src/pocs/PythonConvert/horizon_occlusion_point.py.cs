
using @absolute_import = @@__future__.absolute_import;

using @division = @@__future__.division;

using map = builtins.map;

using old_div = past.utils.old_div;

using math;

using np = numpy;

using System.Collections.Generic;

using System.Linq;

public static class horizon_occlusion_point {
    
    public static object rX = old_div(1.0, ecef.radiusX);
    
    public static object rY = old_div(1.0, ecef.radiusY);
    
    public static object rZ = old_div(1.0, ecef.radiusZ);
    
    // Functions assumes ellipsoid scaled coordinates
    public static object computeMagnitude(object point, object sphereCenter) {
        var magnitudeSquared = c3d.magnitudeSquared(point);
        var magnitude = math.sqrt(magnitudeSquared);
        var direction = c3d.multiplyByScalar(point, old_div(1, magnitude));
        magnitudeSquared = max(1.0, magnitudeSquared);
        magnitude = max(1.0, magnitude);
        var cosAlpha = np.dot(direction, sphereCenter);
        var sinAlpha = c3d.magnitude(np.cross(direction, sphereCenter));
        var cosBeta = old_div(1.0, magnitude);
        var sinBeta = math.sqrt(magnitudeSquared - 1.0) * cosBeta;
        return old_div(1.0, cosAlpha * cosBeta - sinAlpha * sinBeta);
    }
    
    // https://cesiumjs.org/2013/05/09/Computing-the-horizon-occlusion-point/
    public static object fromPoints(object points, object boundingSphere) {
        if (points.Count < 1) {
            throw new Exception("Your list of points must contain at least 2 points");
        }
        // Bring coordinates to ellipsoid scaled coordinates
        Func<object, object> scaleDown = coord => {
            return new List<object> {
                coord[0] * rX,
                coord[1] * rY,
                coord[2] * rZ
            };
        };
        var scaledPoints = map(scaleDown, points).ToList();
        var scaledSphereCenter = scaleDown(boundingSphere.center);
        Func<object, object> magnitude = coord => {
            return computeMagnitude(coord, scaledSphereCenter);
        };
        var magnitudes = map(magnitude, scaledPoints).ToList();
        return c3d.multiplyByScalar(scaledSphereCenter, max(magnitudes));
    }
}
