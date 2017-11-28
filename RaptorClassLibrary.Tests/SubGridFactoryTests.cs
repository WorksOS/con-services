using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.VisionLink.Raptor.SubGridTrees.Interfaces;
using VSS.VisionLink.Raptor.SubGridTrees;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.SubGridTrees.Client;

namespace VSS.VisionLink.Raptor.RaptorClassLibrary.Tests
{
    [TestClass]
    public class SubGridFactoryTests
    {
        [TestMethod]
        public void Test_SubGridFactory_Creation()
        {
            ISubGridFactory factory = new SubGridFactory<NodeSubGrid, LeafSubGrid>();

            Assert.IsTrue(factory != null, "Factory failed to construct");

            // Create a tree for the factory to create subgrids for
            ISubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, factory);

            Assert.IsTrue(tree != null, "Sub grid tree failed to construct");
        }

        [TestMethod]
        public void Test_SubGridFactory_Create_NodeAndLeafSubgrids()
        {
            ISubGridFactory factory = new SubGridFactory<NodeSubGrid, LeafSubGrid>();

            // Create a tree for the factory to create subgrids for
            ISubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, factory);

            // Create subgrids for each layer, ensure layers
            // Ask the factory to create node for an invalid tree level
            try
            {
                ISubGrid invalid = factory.GetSubGrid(tree, 0);
                Assert.Fail("Factory created subgrid for invalid tree level.");
            }
            catch (ArgumentException)
            {
                // As expected
            }

            // Ask the factory to create node and leaf subgrids for a 6 level tree, from root to leaf.
            ISubGrid root = factory.GetSubGrid(tree, 1);
            Assert.IsFalse(root == null, "Factory did not create subgrid for root tree level.");
            Assert.IsTrue(root is NodeSubGrid, "Factory did not create node subgrid for root tree level.");

            ISubGrid level2 = factory.GetSubGrid(tree, 2);
            Assert.IsFalse(level2 == null, "Factory did not create subgrid for tree level 2.");
            Assert.IsTrue(level2 is NodeSubGrid, "Factory did not create node subgrid for tree level 2.");

            ISubGrid level3 = factory.GetSubGrid(tree, 3);
            Assert.IsFalse(level3 == null, "Factory did not create subgrid for tree level 3.");
            Assert.IsTrue(level3 is NodeSubGrid, "Factory did not create node subgrid for tree level 3.");

            ISubGrid level4 = factory.GetSubGrid(tree, 4);
            Assert.IsFalse(level4 == null, "Factory did not create subgrid for tree level 4.");
            Assert.IsTrue(level4 is NodeSubGrid, "Factory did not create node subgrid for tree level 4.");

            ISubGrid level5 = factory.GetSubGrid(tree, 5);
            Assert.IsFalse(level5 == null, "Factory did not create subgrid for tree level 5.");
            Assert.IsTrue(level5 is NodeSubGrid, "Factory did not create node subgrid for tree level 5.");

            ISubGrid leaf = factory.GetSubGrid(tree, 6);
            Assert.IsFalse(leaf == null, "Factory did not create subgrid for tree level 6.");
            Assert.IsTrue(leaf is LeafSubGrid, "Factory did not create node subgrid for tree level 6.");
        }

        [TestMethod]
        public void Test_SubGridClientLeafFactory_Creation()
        {
            IClientLeafSubgridFactory factory = ClientLeafSubgridFactoryFactory.GetClientLeafSubGridFactory();

            Assert.IsTrue(factory != null, "Factory failed to construct");

            IClientLeafSubGrid HeightLeaf = factory.GetSubGrid(Types.GridDataType.Height);

            Assert.IsTrue(factory != null, "Factory failed to construct height leaf subgrid");

            IClientLeafSubGrid HeightAndTimeLeaf = factory.GetSubGrid(Types.GridDataType.HeightAndTime);

            Assert.IsTrue(factory != null, "Factory failed to construct height and time leaf subgrid");
        }

        [TestMethod]
        public void Test_SubGridClientLeafFactory_Recycling()
        {
            IClientLeafSubgridFactory factory = ClientLeafSubgridFactoryFactory.GetClientLeafSubGridFactory();

            Assert.IsTrue(factory != null, "Factory failed to construct");

            IClientLeafSubGrid HeightLeaf = factory.GetSubGrid(Types.GridDataType.Height);
            factory.ReturnClientSubGrid(ref HeightLeaf);

            Assert.IsTrue(HeightLeaf == null, "Leaf height reference not set to null");

            IClientLeafSubGrid HeightAndTimeLeaf = factory.GetSubGrid(Types.GridDataType.HeightAndTime);
            factory.ReturnClientSubGrid(ref HeightAndTimeLeaf);

            Assert.IsTrue(HeightAndTimeLeaf == null, "Leaf height and time reference not set to null");
        }

        [TestMethod]
        public void Test_SubGridClientLeafFactory_Reuse()
        {
            IClientLeafSubgridFactory factory = ClientLeafSubgridFactoryFactory.GetClientLeafSubGridFactory();

            Assert.IsTrue(factory != null, "Factory failed to construct");

            IClientLeafSubGrid HeightLeaf = factory.GetSubGrid(Types.GridDataType.Height);
            factory.ReturnClientSubGrid(ref HeightLeaf);

            IClientLeafSubGrid HeightAndTimeLeaf = factory.GetSubGrid(Types.GridDataType.HeightAndTime);
            factory.ReturnClientSubGrid(ref HeightAndTimeLeaf);

            IClientLeafSubGrid HeightLeaf2 = factory.GetSubGrid(Types.GridDataType.Height);
            Assert.IsTrue(HeightLeaf2 != null, "Reused subgrid is null");
            Assert.IsTrue(HeightLeaf2.GridDataType == Types.GridDataType.Height, "Reused subgrid is not height type");

            IClientLeafSubGrid HeightAndTimeLeaf2 = factory.GetSubGrid(Types.GridDataType.HeightAndTime);
            Assert.IsTrue(HeightAndTimeLeaf2 != null, "Reused subgrid is null");
            Assert.IsTrue(HeightAndTimeLeaf2.GridDataType == Types.GridDataType.HeightAndTime, "Reused subgrid is not height and time type");
        }
    }
}
