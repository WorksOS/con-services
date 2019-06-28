using System;

namespace VSS.Hydrology.WebApi.DXF.Collections
{
    /// <summary>
    /// Represents the arguments thrown by the <c>ObservableCollection</c> events.
    /// </summary>
    /// <typeparam name="T">Type of items.</typeparam>
    public class ObservableCollectionEventArgs<T> :
        EventArgs
    {
        #region private fields

        private readonly T item;
        private bool cancel;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of <c>ObservableCollectionEventArgs</c>.
        /// </summary>
        /// <param name="item">Item that is being added or removed from the collection.</param>
        public ObservableCollectionEventArgs(T item)
        {
            this.item = item;
            this.cancel = false;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Get the item that is being added or removed from the collection.
        /// </summary>
        public T Item
        {
            get { return this.item; }
        }

        /// <summary>
        /// Gets or sets if the operation must be canceled.
        /// </summary>
        /// <remarks>This property is used by the OnBeforeAdd and OnBeforeRemove events to cancel the add or remove operation.</remarks>
        public bool Cancel
        {
            get { return this.cancel; }
            set { this.cancel = value; }
        }

        #endregion
    }
}
