using Apache.Ignite.Core.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.VisionLink.Raptor.Storage.Serializers
{
    public class TestBinarizable : IBinarizable
    {
        public void ReadBinary(IBinaryReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteBinary(IBinaryWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class TestSerializer : IBinarySerializer
    {
        public void ReadBinary(object obj, IBinaryReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteBinary(object obj, IBinaryWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
