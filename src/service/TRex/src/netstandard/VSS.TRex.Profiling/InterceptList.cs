using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Utilities;

namespace VSS.TRex.Profiling
{
  /// <summary>
  /// Contains a vector of InterceptRec instances that describe all the cells a profile line has crossed
  /// </summary>
  public class InterceptList
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<InterceptList>();

    /// <summary>
    /// The amount by which the size of the list should be increased as more elements are added.
    /// This is also the initial size of the intercept rec list.
    /// </summary>
    public const int ListInc = 2000;

    /// <summary>
    /// Profile lines which require more intercepts than this are not supported
    /// </summary>
    public const int MaxIntercepts = 20000; // note this should give a profile at least up to 4-6km long

    /// <summary>
    /// Explicit count of the number of intercepts in the vector. This is held distinctly from the count
    /// of elements in the list itself due to recycling of elements
    /// </summary>
    public int Count;

    /// <summary>
    /// The array of intercept items currently provisioned for the list
    /// </summary>
    public InterceptRec[] Items = new InterceptRec[ListInc];

    /// <summary>
    /// Construct a default empty intercept list
    /// </summary>
    public InterceptList()
    {
    }

    /// <summary>
    /// Adds a new point to the list, unless the point being added is equal to the last point in the list
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="ind"></param>
    public void AddPoint(double x, double y, double ind)
    {
      if (Count > 0 && Items[Count - 1].Equals(x, y, ind))
        return;

      if (Count >= MaxIntercepts)
        return;

      if (Count >= Items.Length)
        Array.Resize(ref Items, Items.Length + ListInc);

      Items[Count] = new InterceptRec {OriginX = x, OriginY = y, ProfileItemIndex = ind};

      Count++;
    }

    /// <summary>
    /// Adds a new point to the list, unless the point being added is equal to the last point in the list
    /// </summary>
    /// <param name="point"></param>
    public void AddPoint(InterceptRec point)
    {
      if (Count > 0 && Items[Count - 1].Equals(point))
        return;

      if (Count >= MaxIntercepts)
        return;

      if (Count >= Items.Length)
        Array.Resize(ref Items, Items.Length + ListInc);

      Items[Count] = point;

      Count++;
    }

    /// <summary>
    /// Updates the mid point locations of all the intercept/intervals contained in the list.
    /// </summary>
    public void UpdateMergedListInterceptMidPoints()
    {
      if (Count == 0)
        return;

      for (int i = 0; i < Count - 1; i++)
      {
        // Grab a pair of adjacent intercepts
        InterceptRec InterceptA = Items[i];
        InterceptRec InterceptB = Items[i + 1];

        double IntLength = MathUtilities.Hypot(InterceptB.OriginX - InterceptA.OriginX, InterceptB.OriginY - InterceptA.OriginY);

        // Calculate the midpoint of the line between the pair of intercepts
        double MPX = (InterceptA.OriginX + InterceptB.OriginX) / 2;
        double MPY = (InterceptA.OriginY + InterceptB.OriginY) / 2;

        Items[i] = new InterceptRec(Items[i].OriginX, Items[i].OriginY, MPX, MPY, Items[i].ProfileItemIndex, IntLength);
      }

      // Discard the last intercept as it's already been used as the end point of the previous pair of intercepts
      Count--;
    }

    /// <summary>
    /// Takes two intercept lists and merges the two into a single list within this instance
    /// </summary>
    /// <param name="List1"></param>
    /// <param name="List2"></param>
    public void MergeInterceptLists(InterceptList List1, InterceptList List2)
    {
      int Index1 = 0;
      int Index2 = 0;

      // Move through both lists, adding points from both in increasing chainage order
      // We assume that the points in the two lists are already in an increasing chainage order.

      while (Index1 < List1.Count || Index2 < List2.Count)
      {
        while (Index1 < List1.Count &&
               (Index2 >= List2.Count ||
                List1.Items[Index1].ProfileItemIndex <= List2.Items[Index2].ProfileItemIndex))
        {
          if (Count >= MaxIntercepts)
            break;

          AddPoint(List1.Items[Index1]);
          Index1++;
        }

        while (Index2 < List2.Count &&
               (Index1 >= List1.Count ||
                List2.Items[Index2].ProfileItemIndex <= List1.Items[Index1].ProfileItemIndex))
        {
          if (Count >= MaxIntercepts)
            break;

          AddPoint(List2.Items[Index2]);
          Index2++;
        }

        if (Count >= MaxIntercepts)
        {
          Log.LogInformation($"Profile Intercept list truncated at {MaxIntercepts} intercepts");
          break;
        }
      }
    }
  }
}
