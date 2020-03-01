using System;
using VSS.TRex.QuantizedMesh.Models;

namespace VSS.TRex.QuantizedMesh.MeshUtils
{
  public static class Cartesian3D
  {

    public static Double MagnitudeSquared(Vector3 p)
    {
      return Math.Pow(p.X, 2) + Math.Pow(p.Y, 2) + Math.Pow(p.Z, 2);
    }

    public static Double Magnitude(Vector3 p)
    {
      return Math.Sqrt(MagnitudeSquared(p));
    }

    public static Vector3 Add(Vector3 left, Vector3 right)
    {
      return new Vector3()
      {
        X = left.X + right.X,
        Y = left.Y + right.Y,
        Z = left.Z + right.Z
      };
    }

    public static Vector3 Subtract(Vector3 left, Vector3 right)
    {
      return new Vector3()
      {
        X = left.X - right.X,
        Y = left.Y - right.Y,
        Z = left.Z - right.Z
      };
    }

    public static Double DistanceSquared(Vector3 p1, Vector3 p2)
    {
      return Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2) + Math.Pow(p1.Z - p2.Z, 2);
    }

    public static Double Distance(Vector3 p1, Vector3 p2)
    {
      return Math.Sqrt(DistanceSquared(p1, p2));
    }

    public static Vector3 MultiplyByScalar(Vector3 p, Double scalar)
    {
      return new Vector3()
      {
        X = p.X * scalar,
        Y = p.Y * scalar,
        Z = p.Z * scalar
      };
    }

    public static Vector3 Normalize(Vector3 p)
    {
      var mgn = Magnitude(p);
      return new Vector3()
      {
        X = p.X / mgn,
        Y = p.Y / mgn,
        Z = p.Z / mgn
      };
    }

    public static Vector3 XNormalize(Vector3 p)
    {

      double num1 = Math.Abs(p.X);
      double num2 = Math.Abs(p.Y);
      double num3 = Math.Abs(p.Z);
      if (num2 > num1)
        num1 = num2;
      if (num3 > num1)
        num1 = num3;
      p.X /= num1;
      p.Y /= num1;
      p.Z /= num1;
      var mgn = Math.Sqrt(p.X * p.X + p.Y * p.Y + p.Z * p.Z);
      return new Vector3()
      {
        X = p.X / mgn,
        Y = p.Y / mgn,
        Z = p.Z / mgn
      };
    }

    public static Vector3 CrossProduct(Vector3 org, Vector3 ptA, Vector3 ptB)
    {
      Vector3 a = Subtract(ptA, org);
      Vector3 b = Subtract(ptB, org);
      return new Vector3(a.Y * b.Z - b.Y * a.Z, a.Z * b.X - b.Z * a.X, a.X * b.Y - b.X * a.Y);
    }

    public static Vector3 ComputeTriangleNormal(Vector3 v1, Vector3 v2, Vector3 v3)
    {
      return Normalize(CrossProduct(v1, v2, v3));
    }

