
using @absolute_import = @@__future__.absolute_import;

using @division = @@__future__.division;

using math;

using map = builtins.map;

using object = builtins.object;

using xrange = past.builtins.xrange;

using old_div = past.utils.old_div;

using System.Collections;

using System.Collections.Generic;

using System.Linq;

public static class bbsphere {
    
    public class BoundingSphere
        : object {
        
        public object center;
        
        public object maxPointX;
        
        public object maxPointY;
        
        public object maxPointZ;
        
        public object minPointX;
        
        public object minPointY;
        
        public object minPointZ;
        
        public double radius;
        
        public BoundingSphere(Hashtable kwargs, params object [] args) {
            var MAX = float("infinity");
            var MIN = float("-infinity");
            this.center = map(float, kwargs.get("center", new List<object>())).ToList();
            this.radius = float(kwargs.get("radius", 0));
            this.minPointX = new List<double> {
                MAX,
                MAX,
                MAX
            };
            this.minPointY = new List<double> {
                MAX,
                MAX,
                MAX
            };
            this.minPointZ = new List<double> {
                MAX,
                MAX,
                MAX
            };
            this.maxPointX = new List<double> {
                MIN,
                MIN,
                MIN
            };
            this.maxPointY = new List<double> {
                MIN,
                MIN,
                MIN
            };
            this.maxPointZ = new List<double> {
                MIN,
                MIN,
                MIN
            };
        }
        
        // Based on Ritter's algorithm
        public virtual object fromPoints(object points) {
            var nbPositions = points.Count;
            if (nbPositions < 2) {
                throw new Exception("Your list of points must contain at least 2 points");
            }
            foreach (var i in xrange(0, nbPositions)) {
                var point = points[i];
                // Store the points containing the smallest and largest component
                // Used for the naive approach
                if (point[0] < this.minPointX[0]) {
                    this.minPointX = point;
                }
                if (point[1] < this.minPointY[1]) {
                    this.minPointY = point;
                }
                if (point[2] < this.minPointZ[2]) {
                    this.minPointZ = point;
                }
                if (point[0] > this.maxPointX[0]) {
                    this.maxPointX = point;
                }
                if (point[1] > this.maxPointY[1]) {
                    this.maxPointY = point;
                }
                if (point[2] > this.maxPointZ[2]) {
                    this.maxPointZ = point;
                }
            }
            // Squared distance between each component min and max
            var xSpan = c3d.magnitudeSquared(c3d.subtract(this.maxPointX, this.minPointX));
            var ySpan = c3d.magnitudeSquared(c3d.subtract(this.maxPointY, this.minPointY));
            var zSpan = c3d.magnitudeSquared(c3d.subtract(this.maxPointZ, this.minPointZ));
            var diameter1 = this.minPointX;
            var diameter2 = this.maxPointX;
            var maxSpan = xSpan;
            if (ySpan > maxSpan) {
                maxSpan = ySpan;
                diameter1 = this.minPointY;
                diameter2 = this.maxPointY;
            }
            if (zSpan > maxSpan) {
                maxSpan = zSpan;
                diameter1 = this.minPointZ;
                diameter2 = this.maxPointZ;
            }
            var ritterCenter = new List<double> {
                (diameter1[0] + diameter2[0]) * 0.5,
                (diameter1[1] + diameter2[1]) * 0.5,
                (diameter1[2] + diameter2[2]) * 0.5
            };
            var radiusSquared = c3d.magnitudeSquared(c3d.subtract(diameter2, ritterCenter));
            var ritterRadius = math.sqrt(radiusSquared);
            // Initial center and radius (naive) get min and max box
            var minBoxPt = new List<object> {
                this.minPointX[0],
                this.minPointY[1],
                this.minPointZ[2]
            };
            var maxBoxPt = new List<object> {
                this.maxPointX[0],
                this.maxPointY[1],
                this.maxPointZ[2]
            };
            var naiveCenter = c3d.multiplyByScalar(c3d.add(minBoxPt, maxBoxPt), 0.5);
            var naiveRadius = 0.0;
            foreach (var i in xrange(0, nbPositions)) {
                var currentP = points[i];
                // Find the furthest point from the naive center to calculate the naive radius.
                var r = c3d.magnitude(c3d.subtract(currentP, naiveCenter));
                if (r > naiveRadius) {
                    naiveRadius = r;
                }
                // Make adjustments to the Ritter Sphere to include all points.
                var oldCenterToPointSquared = c3d.magnitudeSquared(c3d.subtract(currentP, ritterCenter));
                if (oldCenterToPointSquared > radiusSquared) {
                    var oldCenterToPoint = math.sqrt(oldCenterToPointSquared);
                    ritterRadius = (ritterRadius + oldCenterToPoint) * 0.5;
                    // Calculate center of new Ritter sphere
                    var oldToNew = oldCenterToPoint - ritterRadius;
                    ritterCenter = new List<object> {
                        old_div(ritterRadius * ritterCenter[0] + oldToNew * currentP[0], oldCenterToPoint),
                        old_div(ritterRadius * ritterCenter[1] + oldToNew * currentP[1], oldCenterToPoint),
                        old_div(ritterRadius * ritterCenter[2] + oldToNew * currentP[2], oldCenterToPoint)
                    };
                }
            }
            // Keep the naive sphere if smaller
            if (naiveRadius < ritterRadius) {
                this.radius = ritterRadius;
                this.center = ritterCenter;
            } else {
                this.radius = naiveRadius;
                this.center = naiveCenter;
            }
        }
    }
}
