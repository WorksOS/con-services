﻿using Apache.Ignite.Core.Cluster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.VisionLink.Raptor.Servers;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests.MockedClasses.GridFabric
{
    public class MockedClusterServerNode : IClusterNode
    {
        private Guid _id = Guid.NewGuid();
        private Dictionary<string, object> _attributes = null;

        public Guid Id => _id;

        public ICollection<string> Addresses => new List<string>(); //throw new NotImplementedException();

        public ICollection<string> HostNames => new List<string>(); //throw new NotImplementedException();

        public long Order => 0; ///throw new NotImplementedException();

        public bool IsLocal => false; //throw new NotImplementedException();

        public bool IsDaemon => false; //throw new NotImplementedException();

        public bool IsClient => false; //throw new NotImplementedException();

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

        public MockedClusterServerNode(string role)
        {
            _attributes = new Dictionary<string, object>()
            {
                { ServerRoles.ROLE_ATTRIBUTE_NAME, role }
            };
        }
    }

}
