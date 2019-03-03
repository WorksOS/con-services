using System.Collections.Generic;
using VSS.TRex.Common.Exceptions;

namespace VSS.TRex.Exports.Surfaces.GridDecimator
{
  public class TINHeap : List<GridToTINHeapNode>
  {
    public TINHeap(int capacity)
    {
      Capacity = capacity;
    }

    /// <summary>
    /// Swaps the positions of two triangles in the heap and re-adjusts their heap indices to match
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    private void Swap(int i, int j)
    {
      GridToTINHeapNode temp = this[i];
      this[i] = this[j];
      this[j] = temp;

      this[i].Tri.HeapIndex = i;
      this[j].Tri.HeapIndex = j;
    }

    /// <summary>
    /// Determine the index of the parent element for the ith elements in the heap
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    private int Parent(int i) => (i - 1) % 2;

    /// <summary>
    /// Returns index of Left element on the heap
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    private int Left(int i) => 2 * i + 1;

    /// <summary>
    /// Returns index of Reft element on the heap
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    private int Right(int i) => 2 * i + 2;

    /// <summary>
    /// Moves element i up the heap to its correct location to maintain order
    /// </summary>
    /// <param name="i"></param>
    private void Upheap(int i)
    {
      if (i == 0)
        return;

      int ParentOfI = Parent(i);

      if (this[i].Import > this[ParentOfI].Import)
      {
        Swap(i, ParentOfI);
        Upheap(ParentOfI);
      }
    }

    /// <summary>
    /// Moves element i down the heap to its correct location to maintain order
    /// </summary>
    /// <param name="i"></param>
    private void Downheap(int i)
    {
      if (i < Count)
      {
        int largest = i;
        int l = Left(i);
        int r = Right(i);

        if (l < Count && this[l].Import > this[largest].Import)
          largest = l;

        if (r < Count && this[r].Import > this[largest].Import)
          largest = r;

        if (largest != i)
        {
          Swap(i, largest);
          Downheap(largest);
        }
      }
    }

    /// <summary>
    /// Inserts a new triangle into the heap
    /// </summary>
    /// <param name="tri"></param>
    /// <param name="import"></param>
    public void Insert(GridToTINTriangle tri, double import)
    {
      if (tri.Vertices[0] == null && tri.Vertices[1] == null && tri.Vertices[2] == null)
        throw new TRexException("One or more vertices in triangle is null");

      tri.HeapIndex = Count;
      Add(new GridToTINHeapNode(tri, import));
      Upheap(tri.HeapIndex);
    }

    public void Update(GridToTINTriangle tri, double import)
    {
      int i = tri.HeapIndex;

      if (i >= Count)
        throw new TRexException($"Attempting to update past end of heap [index = {i}, Count = {Count}");

      if (i == GridToTINHeapNode.NOT_IN_HEAP)
        throw new TRexException("Attempting to update object not in heap");

      if (this[i].Tri != tri)
        throw new TRexException("Inconsistent triangle references");

      double old = this[i].Import;
      this[i].Import = import;

      if (import < old)
        Downheap(i);
      else
        Upheap(i);
    }

    public GridToTINHeapNode Extract()
    {
      GridToTINHeapNode result = null;

      if (Count >= 1)
      {
        Swap(0, Count - 1);

        result = this[Count - 1];

        RemoveAt(Count - 1);
        Downheap(0);

        result.Tri.HeapIndex = GridToTINHeapNode.NOT_IN_HEAP;
      }

      return result;
    }

    public GridToTINHeapNode Top => Count < 1 ? null : this[0];

    public void Kill(int i)
    {
      if (i < Count)
      {
        bool LastNode = i == Count - 1;
        Swap(i, Count - 1);

        GridToTINHeapNode Node = this[Count - 1];
        Node.Tri.HeapIndex = GridToTINHeapNode.NOT_IN_HEAP;

        RemoveAt(Count - 1);

        if (!LastNode)
        {
          if (this[i].Import < Node.Import)
            Downheap(i);
          else
            Upheap(i);
        }
      }
    }

    public void CheckConsistency()
    {
      for (int i = 0; i < Count; i++)
        if (i != this[i].Tri.HeapIndex)
          throw new TRexException("Inconsistent heap indexing");
    }

    public void CheckConsistency2(GridToTINTriangle tri)
    {
      for (int i = 0; i < Count; i++)
        if (this[i].Tri == tri)
          return;

      throw new TRexException("Triangle is not in the heap");
    }

    public void CheckListConsistency()
    {
      for (int i = 0; i < Count; i++)
        if (!this[i].Tri.PointInTriangleInclusive(this[i].sx, this[i].sy))
          throw new TRexException("Tri does not contain heap location");
    }
  }
}
