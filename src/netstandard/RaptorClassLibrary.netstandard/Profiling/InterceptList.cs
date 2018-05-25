using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace VSS.TRex.Profiling
{
  public class InterceptList
  {
    private static ILogger Log = Logging.Logger.CreateLogger<InterceptList>();

    public const int ListInc = 2000;
    public const int MaxIntercepts = 20000; // note this should give a profile at least up to 4-6km long

    public int Count;

    public InterceptRec[] Items = new InterceptRec[ListInc];

    public InterceptList()
    {
    }

    /// <summary>
    /// Adds a new point to the list, unless the point being added is equal to the last point in the list
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="ind"></param>
    public void AddPoint(float x, float y, float ind)
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

      float FirstOriginX = 0; // X-axis offset of first intercept in pair from start point of requested profile
      float IntLength = 0;

      for (int i = 0; i < Count - 2; i++)
      {
        // Grab a pair of adjacent intercepts
        InterceptRec InterceptA = Items[i];
        InterceptRec InterceptB = Items[i + 1];

        // If this is the first pair, then the X-axis offset of the intercept line is 0
        // else the X-axis offset of the intercept line is the previous offset plus
        // the length of the previous intercept line
        FirstOriginX = i == 0 ? 0 : FirstOriginX + IntLength;

        IntLength = (float) Math.Sqrt(Math.Pow(InterceptB.OriginX - InterceptA.OriginX, 2) +
                                      Math.Pow(InterceptB.OriginY - InterceptA.OriginY, 2));

        float MPX, MPY; // Mid-point of line between a pair of intercepts

        // Calculate the midpoint of the line between the pair of intercepts
        if (InterceptA.OriginX < InterceptB.OriginX)
          MPX = InterceptA.OriginX + (InterceptB.OriginX - InterceptA.OriginX) / 2;
        else
          MPX = InterceptA.OriginX - (InterceptA.OriginX - InterceptB.OriginX) / 2;

        if (InterceptA.OriginY < InterceptB.OriginY)
          MPY = InterceptA.OriginY + (InterceptB.OriginY - InterceptA.OriginY) / 2;
        else
          MPY = InterceptA.OriginY - (InterceptA.OriginY - InterceptB.OriginY) / 2;

        Items[i] = new InterceptRec(FirstOriginX, Items[i].OriginY, MPX, MPY, Items[i].ProfileItemIndex, IntLength);
      }

      // Discard the last intercept as it's already been used as the end point of the previous pair of intercepts
      Count--;
    }

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
