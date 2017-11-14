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
using TestClassLibrary;

namespace Apache_Ignite_DotNet_Test
{
    [Serializable]
    class HelloAction : IComputeAction
    {
        public void Invoke()
        {
            Console.WriteLine("Hello World!");

            TestClass tc = new TestClass();
            tc.DoSomething();
        }
    }

    [Serializable]
    class HelloComputeFunc : Apache.Ignite.Core.Compute.IComputeFunc<String, String>
    {
        public String Invoke(String s)
        {
            Console.WriteLine("Hello World! - " + s);

            TestClass tc = new TestClass();

            return s;
        }
    }

    [Serializable]
    class AffinityComputeFunc : Apache.Ignite.Core.Compute.IComputeFunc<String>
    {
        private String s = null;

        public String Invoke()
        {
            IIgnite ignite = Ignition.TryGetIgnite("Raptor");

            if (ignite != null)
            {
                ICache<String, MyCacheClass> cache = ignite.GetCache<String, MyCacheClass>("TestCache");
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

        public AffinityComputeFunc(String _s)
        {
            s = _s;
        }
    }

    [Serializable]
    class MyComputeJob : IComputeJob<String>
    {
        private String _arg = null;

        public void Cancel()
        {
            // Do nothing
        }

        public String Execute()
        {
            return _arg + ":" + (new Random()).NextDouble().ToString();
        }

        public MyComputeJob(String arg)
        {
            _arg = arg;
        }
    }

    [Serializable]
    [ComputeTaskNoResultCache]
    class MyComputeTask : IComputeTask<String, String, String>
    {
        // <in TArg, TJobRes, out TRes>
        //IDictionary<IComputeJob<TJobRes>, IClusterNode> Map(IList<IClusterNode> subgrid, TArg arg);
        //ComputeJobResultPolicy OnResult(IComputeJobResult<TJobRes> res, IList<IComputeJobResult<TJobRes>> rcvd);   
        // TRes Reduce(IList<IComputeJobResult<TJobRes>> results);

        String result = String.Empty;

        public IDictionary<IComputeJob<String>, IClusterNode> Map(IList<IClusterNode> subgrid, String arg)
        {
            var map = new Dictionary<IComputeJob<String>, IClusterNode>();

            foreach (var s in subgrid)
            {
                map.Add(new MyComputeJob(arg), s);
            }

            return map;
        }

        public ComputeJobResultPolicy OnResult(IComputeJobResult<String> res, IList<IComputeJobResult<String>> rcvd)
        {
            result += res.Data;

            return ComputeJobResultPolicy.Wait;
        }

        public String Reduce(IList<IComputeJobResult<String>> results)
        {
            // Aggregate all the response strings (converting from the IComputeJobResult wrapper first)

            return (results?.Any() == true) ? results.Select(x => x.Data).Aggregate((s1, s2) => s1 + " " + s2) : result;
        }
    }

    [Serializable]
    public class MyCacheClass
    {
        [AffinityKeyMapped]
        public String name = String.Empty;

        public byte[] localData = null;

        public MyCacheClass(String _name)
        {
            name = _name;
            localData = new byte[30000];
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            String s;
            DateTime startTime;
            DateTime endTime;

            IgniteConfiguration cfg = new IgniteConfiguration()
            {
                IgniteInstanceName = "Raptor",

                // Register custom class for Ignite serialization
                BinaryConfiguration = new Apache.Ignite.Core.Binary.BinaryConfiguration(typeof(MyCacheClass))                                    
            };

            IIgnite ignite = Ignition.Start(cfg);
         
            // Add a cache to Ignite
            ICache<String, MyCacheClass> cache = ignite.CreateCache<String, MyCacheClass>
                (new CacheConfiguration()
                {
                    Name = "TestCache",
                    CopyOnRead = false,
                    KeepBinaryInStore = false,
                    CacheStoreFactory = new RaptorCacheStoreFactory(),
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

            var binCache = cache.WithKeepBinary<String, IBinaryObject>();
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

            // Broadcast a no-arg/no-return action
            compute.Broadcast(new HelloAction());

            // Broadcast an action taking a string and returning a string
            ICollection<String> result = compute.Broadcast<String, String>(new HelloComputeFunc(), "Come back!");
            foreach (String ss in result)
            {
                Console.WriteLine("Result: " + ss);
            }

            // Broadcast an action taking a string and returning a string to all nodes other than this node
            if (compute2.ForServers()?.ForRemotes()?.GetNodes()?.Any() == true)
            {
                try
                {
                    ICollection<String> result2 = compute2.ForRemotes().GetCompute().Broadcast<String, String>(new HelloComputeFunc(), "Come back (2)!");
                    foreach (String ss in result2)
                    {
                        Console.WriteLine("Result: " + ss);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception calling cluster remote nodes\r\n: {0}", e);
                }
            }
            else
            {
                Console.WriteLine("Calling cluster remote broadcast function: No remote servers present in cluster");
            }

            // Execute a map reduce on the cluster
            if (compute2.ForServers()?.GetNodes()?.Any() == true)
            {
                try
                {
                    String mapReduceResult = ignite.GetCompute().Execute<String, String, String>(new MyComputeTask(), "Bob");
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
                String affinityResult = ignite.GetCompute().AffinityCall<String>("TestCache", "First", new AffinityComputeFunc("First"));
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
