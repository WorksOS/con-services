using System.IO;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.Types;

namespace VSS.VisionLink.Raptor.SubGridTrees.Client
{
    public class SubGridTreeLeafSubGridBaseResult
    {
        private static IClientLeafSubgridFactory ClientLeafSubGridFactory = ClientLeafSubgridFactoryFactory.GetClientLeafSubGridFactory();

        public IClientLeafSubGrid SubGrid;
        public int SubgridResultCode;

        public void Write(BinaryWriter writer)
        {
            writer.Write(SubGrid != null);

            if (SubGrid != null)
            {
                SubGrid.Write(writer, new byte[10000]);

                writer.Write(SubgridResultCode);
            }
        }

        public void ReadFromStream(BinaryReader reader,
                                   GridDataType GridDataType)
        {
            if (reader.ReadBoolean())
            {
                SubGrid = ClientLeafSubGridFactory.GetSubGrid(GridDataType);

                SubGrid.Read(reader, new byte[10000]);

                SubgridResultCode = reader.ReadInt32();
            }
            else
            {
                SubGrid = null;
            }
        }
    }
}
