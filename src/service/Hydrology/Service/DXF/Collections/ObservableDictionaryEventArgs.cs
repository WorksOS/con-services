using System;
using System.Collections.Generic;

namespace VSS.Hydrology.WebApi.DXF.Collections
{
    /// <summary>
    /// Represents the arguments thrown by the <c>ObservableDictionaryEventArgs</c> events.
    /// </summary>
    /// <typeparam name="TKey">Type of items.</typeparam>
    /// <typeparam name="TValue">Type of items.</typeparam>
    public class ObservableDictionaryEventArgs<TKey, TValue> :
        EventArgs
    {
        #region private fields

        private readonly KeyValuePair<TKey, TValue> item;
        private bool cancel;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of <c>ObservableDictionaryEventArgs</c>.
        /// </summary>
        /// <param name="item">Item that is being added or removed from the dictionary.</param>
        public ObservableDictionaryEventArgs(KeyValuePair<TKey, TValue> item)
        {
            this.item = item;
            this.cancel = false;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Get the item that is being added to or removed from the dictionary.
        /// </summary>
        public KeyValuePair<TKey, TValue> Item
        {
            get { return this.item; }
        }

        /// <summary>
        /// Gets or sets if the operation must be canceled.
        /// </summary>
        /// <remarks>This property is used by the OnBeforeAdd and OnBeforeRemove events to cancel the add or remove operations.</remarks>
        public bool Cancel
        {
            get { return this.cancel; }
            set { this.cancel = value; }
        }

        #endregion
    }
}
