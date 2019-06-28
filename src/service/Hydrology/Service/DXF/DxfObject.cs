namespace VSS.Hydrology.WebApi.DXF
{
    /// <summary>
    /// Represents the base class for all DXF objects.
    /// </summary>
    public abstract class DxfObject
    {
        #region private fields

        private string codename;
        private string handle;
        private DxfObject owner;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>DxfObject</c> class.
        /// </summary>
        /// <param name="codename"><see cref="DxfObjectCode">DXF object name</see>.</param>
        protected DxfObject(string codename)
        {
            this.codename = codename;
            this.handle = null;
            this.owner = null;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the <see cref="DxfObjectCode">DXF object name</see>.
        /// </summary>
        public string CodeName
        {
            get { return this.codename; }
            protected set { this.codename = value; }
        }

        /// <summary>
        /// Gets the handle assigned to the DXF object.
        /// </summary>
        /// <remarks>
        /// The handle is a unique hexadecimal number assigned automatically to every dxf object,
        /// that has been added to a <see cref="DxfDocument">DxfDocument</see>.
        /// </remarks>
        public string Handle
        {
            get { return this.handle; }
            internal set { this.handle = value; }
        }

        /// <summary>
        /// Gets the owner of the actual <see cref="DxfObject">DxfObject</see>.
        /// </summary>
        public DxfObject Owner
        {
            get { return this.owner; }
            internal set { this.owner = value; }
        }

        #endregion

        #region internal methods

        /// <summary>
        /// Assigns a handle to the object based in a integer counter.
        /// </summary>
        /// <param name="entityNumber">Number to assign to the actual object.</param>
        /// <returns>Next available entity number.</returns>
        /// <remarks>
        /// Some objects might consume more than one, is, for example, the case of polylines that will assign
        /// automatically a handle to its vertexes. The entity number will be converted to an hexadecimal number.
        /// </remarks>
        internal virtual long AsignHandle(long entityNumber)
        {
            this.handle = entityNumber.ToString("X");
            return entityNumber + 1;
        }

        #endregion

        #region overrides

        /// <summary>
        /// Obtains a string that represents the DXF object.
        /// </summary>
        /// <returns>A string text.</returns>
        public override string ToString()
        {
            return this.codename;
        }

        #endregion
    }
}
