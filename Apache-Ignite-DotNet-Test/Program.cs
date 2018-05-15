using Apache.Ignite.Core;
using Apache.Ignite.Core.Binary;
using Apache.Ignite.Core.Cache;
using Apache.Ignite.Core.Cache.Affinity;
using Apache.Ignite.Core.Cache.Configuration;
using Apache.Ignite.Core.Cache.Eviction;
using Apache.Ignite.Core.Cache.Store;
using Apache.Ignite.Core.Cluster;
using Apache.Ignite.Core.Common;
using Apache.Ignite.Core.Compute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apache_Ignite_DotNet_Test
{




    [Serializable]
    class AffinityComputeFunc : Apache.Ignite.Core.Compute.IComputeFunc<string>
    {
        private string s = null;

        public string Invoke()
        {
            IIgnite ignite = Ignition.TryGetIgnite("TRex");

            if (ignite != null)
            {
                ICache<string, MyCacheClass> cache = ignite.GetCache<string, MyCacheClass>("TestCache");
                MyCacheClass c = cache.Get(s);
                return "Affinity: " + c.name;
            }
            else
            {
                Console.WriteLine("Unable to get Ignite instance with Ignition.TryGetIgnite()");
                return "Affinity: <Error: No Ignite>";
            }

            //return "Affinity: " + Ignition.TryGetIgnite().GetCache<String, MyCacheClass>("TestCache").Get(s).name;
        }

        public AffinityComputeFunc(string _s)
        {
            s = _s;
        }
    }

    [Serializable]
    class MyComputeJob : IComputeJob<string>
    {
        private string _arg = null;

        public void Cancel()
        {
            // Do nothing
        }

        public string Execute()
        {
            return _arg + ":" + (new Random()).NextDouble().ToString();
        }

        public MyComputeJob(string arg)
        {
            _arg = arg;
        }
    }

    [Serializable]
    [ComputeTaskNoResultCache]
    class MyComputeTask : IComputeTask<string, string, string>
    {
        // <in TArg, TJobRes, out TRes>
        //IDictionary<IComputeJob<TJobRes>, IClusterNode> Map(IList<IClusterNode> subgrid, TArg arg);
        //ComputeJobResultPolicy OnResult(IComputeJobResult<TJobRes> res, IList<IComputeJobResult<TJobRes>> rcvd);   
        // TRes Reduce(IList<IComputeJobResult<TJobRes>> results);

        string result = string.Empty;

        public IDictionary<IComputeJob<string>, IClusterNode> Map(IList<IClusterNode> subgrid, string arg)
        {
            var map = new Dictionary<IComputeJob<string>, IClusterNode>();

            foreach (var s in subgrid)
            {
                map.Add(new MyComputeJob(arg), s);
            }

            return map;
        }

        public ComputeJobResultPolicy OnResult(IComputeJobResult<string> res, IList<IComputeJobResult<string>> rcvd)
        {
            result += res.Data;

            return ComputeJobResultPolicy.Wait;
        }

        public string Reduce(IList<IComputeJobResult<string>> results)
        {
            // Aggregate all the response strings (converting from the IComputeJobResult wrapper first)

            return (results?.Any() == true) ? results.Select(x => x.Data).Aggregate((s1, s2) => s1 + " " + s2) : result;
        }
    }

    [Serializable]
    public class MyCacheClass
    {
        [AffinityKeyMapped]
        public string name = string.Empty;

        public byte[] localData = null;

        public MyCacheClass(string _name)
        {
            name = _name;
            localData = new byte[30000];
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string s;
            DateTime startTime;
            DateTime endTime;

            IgniteConfiguration cfg = new IgniteConfiguration()
            {
                IgniteInstanceName = "TRex",

                // Register custom class for Ignite serialization
                BinaryConfiguration = new Apache.Ignite.Core.Binary.BinaryConfiguration(typeof(MyCacheClass))                                    
            };

            IIgnite ignite = Ignition.Start(cfg);
         
            // Add a cache to Ignite
            ICache<string, MyCacheClass> cache = ignite.CreateCache<string, MyCacheClass>
                (new CacheConfiguration()
                {
                    Name = "TestCache",
                    CopyOnRead = false,
                    KeepBinaryInStore = false,
                    CacheStoreFactory = new TRexCacheStoreFactory(),
                    ReadThrough = true,
                    WriteThrough = true,
                    WriteBehindFlushFrequency = new TimeSpan(0, 0, 5), // 5 seconds 
                    EvictionPolicy = new LruEvictionPolicy()
                        {
                            MaxMemorySize = 1000000,
                        }
                });

            int NumCacheEntries = 5000;

            // Add a cache item
            cache.Put("First", new MyCacheClass("FirstItem"));

            // Add a collectikon of items
            startTime = DateTime.Now;
            for (int i = 0; i < NumCacheEntries; i++)
            {
                cache.Put("First"+i, new MyCacheClass("First"+i));
            }
            endTime = DateTime.Now;

            s = string.Format("{0}", endTime - startTime);
            Console.WriteLine("Time to add cache items with serialisation: {0}", s);

            int sumsum = 0;

            // Query back the cache items with serialisation
            startTime = DateTime.Now;
            for (int i = 0; i < NumCacheEntries; i++)
            {
                MyCacheClass first = cache.Get("First"+i);

                int sum = 0;
                for (int ii = 0; ii < first.localData.Length; ii++)
                {
                    sum += first.localData[ii];
                }
                sumsum += sum;
            }
            endTime = DateTime.Now;

            s = string.Format("{0}", endTime - startTime);
            Console.WriteLine("Time to query cache items with serialisation: {0}, sum = {1}", s, sumsum);

            var binCache = cache.WithKeepBinary<string, IBinaryObject>();
            //            IBinaryObject binCacheItem = binCache["First"];
            //            Console.WriteLine(binCacheItem.GetField<string>("Name"));

            // Query back the cache items without serialisation (using BinaryObject)
            startTime = DateTime.Now;
            for (int i = 0; i < NumCacheEntries; i++)
            {
                IBinaryObject binCacheItem = binCache["First"+i];

                byte[] bytes = binCacheItem.GetField<byte[]>("localData");

                int sum = 0;
                for (int ii = 0; ii < bytes.Length; ii++)
                {
                    sum += bytes[ii];
                }
                sumsum += sum;
            }
            endTime = DateTime.Now;

            s = string.Format("{0}", endTime - startTime);
            Console.WriteLine("Time to query cache items without serialisation: {0}, sum = {1}", s, sumsum);

            // Get compute instance over all nodes in the cluster.
            ICompute compute = ignite.GetCompute();
            IClusterGroup compute2 = ignite.GetCompute().ClusterGroup;




           // Execute a map reduce on the cluster
            if (compute2.ForServers()?.GetNodes()?.Any() == true)
            {
                try
                {
                    string mapReduceResult = ignite.GetCompute().Execute<string, string, string>(new MyComputeTask(), "Bob");
                    Console.WriteLine("Mapreduce result = '{0}'", mapReduceResult);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception executing mapReduce execution\r\n: {0}", e);
                }
            }
            else
            {
                Console.WriteLine("Calling cluster mapReduce broadcast function: No servers present in cluster");
            }

            // Execute a command using affinity on the cluster
            try
            {
                string affinityResult = ignite.GetCompute().AffinityCall<string>("TestCache", "First", new AffinityComputeFunc("First"));
                Console.WriteLine("Affinity result = '{0}'", affinityResult);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception executing affinity execution\r\n: {0}", e);
            }

            if (ignite == null)
            {
                Console.WriteLine("Ignite instance is null at end of method");
            }
            Console.ReadKey();
        }
    }
}
