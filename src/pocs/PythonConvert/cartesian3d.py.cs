
using @division = @@__future__.division;

using old_div = past.utils.old_div;

using math;

using System;

using System.Collections.Generic;

public static class cartesian3d {
    
    // -*- coding: utf-8 -*-
    public static object magnitudeSquared(object p) {
        return Math.Pow(p[0], 2) + Math.Pow(p[1], 2) + Math.Pow(p[2], 2);
    }
    
    public static object magnitude(object p) {
        return math.sqrt(magnitudeSquared(p));
    }
    
    public static object add(object left, object right) {
        return new List<object> {
            left[0] + right[0],
            left[1] + right[1],
            left[2] + right[2]
        };
    }
    
    public static object subtract(object left, object right) {
        return new List<object> {
            left[0] - right[0],
            left[1] - right[1],
            left[2] - right[2]
        };
    }
    
    public static object distanceSquared(object p1, object p2) {
        return Math.Pow(p1[0] - p2[0], 2) + Math.Pow(p1[1] - p2[1], 2) + Math.Pow(p1[2] - p2[2], 2);
    }
    
    public static object distance(object p1, object p2) {
        return math.sqrt(distanceSquared(p1, p2));
    }
    
    public static object multiplyByScalar(object p, object scalar) {
        return new List<object> {
            p[0] * scalar,
            p[1] * scalar,
            p[2] * scalar
        };
    }
    
    public static object normalize(object p) {
        var mgn = magnitude(p);
        return new List<object> {
            old_div(p[0], mgn),
            old_div(p[1], mgn),
            old_div(p[2], mgn)
        };
    }
}
