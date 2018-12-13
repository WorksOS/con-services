using Apache.Ignite.Core.Binary;

namespace VSS.TRex.Storage.Serializers
{
    public class TestBinarizable : IBinarizable
    {
        public void ReadBinary(IBinaryReader reader)
        {
        }

        public void WriteBinary(IBinaryWriter writer)
        {
        }
    }

    public class TestSerializer : IBinarySerializer
    {
        public void ReadBinary(object obj, IBinaryReader reader)
        {
        }

        public void WriteBinary(object obj, IBinaryWriter writer)
        {
        }
    }
}
