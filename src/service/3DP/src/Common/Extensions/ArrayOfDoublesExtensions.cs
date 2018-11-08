using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Productivity3D.Common.Extensions
{
  public static class ArrayOfDoublesExtensions
  {
    /// <summary>
    /// Adds 0.0 value to the source array, remove any duplicates and sort contents by specified order...
    /// </summary>
    /// <param name="sourceArray">The source array of doubles.</param>
    /// <param name="ascendingOrder">The sort order (ascending or descending). The defoult is true.</param>
    /// <returns>An updated array of doubles.</returns>
    /// 
    public static double[] AddZeroDistinctSortBy(this double[] sourceArray, bool ascendingOrder = true)
    {
      var updatedOffsetsList = sourceArray.ToList();

      updatedOffsetsList.Add(0.0);

      if (ascendingOrder)
        return updatedOffsetsList.Distinct().OrderBy(d => d).ToArray();

      return updatedOffsetsList.Distinct().OrderByDescending(d => d).ToArray();
    }
  }
}
