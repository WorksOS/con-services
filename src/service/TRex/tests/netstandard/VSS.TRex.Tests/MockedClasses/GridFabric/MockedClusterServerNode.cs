using Apache.Ignite.Core.Cluster;
using System;
using System.Collections.Generic;
using VSS.TRex.GridFabric.Models.Servers;

namespace VSS.TRex.Tests.MockedClasses.GridFabric
{
    public class MockedClusterServerNode : IClusterNode
    {
        private Guid _id = Guid.NewGuid();
        private Dictionary<string, object> _attributes = null;
        private bool _IsClient = false;

        public Guid Id => _id;

        public ICollection<string> Addresses => new List<string>(); //throw new NotImplementedException();

        public ICollection<string> HostNames => new List<string>(); //throw new NotImplementedException();

        public long Order => 0; ///throw new NotImplementedException();

        public bool IsLocal => false; //throw new NotImplementedException

        public bool IsDaemon => false; //throw new NotImplementedException();

        public bool IsClient => _IsClient; //throw new NotImplementedException();

        public object ConsistentId => GetHashCode();  //throw new NotImplementedException();

        public IDictionary<string, object> Attributes => _attributes;

        public T GetAttribute<T>(string name)
        {
            _attributes.TryGetValue(name, out object Value);

            return (T)Value;
        }

        public IDictionary<string, object> GetAttributes() => _attributes;

        public IClusterMetrics GetMetrics() => null; // throw new NotImplementedException();

        public bool TryGetAttribute<T>(string name, out T attr)
        {
            bool result = _attributes.TryGetValue(name, out object Value);
            attr = (T)Value;

            return result;
        }

        public MockedClusterServerNode()
        {
        }

        public MockedClusterServerNode(bool isClient, string role)
        {
            _IsClient = isClient;
            _attributes = new Dictionary<string, object>()
            {
                { $"{ServerRoles.ROLE_ATTRIBUTE_NAME}-{role}", "True" }
            };
        }
    }

}
