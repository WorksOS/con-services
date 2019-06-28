using System;
using VSS.Hydrology.WebApi.DXF.Tables;

namespace VSS.Hydrology.WebApi.DXF.Collections
{
    /// <summary>
    /// Represents the arguments thrown by the <see cref="DimensionStyleOverrideDictionary">DimensionStyleOverrideDictionary</see> events.
    /// </summary>
    public class DimensionStyleOverrideDictionaryEventArgs :
        EventArgs
    {
        #region private fields

        private readonly DimensionStyleOverride item;
        private bool cancel;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of <c>DimensionStyleOverrideDictionaryEventArgs</c>.
        /// </summary>
        /// <param name="item">Item that is being added or removed from the dictionary.</param>
        public DimensionStyleOverrideDictionaryEventArgs(DimensionStyleOverride item)
        {
            this.item = item;
            this.cancel = false;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Get the item that is being added to or removed from the dictionary.
        /// </summary>
        public DimensionStyleOverride Item
        {
            get { return this.item; }
        }

        /// <summary>
        /// Gets or sets if the operation must be canceled.
        /// </summary>
        /// <remarks>This property is used by the BeforeAddItem and BeforeRemoveItem events to cancel the add or remove operations.</remarks>
        public bool Cancel
        {
            get { return this.cancel; }
            set { this.cancel = value; }
        }

        #endregion
    }
}