    /// <summary>
    /// From each connecting face normal working out the value for the vertext normal
    /// </summary>
    /// <param name="gridSize"></param>
    /// <param name="vectorNormals"></param>
    /// <param name="Faces"></param>
    public static void ComputeSmoothShading(int gridSize, ref Vector3[] vectorNormals, ref Vector3[] Faces)
    {

      // each vertex nornmal is effected by all connecting faces
      // This may require some experiementation to get the best results

      var ptr = 0;
      vectorNormals[ptr] = Normalize(Add(Faces[0], Faces[1])); // two triangles bottom left

      // bottom row one in only as triangles are all above vertex
      for (var x = 1; x < gridSize; x++)
      {
        ptr = x * 2 - 1;
        if (x == gridSize - 1)
          vectorNormals[x] = Normalize(Faces[ptr]); // one face
        else
          vectorNormals[x] = Normalize(Add(Add(Faces[ptr], Faces[ptr + 1]), Faces[ptr + 2]));
      }

      // now the rest working up left to right
      ptr = 0;
      var idx = gridSize;
      var idxStep = (gridSize - 1) * 2;
      var idx3 = (gridSize - 1) * 2 - 1;
      for (var y = 1; y < gridSize; y++) // note one up
      {
        if (y != gridSize - 1)
          ptr += idxStep;
        var yIdx = ptr;
        var lastRow = 0;
        var midRows = 0;
        for (var x = 0; x < gridSize; x++)
        {
          if (y == gridSize - 1) // last row faces are down
          {
            if (x == 0)
              vectorNormals[idx] = Normalize(Faces[ptr]);
            else if (x == gridSize - 1)
              vectorNormals[idx] = Normalize(Add(Faces[ptr + ((x - 1) * 2)], Faces[ptr + ((x - 1) * 2) + 1]));
            else
            {
              vectorNormals[idx] = Normalize(Add(Add(Faces[yIdx + lastRow], Faces[yIdx + lastRow + 1]), Faces[yIdx + lastRow + 2]));
              lastRow += 2;
            }
          }
          else if (x == 0) // 3 faces
          {
            vectorNormals[idx] = Normalize(Add(Add(Faces[ptr], Faces[ptr + 1]), Faces[ptr - idxStep]));
          }
          else if (x == gridSize - 1) // 3 faces last box in row
          {
            vectorNormals[idx] = Normalize(Add(Add(Faces[ptr - 1], Faces[ptr - 2]), Faces[ptr + idx3]));
          }
          else // 6 faces for middle section
          {
            vectorNormals[idx] = Normalize(Add(Add(Add(Faces[yIdx + x], Faces[yIdx + x + 1]), Faces[yIdx + x + 2]), Add(Add(Faces[yIdx - idxStep + midRows], Faces[yIdx - idxStep + 1 + midRows]), Faces[yIdx - idxStep + 2 + midRows])));
            midRows++;
            yIdx++;
          }
          idx++;
        }
      }
    }

    public static void ComputeSmoothShadingFull(int gridSize, ref Vector3[] vectorNormals, ref Vector3[] Faces)
    {

      var ptr = 0;

      vectorNormals[ptr] = Normalize(Add(Faces[0], Faces[1]));

      // start bottom row
      for (var x = 1; x < gridSize; x++)
      {
        ptr = x * 2 - 1;
        if (x == gridSize - 1)
          vectorNormals[x] = Normalize(Faces[ptr - 1]); // one face
        else
          vectorNormals[x] = Normalize(Add(Add(Faces[ptr - 1], Faces[ptr + 2]), Faces[ptr + 1]));
      }

      // now the rest working up left to right
      ptr = 0;
      var idx = gridSize;
      var idxStep = (gridSize - 1) * 2;
      var idx3 = (gridSize - 1) * 2 - 2;
      for (var y = 1; y < gridSize; y++)
      {
        if (y != gridSize - 1)
          ptr += idxStep;
        var yIdx = ptr;
        var lastRow = 0;
        var midRows = 0;
        var step2 = 2;


        for (var x = 0; x < gridSize; x++)
        {
          if (y == gridSize - 1) // last row
          {
            if (x == 0)
              vectorNormals[idx] = Normalize(Faces[ptr + 1]);
            else if (x == gridSize - 1)
              vectorNormals[idx] = Normalize(Add(Faces[ptr + ((x - 1) * 2)], Faces[ptr + ((x - 1) * 2) + 1]));
            else
            {
              vectorNormals[idx] = Normalize(Add(Add(Faces[yIdx + lastRow], Faces[yIdx + lastRow + 1]), Faces[yIdx + lastRow + 3]));
              lastRow += 2;
            }
          }
          else if (x == 0) // 3 faces
          {
            vectorNormals[idx] = Normalize(Add(Add(Faces[ptr], Faces[ptr + 1]), Faces[ptr - idxStep + 1]));
          }
          else if (x == gridSize - 1) // 3 faces last box in row
          {
            vectorNormals[idx] = Normalize(Add(Add(Faces[ptr - 1], Faces[ptr - 2]), Faces[ptr + idx3]));
          }
          else // 6 faces for middle section
          {
            //            vectorNormals[idx] = Normalize(Add(Add(Add(Faces[yIdx], Faces[yIdx + 3]), Faces[yIdx + 2]), Add(Add(Faces[yIdx - idxStep + midRows], Faces[yIdx - idxStep + 1 + midRows]), Faces[yIdx - idxStep + 3 + midRows])));
            vectorNormals[idx] = Normalize(Add(Add(Add(Faces[yIdx], Faces[yIdx + 3]), Faces[yIdx + 2]),
                                           Add(Add(Faces[yIdx - idxStep], Faces[yIdx - idxStep + 1]), Faces[yIdx - idxStep + 3])));
            midRows++;
            yIdx += 2;
            step2++;

          }
          idx++;
          //  ptr++;
        }
      }

    }



