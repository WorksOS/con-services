using System;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.SubGridTrees.Types;
using VSS.TRex;
using Xunit;
using VSS.TRex.SubGridTrees.Factories;

namespace VSS.TRex.Tests.SubGridTrees
{
        public class SubGridTests
    {
        [Fact]
        public void Test_SubGrid_Creation()
        {
            ISubGrid subgrid = null;

            // Try creating a new base subgrid instance directly, supplying 
            subgrid = new SubGrid(new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>()), null, SubGridTree.SubGridTreeLevels);
            Assert.NotNull(subgrid);
        }

        [Fact]
        public void Test_SubGrid_LeafSubgridProperties()
        {
            ISubGrid leafSubgrid = null;
            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            // Create a new base subgrid leaf instance directly
            leafSubgrid = new SubGrid(tree, null, SubGridTree.SubGridTreeLevels);

            Assert.True(leafSubgrid.IsLeafSubGrid());

            Assert.False(leafSubgrid.Dirty);
            Assert.Equal(leafSubgrid.Level, SubGridTree.SubGridTreeLevels);
            Assert.Equal(leafSubgrid.AxialCellCoverageByThisSubgrid(), SubGridTree.SubGridTreeDimension);

            Assert.Equal((uint)0, leafSubgrid.OriginX);
            Assert.Equal((uint)0, leafSubgrid.OriginY);
            Assert.Equal("0:0", leafSubgrid.Moniker());

            // Does the dirty flag change?
            leafSubgrid.Dirty = true;
            Assert.True(leafSubgrid.Dirty, "Leaf subgrid is not marked as dirty after setting it to dirty");
        }

        [Fact]
        public void Test_SubGrid_NodeSubgridProperties()
        {
            ISubGrid nodeSubgrid = null;
            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            // Create a new base subgrid node instance directly
            nodeSubgrid = new SubGrid(tree, null, SubGridTree.SubGridTreeLevels - 1);
            Assert.False(nodeSubgrid.IsLeafSubGrid());

            Assert.False(nodeSubgrid.Dirty);
            Assert.Equal(nodeSubgrid.Level, SubGridTree.SubGridTreeLevels - 1);

            // A subgrid one level above a leaf subgrid covers sqr(SubGridTree.SubGridTreeDimension) cells in each dimension (X & Y)
            Assert.Equal((int)nodeSubgrid.AxialCellCoverageByThisSubgrid(), SubGridTree.SubGridTreeDimension * SubGridTree.SubGridTreeDimension);

            // A child subgrid of this parent shoudl ahve an axial coverage of SubGridTree.SubGridTreeDimension cells in each dimension (X & Y)
            // (as there are SubGridTree.SubGridTreeDimension children cells in the X and Y dimensions
            Assert.Equal(nodeSubgrid.AxialCellCoverageByChildSubgrid(), SubGridTree.SubGridTreeDimension);
        }

        [Fact]
        public void Test_SubGrid_ParentAssignment()
        {
            ISubGrid parentSubgrid = null;
            ISubGrid leafSubgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            leafSubgrid = new SubGrid(tree, null, SubGridTree.SubGridTreeLevels);
            parentSubgrid = new SubGrid(tree, null, SubGridTree.SubGridTreeLevels - 1);

            leafSubgrid.Parent = parentSubgrid;
            leafSubgrid.SetOriginPosition(10, 10);

            Assert.Equal((int)leafSubgrid.OriginX, 10 * SubGridTree.SubGridTreeDimension);
            Assert.Equal((int)leafSubgrid.OriginY, 10 * SubGridTree.SubGridTreeDimension);
            Assert.Equal(leafSubgrid.Moniker(), string.Format("{0}:{0}", 10 * SubGridTree.SubGridTreeDimension));
        }

        [Fact]
        public void Test_SubGrid_Invalid_GetSubgrid()
        {
            ISubGrid subgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            subgrid = new SubGrid(tree, null, SubGridTree.SubGridTreeLevels);

            // Check a call to the base GetSubGrid subgrid yields an exception
            try
            {
                ISubGrid gotSubgrid = subgrid.GetSubGrid(0, 0);
                Assert.True(false,"Base SubGrid class GetSubGrid() did not throw an exception");
            }
            catch (Exception)
            {
                // Good!
            }
        }

