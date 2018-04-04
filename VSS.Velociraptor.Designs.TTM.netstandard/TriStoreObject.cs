namespace VSS.Velociraptor.Designs.TTM
{
    /// <summary>
    /// The base class all classes in the TTM implementation descend from
    /// </summary>
    public class TriStoreObject
    {
        /// <summary>
        /// Base initialisation logic for the object
        /// </summary>
        protected virtual void Initialise()
        {
            // base class does no initialisation
        }

        /// <summary>
        /// A 'tag' used for various purposes in TTM processing
        /// </summary>
        public int Tag { get; set; }

        /// <summary>
        /// Default no-arg constructor
        /// </summary>
        public TriStoreObject()
        {
            Initialise();
        }
    }
}
