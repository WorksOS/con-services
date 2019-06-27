using VSS.TRex.SubGridTrees.Server;
using VSS.TRex.Tests.TestFixtures;
using Xunit;

namespace VSS.TRex.Tests.SubGridTrees
{
        public class Test_SubGridCellLatestPassDataWrapper : IClassFixture<DILoggingFixture>
    {
        [Fact()]
        public void Test_SubGridCellLatestPassDataWrapper_NonStatic_Creation()
        {
            SubGridCellLatestPassDataWrapper_NonStatic wrapper = new SubGridCellLatestPassDataWrapper_NonStatic();

            Assert.True(wrapper.PassDataExistenceMap != null && wrapper.PassDataExistenceMap.IsEmpty(), "Instance not created as expected");
        }

        [Fact()]
        public void Test_SubGridCellLatestPassDataWrapper_NonStatic_Clear()
        {
            SubGridCellLatestPassDataWrapper_NonStatic wrapper = new SubGridCellLatestPassDataWrapper_NonStatic();

            Assert.True(wrapper.PassDataExistenceMap.IsEmpty(), "Existence map not empty after creation");

            wrapper.PassDataExistenceMap.SetBitValue(0, 0, true);
        }
    }
}