        [Fact]
        public void Test_SubGrid_Invalid_SetSubgrid()
        {
            ISubGrid subgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            subgrid = new SubGrid(tree, null, SubGridTree.SubGridTreeLevels);

            // Check a call to the base SetSubGrid subgrid yields an exception
            try
            {
                subgrid.SetSubGrid(0, 0, null);
                Assert.True(false,"Base SubGrid class SetSubGrid() did not throw an exception");
            }
            catch (Exception)
            {
                // Good!
            }
        }

        [Fact]
        public void Test_SubGrid_Invalid_Clear()
        {
            ISubGrid subgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            subgrid = new SubGrid(tree, null, SubGridTree.SubGridTreeLevels);

            // Check a call to the base SetSubGrid subgrid yields an exception
            try
            {
                subgrid.Clear();
                Assert.True(false,"Base SubGrid class Clear() did not throw an exception");
            }
            catch (Exception)
            {
                // Good!
            }
        }

        [Fact]
        public void Test_SubGrid_Invalid_CellHasValue()
        {
            ISubGrid subgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            subgrid = new SubGrid(tree, null, SubGridTree.SubGridTreeLevels);

            // Check a call to the base SetSubGrid subgrid yields an exception
            try
            {
                if (subgrid.CellHasValue(0, 0))
                { }

                Assert.True(false,"Base SubGrid class CellHasValue() did not throw an exception");
            }
            catch (Exception)
            {
                // Good!
            }
        }

        [Fact]
        public void Test_SubGrid_GetWorldOrigin()
        {
            ISubGrid leafSubgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            leafSubgrid = new SubGrid(tree, null, SubGridTree.SubGridTreeLevels);

            double WorldOriginX, WorldOriginY;
            leafSubgrid.CalculateWorldOrigin(out WorldOriginX, out WorldOriginY);

            // World origin of leaf subgrid is the extreme origin of the overmapped world coordinate system (cell coordinate system * cell size)
            // as the cell origin position is 0, 0 in the cell address space for a newly created subgrid
            // The leaf So, both X and Y origin values 
            Assert.Equal(WorldOriginX, WorldOriginY);
            Assert.Equal(WorldOriginX, (-tree.IndexOriginOffset * tree.CellSize));
        }

        [Fact]
        public void Test_SubGrid_GetSubGridCellIdex()
        {
            ISubGrid leafSubgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            leafSubgrid = new SubGrid(tree, null, SubGridTree.SubGridTreeLevels);

            // GetSubGridCellIndex is a subgrid relative operation only, and depends only on the Owner to derive the difference
            // between the numer of levels in the overall tree, and the level in the tree at which this subgrid resides (in this
            // case the bottom of the tree (level 6) to compute the subgrid relative X and y cell indices as it is a leaf subgrid.

            byte SubGridCellX, SubGridCellY;
            leafSubgrid.GetSubGridCellIndex(0, 0, out SubGridCellX, out SubGridCellY);
            Assert.True(SubGridCellX == 0 && SubGridCellY == 0, "Subgrid cell indices incorrect");

            leafSubgrid.GetSubGridCellIndex(SubGridTree.SubGridTreeDimensionMinus1, SubGridTree.SubGridTreeDimensionMinus1, out SubGridCellX, out SubGridCellY);
            Assert.True(SubGridCellX == (SubGridTree.SubGridTreeDimensionMinus1) && SubGridCellY == (SubGridTree.SubGridTreeDimensionMinus1), "Subgrid cell indices incorrect");

            leafSubgrid.GetSubGridCellIndex(SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension, out SubGridCellX, out SubGridCellY);
            Assert.True(SubGridCellX == 0 && SubGridCellY == 0, "Subgrid cell indices incorrect");
        }  

        [Fact]
        public void Test_SubGrid_AllChangesMigrated()
        {
            ISubGrid leafSubgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            leafSubgrid = new SubGrid(tree, null, SubGridTree.SubGridTreeLevels);

            Assert.False(leafSubgrid.Dirty, "Leaf is Dirty after creation");
            leafSubgrid.Dirty = true;
            Assert.True(leafSubgrid.Dirty, "Leaf is not Dirty after setting Dirty to true");
            leafSubgrid.AllChangesMigrated();
            Assert.False(leafSubgrid.Dirty, "Leaf is Dirty after AllChangesMigrated");
        }

