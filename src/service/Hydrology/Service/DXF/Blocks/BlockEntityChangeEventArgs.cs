using System;
using VSS.Hydrology.WebApi.DXF.Entities;

namespace VSS.Hydrology.WebApi.DXF.Blocks
{
    /// <summary>
    /// Represents the arguments thrown when an entity is added ore removed from a <see cref="Block">Block</see>.
    /// </summary>
    public class BlockEntityChangeEventArgs :
        EventArgs
    {
        #region private fields

        private readonly EntityObject item;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of <c>BlockEntityChangeEventArgs</c>.
        /// </summary>
        /// <param name="item">The entity that is being added or removed from the block.</param>
        public BlockEntityChangeEventArgs(EntityObject item)
        {
            this.item = item;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the entity that is being added or removed.
        /// </summary>
        public EntityObject Item
        {
            get { return this.item; }
        }

        #endregion
    }
}
