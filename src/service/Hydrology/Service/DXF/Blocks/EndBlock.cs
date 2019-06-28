namespace VSS.Hydrology.WebApi.DXF.Blocks
{
    /// <summary>
    /// Represents the termination element of the block definition.
    /// </summary>
    internal class EndBlock :
        DxfObject
    {
        /// <summary>
        /// Initializes a new instance of the <c>BlockEnd</c> class.
        /// </summary>
        public EndBlock(DxfObject owner)
            : base(DxfObjectCode.BlockEnd)
        {
            this.Owner = owner;
        }
    }
}