    /// <summary>
    /// Returns correct sign if less than zero
    /// </summary>
    /// <param name="v"></param>
    /// <returns>sign -1.0 or 1.0</returns>
    public static double SignNotZero(double v)
    {
      return v < 0.0 ? -1.0 : 1.0;
    }

    /// <summary>
    ///  Constraint a value to lie between two values.
    /// </summary>
    /// <param name="value">value The value to constrain.</param>
    /// <param name="min">min The minimum value</param>
    /// <param name="max">max The maximum value.</param>
    /// <returns>The value clamped so that min <= value <= max.</returns>
    public static double Clamp(double value, double min, double max)
    {
      return value < min ? min : value > max ? max : value;
    }

    /// <summary>
    /// Converts a scalar value in the range [-1.0, 1.0] to a SNORM in the range [0, rangeMax]
    /// </summary>
    /// <param name="value">value The scalar value in the range [-1.0, 1.0]</param>
    /// <param name="rangeMax">[rangeMax=255] The maximum value in the mapped range, 255 by default.</param>
    /// <returns>A SNORM value, where 0 maps to -1.0 and rangeMax maps to 1.0.</returns>
    public static double ToSNorm(double value, double rangeMax)
    {
      return Math.Round((Clamp(value, -1.0, 1.0) * 0.5 + 0.5) * rangeMax);
    }

    /// <summary>
    /// Encodes vertec normal into two bytes 
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector2 OctEncode(Vector3 vector)
    {
      Vector2 result = new Vector2(0, 0);
      var rangeMax = 255;
      var EPSILON6 = 0.000001;
      var magSquared = Cartesian3D.MagnitudeSquared(vector);
      if (Math.Abs(magSquared - 1.0) > EPSILON6)
      {
        throw new ArithmeticException("vector must be normalized.");
      }
      result.X = vector.X / (Math.Abs(vector.X) + Math.Abs(vector.Y) + Math.Abs(vector.Z));
      result.Y = vector.Y / (Math.Abs(vector.X) + Math.Abs(vector.Y) + Math.Abs(vector.Z));
      if (vector.Z < 0)
      {
        var x = result.X;
        var y = result.Y;
        result.X = (1.0 - Math.Abs(y)) * SignNotZero(x);
        result.Y = (1.0 - Math.Abs(x)) * SignNotZero(y);
      }
      result.X = ToSNorm(result.X, rangeMax);
      result.Y = ToSNorm(result.Y, rangeMax);
      return result;
    }

    /// <summary>
    /// Computes normal map from triangle faces
    /// </summary>
    /// <param name="triangles"></param>
    /// <param name="gridSize"></param>
    /// <returns></returns>
    public static byte[] ComputeVertextNormals(ref Triangle[] triangles, int gridSize)
    {
      var numFaces = (gridSize - 1) * (gridSize - 1) * 2;
      var numVertices = gridSize * gridSize;
      var Faces = new Vector3[numFaces];
      var vectorNormals = new Vector3[numVertices];
      var packedVectorNormals = new byte[gridSize * gridSize * 2];

      for (var i = 0; i < numFaces; i++)
        Faces[i] = ComputeTriangleNormal(triangles[i].V1, triangles[i].V2, triangles[i].V3);

      // Here we can experiment with different methods of shading to get best results
      //      ComputeSmoothShading(gridSize, ref vectorNormals, ref Faces);
      ComputeSmoothShadingFull(gridSize, ref vectorNormals, ref Faces);

      // Pack vertex normals for QM tile
      var p = 0;
      for (var i = 0; i < vectorNormals.Length; i++)
      {
        //  keep as a reminder 
        // if (vectorNormals[i].IsZeroed())
        //  vectorNormals[i].Z = 1;
        var octNorm = OctEncode(vectorNormals[i]);
        packedVectorNormals[p] = (byte)octNorm.X;
        packedVectorNormals[p + 1] = (byte)octNorm.Y;
        p += 2;
      }
      return packedVectorNormals; //  octencode normals
    }
  }
}
