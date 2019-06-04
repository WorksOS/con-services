using System;
using System.Collections.Generic;

namespace VSS.TRex.TAGFiles.Classes.ValueMatcher
{
    /// <summary>
    /// AccumulatedAttributes stores a list of AccumulatedAttribute instances
    /// the record a series of attribute value observations
    /// </summary>
    public class AccumulatedAttributes<T>
    {
        /// <summary>
        /// List of items being tracked. This list is managed with the aid of the NumAttrs field to
        /// remove the need to resize the attributes list frequently when all but the latest attributes are
        /// discarded.
        /// </summary>
        private readonly List<AccumulatedAttribute<T>> list = new List<AccumulatedAttribute<T>>();

        public AccumulatedAttributes()
        {
        }

        /// <summary>
        /// Provides the count of the internal list of attributes being maintained
        /// </summary>
        /// <returns></returns>
        public int NumAttrs { get; set; }

        // DiscardAllButLatest discards all but the most recently added
        // value from the list;
        public void DiscardAllButLatest()
        {
            if (NumAttrs > 1)
            {
                list[0] = list[NumAttrs - 1];
                NumAttrs = 1;
            }
        }
       
        /// <summary>
        /// Add adds a value recorded at DateTime to the list. It is assumed
        /// that the date time of this item is after the date time
        /// of any previously added item in the list
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="value"></param>
        public void Add(DateTime dateTime, T value)
        {
          if (dateTime.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Attribute time must be a UTC cell pass time", nameof(dateTime));

          // If there are available entries to reuse, then reuse them...
          if (NumAttrs < list.Count)
            list[NumAttrs] = new AccumulatedAttribute<T>(dateTime, value);
          else
            list.Add(new AccumulatedAttribute<T>(dateTime, value));

          NumAttrs++;
        }

        // GetLatest simply returns the last value in the list
        public T GetLatest()
        {
            return NumAttrs > 0 ? list[NumAttrs - 1].value : default(T);
        }

        // GetValueAtDateTime locates the value appropriate for the given datetime
        // Requesting a value for a datetime prior to the first value in the list
        // will return the first value in the list. If there are no values in the list
        // then the function returns false.
        public bool GetValueAtDateTime(DateTime dateTime, out T value)
        {
            value = default(T);

            switch (NumAttrs)
            {
                case 0:
                    return false;
                case 1:
                    value = list[0].value;
                    return true;
            }

            if (dateTime < list[0].dateTime)
            {
                value = list[0].value;
                return true;
            }

            if (dateTime >= list[NumAttrs - 1].dateTime)
            {
                value = list[NumAttrs - 1].value;
                return true;
            }

            bool found = false;
            for (int I = NumAttrs - 1; I >= 0; I--)
            {
              if (dateTime >= list[I].dateTime)
              {
                value = list[I].value;
                found = true;
                break;
              }
            }
            
            return found;
        }

        /// <summary>
        /// Gets the appropriate GPSMode given a UTC datetime
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetValueAtDateTime(DateTime dateTime, T defaultValue)
        {
          return GetValueAtDateTime(dateTime, out T value) ? value : defaultValue;
        }
    }
}
