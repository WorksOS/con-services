namespace VSS.VisionLink.Raptor.ExistenceMaps.GridFabric.Requests
{
    /// <summary>
    /// Base class for existence maps requests. Defines existnace map type descriptors and related base functionality sucj as cachey key calculation
    /// </summary>
    public class BaseExistenceMapRequest
    {
        /// <summary>
        /// Constrct a unique key for the existance map comprised of a type descriptor and an ID
        /// </summary>
        /// <param name="siteModelID"></param>
        /// <param name="typeDescriptor"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static string CacheKey(long siteModelID, long typeDescriptor, long ID) => $"Model:{siteModelID}-Descriptor:{typeDescriptor}-ID:{ID}";
    }
}