        [Fact]
        public void Test_SubGrid_IsEmpty()
        {
            ISubGrid leafSubgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            leafSubgrid = new SubGrid(tree, null, SubGridTree.SubGridTreeLevels);

            Assert.False(leafSubgrid.IsEmpty(), "Base subgrid class identifying itself as empty");
        }

        [Fact]
        public void Test_SubGrid_RemoveFromParent_Null()
        {
            // This can't be tested fully as the entire Set/Get subgrid functionality is abstract at this point, and
            // RemoveFromParent is part of that abstract workflow. At this level, we will test that no exception occurs
            // if the parent relationship is null

            ISubGrid leafSubgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            leafSubgrid = new SubGrid(tree, null, SubGridTree.SubGridTreeLevels);

            try
            {
                leafSubgrid.RemoveFromParent();
                // Good!
            }
            catch (Exception)
            {
                Assert.True(false,"RemoveFromParent failed with null parent refernec in subgrid");
            }
        }

        [Fact]
        public void Test_SubGrid_ContainsOTGCell()
        {
            ISubGrid leafSubgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            // Create a leaf subgrid with it's cell origin (IndexOriginOffset, IndexOriginOffset) 
            // matching the real work coordaintge origin (0, 0)
            leafSubgrid = tree.ConstructPathToCell(tree.IndexOriginOffset, tree.IndexOriginOffset, SubGridPathConstructionType.CreateLeaf);

            Assert.NotNull(leafSubgrid);
            Assert.True(leafSubgrid.OriginX == tree.IndexOriginOffset && leafSubgrid.OriginX == tree.IndexOriginOffset,
                "Failed to create leaf node at the expected location");

            // Check that a 1m x 1m square (the size of the cells in the subgridtree created above) registers as being
            // a part of the newly created subgrid. First, get the cell enclosing that worl location and then ask
            // the subgrid if it contains it

            uint CellX, CellY;

            Assert.True(tree.CalculateIndexOfCellContainingPosition(0.5, 0.5, out CellX, out CellY),
                          "Failed to get cell index for (0.5, 0.5)");
            Assert.True(leafSubgrid.ContainsOTGCell(CellX, CellY),
                         "Leaf subgrid denies enclosing the OTG cell at (0.5, 0.5)");
        }

        [Fact]
        public void Test_SubGrid_SetAbsoluteOriginPosition()
        {
            ISubGrid subgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            subgrid = new SubGrid(tree, null, 2); // create a node to be a chile of the root node

            // Test setting origin for unattached subgrid
            subgrid.SetAbsoluteOriginPosition(100, 100);

            Assert.True(subgrid.OriginX == 100 && subgrid.OriginY == 100,
                          "SetAbsoluteOriginPosition did not set origin position for subgrid");

            // Add subgrid to the root (which will set it's parent and prevent the origin position from 
            // being changed and will throw an exception)
            tree.Root.SetSubGrid(0, 0, subgrid);
            try
            {
                subgrid.SetAbsoluteOriginPosition(100, 100);

                Assert.True(false,"Setting absolute position for node with a parent did not raise an exception");
            } catch (Exception)
            {
                // As expected`
            }
        }

        [Fact]
        public void Test_SubGrid_SetAbsoluteLevel()
        {
            ISubGrid subgrid = null;

            SubGridTree tree = new SubGridTree(SubGridTree.SubGridTreeLevels, 1.0, new SubGridFactory<NodeSubGrid, LeafSubGrid>());

            subgrid = new SubGrid(tree, null, 2); // create a node to be a chile of the root node

            // Test setting level for unattached subgrid (even though we set it in the constructor above
            subgrid.SetAbsoluteLevel(3);

            Assert.Equal(3, subgrid.Level);

            // Add subgrid to the root (which will set it's parent and prevent the level from 
            // being changed and will throw an exception)
            try
            {
                tree.Root.SetSubGrid(0, 0, subgrid);
                Assert.True(false,"Calling SetSubGrid with an invalid/non-null level did not throw an exception");
            }
            catch (Exception)
            {
                // As expected
            }

            // Restore Level to the correct value of 2, then assign it into the root subgrid
            subgrid.SetAbsoluteLevel(2);
            tree.Root.SetSubGrid(0, 0, subgrid);

            // Now test the level cannot be changed with root as its parent
            try
            {
                subgrid.SetAbsoluteLevel(2);
                Assert.True(false,"Setting absolute level for node with a parent did not raise an exception");
            }
            catch (Exception)
            {
                // As expected`
            }
        }

    }
}
