using System;
using System.Collections.Generic;

namespace XnaFan.ImageComparison.Netcore.XnaFan.ImageComparison
{
  /// <summary>
  /// Helper class which compares tuples with imagepath and grayscale based on the values in their grayscale array
  /// </summary>
  internal class PathGrayscaleTupleComparer : IComparer<Tuple<string, byte[,]>>
  {
    private static readonly ArrayComparer<byte> comparer = new ArrayComparer<byte>();

    public int Compare(Tuple<string, byte[,]> x, Tuple<string, byte[,]> y) => comparer.Compare(x.Item2, y.Item2);
  }
}
