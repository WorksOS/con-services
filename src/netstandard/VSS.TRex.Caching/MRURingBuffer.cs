using System;

namespace VSS.TRex.Caching
{
  /// <summary>
  /// Stores a buffer of elements in a ring and provides semantics to add elements to the 'head' of the buffer and to remove elements at any point in the buffer
  /// Elements stored in the ring buffer are defined by the generic type T
  /// </summary>
  public class MRURingBuffer<T> : IMRURingBuffer<T> where T : ITRexMemoryCacheItem
  {
    public const long MAX_SUPPORTED_SLOTS = 1000000000;

    public ITRexSpatialMemoryCache OwnerCache = null;

    /// <summary>
    /// Maximum expected number of elements to be stored in the ring buffer.
    /// </summary>
    public int MaxNumElements { get; private set; }

    /// <summary>
    /// A multiplier applied to the MaxNumElements parameter to allow for efficient operation by ignoring fragmentation.
    /// </summary>
    public double FragmentationMultiplier { get; private set; }

    /// <summary>
    /// The fractional size of the slot space where references elements are not moved to the head of the MRU list represented in the ring buffer
    /// </summary>
    public double MruDeadBandFraction { get; private set; }

    /// <summary>
    /// The token indicating the position of the ring buffer head within the array of slots held in Elements
    /// </summary>
    private long currentToken = 0;

    /// <summary>
    /// The total number of slots allocated to store elements in the rung buffer
    /// </summary>
    private int TotalSlots = 0;

    /// <summary>
    /// The number of slots near the head of the ring buffer that should not be moved when accessed for MRU
    /// </summary>
    private int MruNonUpdateableSlotCount = 0;

    /// <summary>
    /// The array containing all the slots within the ring buffer
    /// </summary>
    private T[] ElementSlots = null;

    /// <summary>
    /// Obtains slot token for the next slot in the ring buffer. This is a thread safe operation. 
    /// </summary>
    /// <returns></returns>
    private long NextToken() => System.Threading.Interlocked.Increment(ref currentToken);

    /// <summary>
    /// Default constructor is hidden
    /// </summary>
    private MRURingBuffer() { }

    /// <summary>
    /// Creates a new ring buffer in T
    /// </summary>
    /// <param name="maxNumElements">The maximum number of elements to be stored in the ring buffer</param>
    /// <param name="fragmentationMultiplier">A multiplier factor to derive the number of slots to create in the ring buffer to take into account likely fragmentation in the buffer due to element LRU updates and eviction</param>
    /// <param name="mruDeadBandFraction">The most recently used fraction of the slot space within the ring buffer queue that is not subject to moving to the head of the ring buffer when touched. EG: A value of 0.10 means any element touched in the top 10% of the slots will not be moved to the head of the ring buffer</param>
    public MRURingBuffer(ITRexSpatialMemoryCache ownerCache, int maxNumElements, double fragmentationMultiplier, double mruDeadBandFraction)
    {
      if (maxNumElements < 1 || maxNumElements > MAX_SUPPORTED_SLOTS)
        throw new ArgumentException($"maxNumElements ({maxNumElements}) not in range 1..{MAX_SUPPORTED_SLOTS}");

      if (fragmentationMultiplier < 1 || fragmentationMultiplier > 10)
        throw new ArgumentException($"Fragmentation factor {fragmentationMultiplier} is less than 1.0 or more than 10.0");

      if (mruDeadBandFraction < 0.0 || mruDeadBandFraction > 1.0)
        throw new ArgumentException($"mruDeadBandFraction ({mruDeadBandFraction}) not in range 0.0..1.0");

      TotalSlots = (int)Math.Truncate(maxNumElements * fragmentationMultiplier);

      if (TotalSlots < 1 || maxNumElements > MAX_SUPPORTED_SLOTS)
        throw new ArgumentException($"Total number of slots in the rung buffer ({TotalSlots}) not in range 1..{MAX_SUPPORTED_SLOTS}");

      OwnerCache = ownerCache ?? throw new ArgumentException("Cannot create an MRU ring buffer without an owning memory cache");

      MaxNumElements = maxNumElements;
      FragmentationMultiplier = fragmentationMultiplier;
      MruNonUpdateableSlotCount = (int) Math.Truncate(TotalSlots * mruDeadBandFraction);

      // Construct the array of slots to hold the elements in the ring buffer taking into account the fragmentation multiplier
      ElementSlots = new T[TotalSlots];
    }

    /// <summary>
    /// Adds an element of type T into the ring buffer. The element is always added to the the head of the ring buffer.
    /// </summary>
    /// <param name="element">The element in T to be added to the ring buffer</param>
    /// <returns>A token representing the location within the ring buffer containing this element</returns>
    public long Put(T element)
    {
      // Determine the next location token
      long token = NextToken();

      // Convert the location token to a physical index and store the element
      ElementSlots[token % TotalSlots] = element;

      // Send back the token which may be used to reference this element in future
      return token;
    }

    /// <summary>
    /// Gets an element from the cache given the token previously returned when the element was added to the cache
    /// This is a thread safe operation. If an element is moved it is guarded by a write lock against the ring buffer that permits
    /// concurrent read operations.
    /// </summary>
    /// <param name="token">The token representing the location of the element. If the element is shifted to the MRU location this token is changed to reflect the new location</param>
    /// <returns>The element at the location referenced by token.</returns>
    public T Get(ref long token)
    {
      // Take a copy of token to prevent issues with it being changed in concurrent threads
      long localToken = token;

      // Obtain the actual element being referenced
      T theElement = ElementSlots[(int) (localToken % TotalSlots)];

      // Check if MRU location needs to be changed
      if (currentToken - MruNonUpdateableSlotCount > localToken)
      {
        // Lock the ring buffer for the duration of the move
        lock (this)
        {
          // the location of token is outside of the MRU dead band...

          // Check this thread was the 'winner' of the lock above
          if (currentToken - MruNonUpdateableSlotCount > localToken)
          {
            // Convert the location token to a physical index
            int oldSlotLocation = (int) (localToken % TotalSlots);

            // Get the new MRU location for the element
            long newToken = NextToken();
            int newSlotLocation = (int) (newToken % TotalSlots);

            // Resettle the element in the new location
            ElementSlots[newSlotLocation] = ElementSlots[oldSlotLocation];

            // Update the supplied token to reference the new location for the element in the ring buffer
            token = newToken;

            ElementSlots[oldSlotLocation] = default(T);
          }
        }
      }

      // Hand the element back to the caller with no change to the token. Note, it is possible, though highly unlikely, that 
      // the returned element
      return theElement;
    }

    /// <summary>
    /// Removes an element from the cache given the token previously returned when the element was added to the cache.
    /// This is a thread safe operation, no locks are obtained in its operation
    /// </summary>
    /// <param name="token"></param>
    /// <returns>The element at the location referenced by token at the time of its removal.</returns>
    public T Remove(long token)
    {
      // Convert the location token to a physical index and return the element
      T result = ElementSlots[token % TotalSlots];

      ElementSlots[token % TotalSlots] = default(T);

      return result;
    }
  }
}
