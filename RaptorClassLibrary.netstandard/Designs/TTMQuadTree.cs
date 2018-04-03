using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSS.Velociraptor.Designs.TTM;

namespace VSS.VisionLink.Raptor.Designs
{
    public class TTMQuadTree : TriangleQuadTree
    {
        public TrimbleTINModel TTM { get; set; }

        // private FSearchStamps : TPAWord = null;

        private StreamWriter writer = new StreamWriter(@"C:\Temp\DTreeProgress-CSharp.txt", false);

        void AddTriangle(Triangle Tri, int Index)
        {
            key_list_rec key_list = key_list_rec.Init();

            FindTriKeys(Tri, ref key_list);

            for (int k = 0; k < key_list.num_keys; k++)
            {
                add_entity_key(key_list.key[k], Index);

                int total = BATree.Select(x => x.Count).Sum();

                writer.WriteLine($"Added ent {Index}, with {key_list.num_keys} keys, using key {key_list.key[k]}, sum elements = {total}");

// Check nothing bad happened (DEBUG ONLY)
/*                foreach (var block in BATree)
                {
                    for (int c = 0; c < block.Count; c++)
                    {
                        if (block.element[c].entity_index == 0 && block.element[c].key == 0)
                        {
                            c = c;
                        }

                        if (c > 0 && block.element[c-1].key > block.element[c].key)
                        {
                            c = c;
                        }
                    }
                }*/

            }
        }

        public TTMQuadTree() : base()
        {
        }

        void FindTriKeys(int index, ref key_list_rec key_list)
        //{ Finds the list of keys associated with the given triangle }
        //{ Assumes the current axes are east_ and north_ }
        {
            FindTriKeys(TTM.Triangles[index], ref key_list);
        }

        void FindTriKeys(Triangle Tri, ref key_list_rec key_list)
        {
            double xmin, ymin, xmax, ymax;
            double x, y;

            xmin = 1E100;
            xmax = -1E100;
            ymin = 1E100;
            ymax = -1E100;

            //  { Find the enclosing rectangle }
            for (int side = 0; side < 3; side++)
            {
                x = Tri.Vertices[side].X;
                y = Tri.Vertices[side].Y;
                if (x < xmin) xmin = x;
                if (x > xmax) xmax = x;
                if (y < ymin) ymin = y;
                if (y > ymax) ymax = y;
            }

            calc_rectangle_keys(xmin, ymin, xmax, ymax, ref key_list);
        }

        public override void find_tri_keys(object triangle, ref key_list_rec key_list)
        {
            FindTriKeys((Triangle)triangle, ref key_list);
        }

        public void Initialise(TrimbleTINModel ATTM, bool AWantSearchStamps)
        {
            TTM = ATTM;

            /*
            if (AWantSearchStamps)
            {
                if (FSearchStamps != null)
                {
                    FreeMem(FSearchStamps);
                    FSearchStamps = null;
                }

                int Num = 0;
                if (TTM.Triangles.Count > 0)
                    Num = (TTM.Triangles.Count + 1);
                else
                  if (TTM.Vertices.Count > 0)
                    Num = 3 * (TTM.Vertices.Count + 1);

                if (Num > 0)
                {
                    // GetMem(FSearchStamps, Num* sizeof(word));
                    //FillChar(FSearchStamps^, num* sizeof(Word), 0);
                }
            }
            */

            resize_quadtree(TTM.Header.MinimumEasting - 100, TTM.Header.MinimumNorthing - 100,
                            TTM.Header.MaximumEasting + 100, TTM.Header.MaximumNorthing + 100);
        }

        public override bool InsideTriangle(object Tri, double world_x, double world_y) => ((Triangle)Tri).PointInTriangle(world_x, world_y);

        public override object ReadEntityRef(int index) => TTM.Triangles[index];

        public override void resize_quadtree(double min_x, double min_y, double max_x, double max_y)
        {
            BATree = null;

            if (!Initialize_BTree())
            {
                throw new Exception("resize_quadtree: initialize_Btree failed");
            }

            set_tree_range(min_x, min_y, max_x, max_y);

            for (int I = 0; I < TTM.Triangles.Count; I++)
               AddTriangle(TTM.Triangles[I], I);
           
            writer.Close();
        }
    }
}
