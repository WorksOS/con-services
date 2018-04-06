using System;
using System.IO;

namespace VSS.VisionLink.Raptor.GridFabric.Caches
{
    /// <summary>
    /// Implements the Ignite ICacheStore interface
    /// </summary>
    [Serializable]
    public class RaptorCacheStoreUtilities 
    {
        /// <summary>
        /// The location to store the files containing the persisted cache entries
        /// </summary>
        protected string path = "C:\\Temp\\RaptorIgniteData";

        /// <summary>
        /// Default constructor that adds the mutability suffix to the path to store persisted cache items
        /// </summary>
        public RaptorCacheStoreUtilities(string MutabilitySuffix)
        {
            path = path + MutabilitySuffix;

            Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Deletes an item in the cache identified by the cache key
        /// </summary>
        /// <param name="key"></param>
        public void Delete(string key)
        {
            // Remove the file representing this element from the file system
            try
            {
                File.Delete(Path.Combine(path, key));
            }
            catch (FileNotFoundException)
            {
                // This is fine, carry on
            }
        }

        /// <summary>
        /// Loads an item from the cache identified by the cache key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public MemoryStream Load(string key)
        {
            MemoryStream MS = null;
            bool ok = false;
            while (!ok)
            {
                string fileName = Path.Combine(path, key);
                try
                {
                    MS = new MemoryStream();
                    using (FileStream fs = new FileStream(fileName, FileMode.Open))
                    {
                        fs.CopyTo(MS);
                    }

                    ok = true;
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("Cache Get() for key {0}, filename {1} resulted in file not found, returning null", key, fileName);
                    return null;
                }
                catch (Exception)
                {
                    Console.WriteLine("Cache Get() blocked on key {0}, waiting 1 second.", key);
                    System.Threading.Thread.Sleep(1000);
                }
            }

            return MS;
        }

        /// <summary>
        /// Writes a cache item identitied by the cache key to the persisted store
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void Write(string key, MemoryStream val)
        {
            using (FileStream fs = new FileStream(Path.Combine(path, key), FileMode.Create))
            {
                try
                {
                    val.Position = 0;
                    val.WriteTo(fs);
                }
                finally
                {
                    fs.Close();
                }
            }
        }
    }
}
