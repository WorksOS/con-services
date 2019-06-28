using System;
using VSS.Hydrology.WebApi.DXF.Entities;

namespace VSS.Hydrology.WebApi.DXF.Blocks
{
    /// <summary>
    /// Represents the arguments thrown when an attribute definition is added ore removed from a <see cref="Block">Block</see>.
    /// </summary>
    public class BlockAttributeDefinitionChangeEventArgs :
        EventArgs
    {
        #region private fields

        private readonly AttributeDefinition item;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of <c>BlockAttributeDefinitionChangeEventArgs</c>.
        /// </summary>
        /// <param name="item">The attribute definition that is being added or removed from the block.</param>
        public BlockAttributeDefinitionChangeEventArgs(AttributeDefinition item)
        {
            this.item = item;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the attribute definition that is being added or removed.
        /// </summary>
        public AttributeDefinition Item
        {
            get { return this.item; }
        }

        #endregion
    }
}
