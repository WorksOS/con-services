using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using VSS.VisionLink.Raptor.Common;

namespace VSS.VisionLink.Raptor.Designs
{
    /// <summary>
    /// Provides a spatial index for triangles in a surface to provide rapid location and scanning from a spatial perspective
    /// </summary>
    public abstract class TriangleQuadTree
    {
        /// <summary>
        /// A block index ued to indicate no block
        /// </summary>
        private const int nilptr = -1;

        private const int elements_per_block = 50;
        private const int tree_splits = 10; //  case keytype of longint: (max = 10); Smallint: (max = 5) 
        private const int tree_divisions = 1 << tree_splits; // Number of divisions in each axis 
        private const int max_Btree_levels = 5; // Should be enough = ~34,000,000 elements!

        private const uint outside_quadtree = 0xffffffff;

        /// <summary>
        /// Stores keys generated from the bounding box of an element within the quadtree extents
        /// </summary>
        public struct key_list_rec
        {
            public byte num_keys;
            public uint[] key; // : array[1..4] of keytype;

            public static key_list_rec Init()
            {
                return new key_list_rec()
                {
                    num_keys = 0,
                    key = new uint[4]
                };
            }
        }

        /// <summary>
        /// The entry in the BTree containing the enentiy key, the entity and th epointer to the BTree block containing the next key in sequence
        /// </summary>
        public struct Tentity_element
        {
            public uint key;
            public int entity_index; // : longint; //entity_key;
            public int next; // : block_index_type;
        }

        /// <summary>
        /// Stores all information for a block in the BTree
        /// </summary>
        public class Ttree_block_type
        {
            public int block_nbr; //: block_index_type;
            public int Count; // : Integer; //0..elements_per_block;
            public int back_pointer; // : block_index_type;
            public Tentity_element[] element; // : array[1..elements_per_block] of Tentity_element;

            public Ttree_block_type()
            {
                block_nbr = -1;
                Count = 0;
                back_pointer = nilptr;
                element = new Tentity_element[elements_per_block];
            }
        }

        /// <summary>
        /// State relevant to a particular search being conducted in the spatial data
        /// </summary>
        public struct Tsearch_state_rec
        {
            public key_list_rec key_list;
            public int list_index; // : Integer;
            public uint quadtree_key; // : keytype;
            public int current_level; // : level_index_type;
            public int current_block; // : block_index_type;
            public int current_index; // : element_index_type;
            public Ttree_block_type[] cache_state; // : Tpath_cache_type;
            public bool scanning_key_group; // : boolean;
            public bool search_started; // : boolean;
            public uint current_resolved_square,
                       resolution_mask,
                       square_size_mask; // : keytype;
            public bool return_all; // : boolean;

            public static Tsearch_state_rec Init()
            {
                return new Tsearch_state_rec()
                {
                    cache_state = new Ttree_block_type[max_Btree_levels],
                    key_list = key_list_rec.Init()
                };
            }

            public void reset_search(TriangleQuadTree tree)
            {
                list_index = 0; //{ Start with 1st key in ORDERED list }
                scanning_key_group = false;
                search_started = false;

                // Set the cache state to be the root path cache node for the tree
                cache_state[0] = tree.non_search_path_cache[0];
            }

            public void start_search(double x1, double y1, double x2, double y2,
                                     bool return_all_entities,
                                     TriangleQuadTree tree)
            // Initializes a search of the quadtree for entities whose keys overlap the search key. 
            {
                double resolution, square_size;
                double min_x, min_y, max_x, max_y;

                Raptor.Utilities.MinMax.SetMinMax(ref x1, ref x2);
                Raptor.Utilities.MinMax.SetMinMax(ref y1, ref y2);

                tree.calc_rectangle_keys(x1, y1, x2, y2, ref key_list);

                // If key returned is outside_quadtree then check if the rectangle
                //  overlaps the quadtree and if so, search the whole quadtree

                if (key_list.key[0] == outside_quadtree)
                {
                    // Calculate a rectangle intersection for the two rectangles
                    if (x1 > tree.tree_min_x)
                        min_x = x1;
                    else
                        min_x = tree.tree_min_x;

                    if (y1 > tree.tree_min_y)
                        min_y = y1;
                    else
                        min_y = tree.tree_min_y;

                    if (x2 > tree.tree_max_x)
                        max_x = tree.tree_max_x;
                    else
                        max_x = x2;

                    if (y2 > tree.tree_max_y)
                        max_y = tree.tree_max_y;
                    else
                        max_y = y2;

                    // If there is an intersection then search the entire quadtree
                    if ((max_x > min_x) && (max_y > min_y))
                        key_list.key[0] = 0;
                }

                return_all = return_all_entities;

                reset_search(tree);

                if (!return_all_entities)
                {
                    // Change X/Y resolution to place constraints on how deep in the quadtree
                    // the search will progress
                    double Xresolution = 0;
                    double Yresolution = 0;

                    if (Xresolution / tree.Xeps < Yresolution / tree.Yeps)
                    {
                        resolution = Xresolution;
                        square_size = 2.0 * (tree.Xeps); //{ Size of smallest quadtree square }
                    }
                    else
                    {
                        resolution = Yresolution;
                        square_size = 2.0 * (tree.Yeps);
                    }

                    if (square_size > 2.0 * (resolution))
                        return_all = true;  //{ Resolution too fine to be able to discard any entities }
                    else
                    {
                        //{ Find a mask which corresponds to a quadtree square about the size of a pixel. }
                        resolution_mask = 0xFFFFFFFF;
                        square_size_mask = 7;
                        current_resolved_square = 0;
                        while (square_size < resolution)
                        {
                            resolution_mask = resolution_mask << 3;
                            square_size_mask = square_size_mask << 3;
                            square_size = 2.0 * (square_size);
                        }
                    }
                } // { if not return_all }
            }
        }

        public Ttree_block_type[] non_search_path_cache = new Ttree_block_type[max_Btree_levels];
        private int num_Btree_blocks; // : Integer; //0..MaxTreeBlocksLimit;
        private int Btree_levels; // : Integer; //1..max_Btree_levels;

        private double local_tree_min_x, local_tree_max_x, local_tree_min_y, local_tree_max_y;
        private double local_tree_x_diff, local_tree_y_diff;
        private double local_tree_x_diff_div_divisions, local_tree_y_diff_div_divisions;

        bool tree_range_initialized; // : boolean;

        public double Xeps, Yeps; // : Double; { < minimum x or y cell dimension }

        public List<Ttree_block_type> BATree;

        public double tree_min_x { get { return local_tree_min_x; } }
        public double tree_min_y { get { return local_tree_min_y; } }
        public double tree_max_x { get { return local_tree_max_x; } }
        public double tree_max_y { get { return local_tree_max_y; } }


        public abstract void resize_quadtree(double min_x, double min_y, double max_x, double max_y);
        public abstract object ReadEntityRef(int index);
        public abstract bool InsideTriangle(object tri, double world_x, double world_y);
        public abstract void find_tri_keys(object data, ref key_list_rec key_list);

        public bool tree_empty()
        {
            return !tree_range_initialized || non_search_path_cache[0].Count == 0;
        }

        public static int compare(ref Tentity_element a, ref Tentity_element b)
        {
            int Result = a.key.CompareTo(b.key);

            return Result == 0 ? a.entity_index.CompareTo(b.entity_index) : Result;
        }

        bool keys_overlap(uint key1, uint key2)
        //{   Repeatedly remove "don't care"s from keys until there are no more "don't care"s or the keys match. }
        {
            if ((key1 == outside_quadtree) || (key2 == outside_quadtree))
                return true;
            else
            {
                while ((key1 != key2) && (((key1 & 7) == 0) || ((key2 & 7) == 0)))
                {
                    key1 >>= 3;
                    key2 >>= 3;
                }
                return key1 == key2;
            };
        }

        void set_smallest_square_size()
        //   Sets Xeps &Yeps to « the size of the smallest square in the x &y directions respectively. 
        {
            Xeps = 0.5 * local_tree_x_diff_div_divisions;
            Yeps = 0.5 * local_tree_y_diff_div_divisions;

            //  safeguard against later division by zero 
            if (Xeps == 0)
                Xeps = 1;
            if (Yeps == 0)
                Yeps = 1;
        }

        protected void set_tree_range(double min_x, double min_y, double max_x, double max_y)
        // { call to set up the quadtree extents. Allow 20 % extra in each direction 
        {
            if (!tree_range_initialized)
            {
                local_tree_min_x = min_x - ((max_x - min_x) * 0.2);
                local_tree_min_y = min_y - ((max_y - min_y) * 0.2);
                local_tree_max_x = max_x + ((max_x - min_x) * 0.2);
                local_tree_max_y = max_y + ((max_y - min_y) * 0.2);

                local_tree_x_diff = local_tree_max_x - local_tree_min_x;
                local_tree_y_diff = local_tree_max_y - local_tree_min_y;

                local_tree_x_diff_div_divisions = local_tree_x_diff / tree_divisions;
                local_tree_y_diff_div_divisions = local_tree_y_diff / tree_divisions;

                set_smallest_square_size();

                tree_range_initialized = true;
            }
        }

        void initialize_tree_range(double min_x, double min_y, double max_x, double max_y)
        {
            set_tree_range(min_x, min_y, max_x, max_y);
        }

        public uint rectangle_key(double min_x, double min_y, double max_x, double max_y)
        //{ Returns the key of the quadtree rectangle bounding the given coords }
        {
            uint key; //: keytype;
            uint x1, x2, y1, y2;
            int shift = 0;
            uint square_num;

            void integerize_coords(double x, double y, out uint i, out uint j)
            {
                if (x < local_tree_min_x)
                    i = outside_quadtree;
                else if (x >= local_tree_max_x)
                    i = outside_quadtree;
                else
                    i = (uint)Math.Truncate(tree_divisions * (x - local_tree_min_x) / local_tree_x_diff);

                if (y < local_tree_min_y)
                    j = outside_quadtree;
                else if (y >= local_tree_max_y)
                    j = outside_quadtree;
                else
                    j = (uint)Math.Truncate(tree_divisions * (y - local_tree_min_y) / local_tree_y_diff);
            }

            if (!tree_range_initialized)
                initialize_tree_range(min_x, min_y, max_x, max_y);

            // { Get the coordinates in quadtree integerized form }
            integerize_coords(min_x, min_y, out x1, out y1);
            if (x1 == outside_quadtree || y1 == outside_quadtree)
                return outside_quadtree;

            if (min_x != max_x || min_y != max_y)
            {
                integerize_coords(max_x, max_y, out x2, out y2);
                if (x2 == outside_quadtree || y2 == outside_quadtree)
                    return outside_quadtree;

                // {'Expand' the possible key range until the resulting rectangle encloses the coordinates }
                while (x1 != x2 || y1 != y2)
                {
                    x1 >>= 1;
                    x2 >>= 1;
                    y1 >>= 1;
                    y2 >>= 1;
                    shift++;
                }
            } // { not point }
            else
            {
                x2 = x1;
                y2 = y1;
            } // { rectangle is point }

            //{ Make up key }
            key = 0;
            for (; shift < tree_splits; shift++)
            {
                //  Interleave y coordinate as most significant 
                // Squares are numbered 1..4 from left to right, bottom to top 
                square_num = ((y1 & 1) << 1) + (x1 & 1) + 1;

                // Insert this number in the key at the position given by shift*3 since
                // each square number takes up 3 bits. 
                key |= (square_num << (shift * 3));

                // Rotate coordinates to the next bit 
                x1 >>= 1;
                y1 >>= 1;
            }

            return key;
        }

        void key_rectangle(uint key, // : keytype;
        ref double minX, ref double minY, ref double maxX, ref double maxY)
        //{ Returns the coordinates of the quadtree rectangle corresponding to the key  }
        {
            int x1, y1, x2, y2, xbit, ybit;
            int square;

            if (key != outside_quadtree)
            {
                // { First extract the interleaved integerized coordinates }
                x1 = 0;
                y1 = 0;
                x2 = 0;
                y2 = 0;
                for (int shift = 0; shift < tree_splits; shift++)
                {
                    // { Extract the square number }
                    square = (int)(key & 7);
                    if (square == 0)
                    {
                        // { "Don't care" found, so set bits on maximums }
                        x2 = x2 | (1 << shift);
                        y2 = y2 | (1 << shift);
                    }
                    else
                    {
                        square--;       // { Put coords in 0..3 range }
                                        // { Split into x & y }
                        xbit = (square & 1) << shift;
                        ybit = (square >> 1) << shift;
                        //{ Set min &max x & y }
                        x1 = x1 | xbit;
                        y1 = y1 | ybit;
                        x2 = x2 | xbit;
                        y2 = y2 | ybit;
                    }
                    key = key >> 3;
                }

                // { Increment the maxima to include the whole square }
                x2++;
                y2++;

                // { Calculate the true coordinates }
                minX = x1 * local_tree_x_diff_div_divisions + local_tree_min_x;
                minY = y1 * local_tree_y_diff_div_divisions + local_tree_min_y;
                maxX = x2 * local_tree_x_diff_div_divisions + local_tree_min_x;
                maxY = y2 * local_tree_y_diff_div_divisions + local_tree_min_y;
            }
        }

        bool split_rectangle(uint key, // : keytype;
        ref double x0, ref double y0, ref double x1, ref double y1, ref double x2, ref double y2)
        //        Splits the rectangle given by the key into 4.
        //        x0,y0 = min, x1,y1 = middle, x2,y2 = max.
        //        shift = position of split in key.
        //  Function returns false if key cannot be split. }
        {
            if (key == outside_quadtree || (key & 7) != 0)
                return false;
            else
            {
                // { Find the enclosing quadtree rectangle}
                key_rectangle(key, ref x0, ref y0, ref x2, ref y2);

                // { Calculate the middle point }
                x1 = 0.5 * (x0 + x2);
                y1 = 0.5 * (y0 + y2);

                return true;
            }
        }

        bool rectangles_overlap(double rx0, double ry0, double rx1, double ry1,        // { first rectangle }
        double sx0, double sy0, double sx1, double sy1) // { 2nd rectangle }
                                                        //{ Returns true if the rectangles overlap. }
                                                        //{ This function requires that rx0 <= rx1, ry0 <= ry1, sx0 <= sx1 & sy0 <= sy1 }
        {
            return (sx0 <= rx1) && (rx0 <= sx1) && (sy0 <= ry1) && (ry0 <= sy1);
        }

        void find_minimum_key(double x1, double y1, double x2, double y2, double minX, double minY, double maxX, double maxY,
                 ref key_list_rec key_list)
        //  Finds the key of the part of the rectangle x1..y2 enclosed by minX..maxY 
        {
            if (rectangles_overlap(x1, y1, x2, y2, minX, minY, maxX, maxY))
            {
                //{ Find enclosed part of rectangle }
                // { Add Xeps to put x1 firmly in the next rectangle  }
                if (minX > x1)
                    x1 = minX + Xeps;
                if (minY > y1)
                    y1 = minY + Yeps;

                //{ Subtract Xeps since maxX is in the 'next' rectangle }
                //{ i.e. if x in rectangle then minX <= x < maxX }
                // { (maxX - Xeps is just inside the smallest quadtree rectangle bounded on the right by maxX }
                if (maxX < x2)
                    x2 = maxX - Xeps;
                if (maxY < y2)
                    y2 = maxY - Yeps;

                // { Calculate key }
                key_list.key[key_list.num_keys] = rectangle_key(x1, y1, x2, y2);
                key_list.num_keys++;
            }
        }

        public void calc_rectangle_keys(double x1, double y1, double x2, double y2,
                                        ref key_list_rec key_list)
        //{ Find the set of up to four quadtree keys which enclose the given rectangle
        //  in minimum total area. }
        {
            uint key;// : keytype;

            // { check that none of the coordinates are nullreal }
            if ((x1 == Consts.NullDouble) || (y1 == Consts.NullDouble) ||
              (x2 == Consts.NullDouble) || (y2 == Consts.NullDouble))
            {
                key_list.num_keys = 0;
                return;
            }

            // { Require x1 <= x2 and y1 <= y2}
            Raptor.Utilities.MinMax.SetMinMax(ref x1, ref x2);
            Raptor.Utilities.MinMax.SetMinMax(ref y1, ref y2);

            // { Get the associated key }
            key = rectangle_key(x1, y1, x2, y2);

            double minX = 0, minY = 0, midX = 0, midY = 0, maxX = 0, maxY = 0;

            // { Check if the rectangle can be split }
            if (!split_rectangle(key, ref minX, ref minY, ref midX, ref midY, ref maxX, ref maxY))
            {
                key_list.num_keys = 1;
                key_list.key[0] = key;
            }
            else
            {
                key_list.num_keys = 0;

                // { Check if the line intersects each of the split rectangles }
                find_minimum_key(x1, y1, x2, y2, minX, minY, midX, midY, ref key_list);
                find_minimum_key(x1, y1, x2, y2, midX, minY, maxX, midY, ref key_list);
                find_minimum_key(x1, y1, x2, y2, minX, midY, midX, maxY, ref key_list);
                find_minimum_key(x1, y1, x2, y2, midX, midY, maxX, maxY, ref key_list);
            }
        }

        /// <summary>
        /// Loads a BTree block from the BTree structure into the path cache
        /// </summary>
        /// <param name="search_path_cache"></param>
        /// <param name="level"></param>
        /// <param name="index"></param>
        void load_block(Ttree_block_type[] search_path_cache, // : Tpath_cache_type;
                        int level, // : level_index_type;
                        int index) // : block_index_type );
        {
            search_path_cache[level] = BATree[index];
        }

        /// <summary>
        /// Locate the first element in the index that matches the given key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="search_path_cache"></param>
        /// <param name="tree_level"></param>
        /// <param name="key_block"></param>
        /// <param name="key_index"></param>
        /// <returns></returns>
        bool find_first_key(uint key,// : keytype;
                            Ttree_block_type[] search_path_cache, // : Tpath_cache_type;
                            ref int tree_level, // : level_index_type;
                            ref int key_block, // : block_index_type;
                            ref int key_index) // : element_index_type )
        {
            //{ Finds the first occurrence of the quadtree key in the B-tree }

            void scan_down_tree(ref int level, // : level_index_type;
                                ref int block, // : block_index_type;
                                ref int index) // : element_index_type );
            {
                int new_index = 0; //: element_index_type;

                int new_block = index == 0 ? search_path_cache[level].back_pointer : search_path_cache[level].element[index - 1].next;

                if (new_block != nilptr)
                {
                    int new_level = level + 1;
                    if (find_key(key, search_path_cache, ref new_level, ref new_block, ref new_index))
                    {
                        scan_down_tree(ref new_level, ref new_block, ref new_index);
                        level = new_level;
                        block = new_block;
                        index = new_index;
                    }
                }
            }

            tree_level = 0;
            key_block = search_path_cache[0].block_nbr;
            if ((search_path_cache[0].Count > 0) && find_key(key, search_path_cache, ref tree_level, ref key_block, ref key_index))
            {
                scan_down_tree(ref tree_level, ref key_block, ref key_index);
                return true;
            }
            else
                return false;
        }

        bool find_key(Tentity_element key,
                      Ttree_block_type[] search_path_cache,
                      ref int tree_level, // : level_index_type;
                      ref int key_block, // : block_index_type;
                      ref int key_index) // : element_index_type )
        // Recursively scan the B-tree to find the key. key_index is returned such that key <= element key_index 
        {
            int upper; //: element_index_type;
            int middle, lower; //: element_index_type;
            int res; //: compare_result;
            bool found_lower; //: boolean;
            int lower_level; //: level_index_type;
            int lower_block; //: block_index_type;
            int lower_index = 0; //: element_index_type;

            bool result = false;

            if (tree_empty())
                return false;

            // Load the block to look at 
            load_block(search_path_cache, tree_level, key_block);

            Ttree_block_type block = search_path_cache[tree_level];
            res = compare(ref key, ref block.element[0]);

            if (res != 1)
            {
                lower_block = block.back_pointer;
                key_index = 0;
            }
            else
            {
                res = compare(ref key, ref block.element[block.Count - 1]);

                if (res == 1)
                {
                    lower_block = block.element[block.Count - 1].next;
                    key_index = block.Count;
                }
                else
                {
                    upper = block.Count - 1;
                    lower = 0;
                    do
                    {
                        middle = (lower + upper) / 2;

                        res = compare(ref key, ref block.element[middle]);

                        switch (res)
                        {
                            case 1: lower = middle + 1; break;
                            case 0: upper = middle; break;
                            case -1: upper = middle - 1; break;
                        }
                    }
                    while (!((lower > upper) || (res == 0 && lower == upper)));

                    key_index = lower;

                    res = compare(ref key, ref block.element[key_index]);

                    lower_block = block.element[key_index - 1].next;
                } 
            } 

            if (lower_block == nilptr) // { No lower levels to search }
                result = (res == 0);
            else
            {
                // Look at the next level down 
                lower_level = tree_level + 1;
                found_lower = find_key(key, search_path_cache, ref lower_level, ref lower_block, ref lower_index);
                if ((res != 0) || found_lower)
                {
                    // { Return the index of the element in the lower level block }
                    tree_level = lower_level;
                    key_block = lower_block;
                    key_index = lower_index;
                    result = found_lower;
                }
                else
                    result = res == 0;
            }

            return result;
        }

        bool find_key(uint key,
                      Ttree_block_type[] search_path_cache,
                      ref int tree_level, // : level_index_type;
                      ref int key_block, // : block_index_type;
                      ref int key_index)
        {
            // Recursively scan the B-tree to find the key.  key_index is returned such that key <= element key_index 

            int upper; //: element_index_type;
            int middle, lower; //: element_index_type;
            bool found_lower; //: boolean;
            int lower_level; //: level_index_type;
            int lower_block; //: block_index_type;
            int lower_index = 0; //: element_index_type;

            bool result = false;

            if (tree_empty())
                return false;

            // { Load the block to look at }
            load_block(search_path_cache, tree_level, key_block);

            Ttree_block_type block = search_path_cache[tree_level];
            int res = key.CompareTo(block.element[0].key);

            if (res != 1)
            {
                lower_block = block.back_pointer;
                key_index = 0;
            }
            else
            {
                res = key.CompareTo(block.element[block.Count - 1].key);

                if (res == 1)
                {
                    lower_block = block.element[block.Count - 1].next;
                    key_index = block.Count; 
                }
                else
                {
                    upper = block.Count - 1;
                    lower = 0;
                    do
                    {
                        middle = (lower + upper) / 2;

                        res = key.CompareTo(block.element[middle].key);

                        switch (res)
                        {
                            case 1: lower = middle + 1; break;
                            case 0: upper = middle; break;
                            case -1: upper = middle - 1; break;
                        }
                    }
                    while (!((lower > upper) || (res == 0 && lower == upper)));

                    key_index = lower;

                    res = key.CompareTo(block.element[key_index].key);

                    lower_block = block.element[key_index - 1].next;
                } 
            } 

            if (lower_block == nilptr) // { No lower levels to search }
                result = (res == 0);
            else
            {
                // Look at the next level down 
                lower_level = tree_level + 1;
                found_lower = find_key(key, search_path_cache, ref lower_level, ref lower_block, ref lower_index);
                if ((res != 0) || found_lower)
                {
                    // { Return the index of the element in the lower level block }
                    tree_level = lower_level;
                    key_block = lower_block;
                    key_index = lower_index;
                    result = found_lower;
                }
                else
                    result = res == 0;
            }

            return result;
        }

        void pop_to_next_element(Ttree_block_type[] search_path_cache, //const search_path_cache : Tpath_cache_type;
                                 ref int tree_level, // : level_index_type;
                                 ref int block_index, // : block_index_type;
                                 ref int element_index) //: element_index_type );
                                                        //{ Moves to the 'next' element after the end of the given block. }
        {
            while ((element_index > search_path_cache[tree_level].Count - 1) && (tree_level > 0))
            {
                // jump up to the parent 
                tree_level--;

                element_index = 0;
                if (block_index != search_path_cache[tree_level].back_pointer)
                {
                    while (search_path_cache[tree_level].element[element_index].next != block_index)
                    {
                        element_index++;
                    }
                    element_index++;
                }
                block_index = search_path_cache[tree_level].block_nbr;
            }
        }

        bool next_matching_key(uint current_key, // : keytype;
                               Ttree_block_type[] search_path_cache, // var search_path_cache : Tpath_cache_type;
                               ref int tree_level, // : level_index_type;
                               ref int block_index, //: block_index_type;
                               ref int element_index) // : element_index_type )
                                                      //{ Finds the next element that matches the quadtree key in the position
                                                      //  specified above. }
        {
            int next_element = search_path_cache[tree_level].element[element_index].next;

            // { Advance to the next key }
            if (next_element == nilptr)
                element_index++;
            else
            {
                //{ Drop down to the next level }
                tree_level++;
                block_index = next_element;
                element_index = 0;
                load_block(search_path_cache, tree_level, block_index);
                while (search_path_cache[tree_level].back_pointer != nilptr)
                {
                    block_index = search_path_cache[tree_level].back_pointer;
                    tree_level++;
                    load_block(search_path_cache, tree_level, block_index);
                }
            }

            // { If we're after the end of a block, pop up to the next element }
            if (element_index > search_path_cache[tree_level].Count - 1)
                pop_to_next_element(search_path_cache, ref tree_level, ref block_index, ref element_index);

            return (element_index <= search_path_cache[tree_level].Count - 1) && keys_overlap(current_key, search_path_cache[tree_level].element[element_index].key);
        }

        public bool next_entity(ref Tsearch_state_rec searchState,
                                ref int ent_index,
                                ref object data)
        //{ Search for the next entity whose keys overlap the search key.
        //  'start_search' must be called before this function. }
        {
            bool entity_found;
            uint current_key = outside_quadtree;

            uint next_key(uint original_key, uint last_key)
            {
                //{ Find the most significant "don't care" digit in the last_key, and
                //  set this digit to the corresponding digit in the original_key.}

                const int tree_splits_Times3 = tree_splits * 3;

                int shift = 0;
                uint mask = 0, new_key = last_key;

                while ((new_key & 7) == 0 && shift < tree_splits_Times3)
                {
                    new_key >>= 3;
                    mask = mask | ((uint)7 << shift);
                    shift += 3;
                }
                new_key = original_key & ~(mask >> 3);
                if (new_key == last_key)
                    new_key = outside_quadtree;

                return new_key;
            }

            if (tree_empty())
                return false;

            // Restore the cache state to that of the last search 
            entity_found = false;

            while (!entity_found &&  // { Check if the search has passed the end of the B-tree }
                   !(searchState.search_started && ((searchState.cache_state[searchState.current_level].Count - 1) < searchState.current_index)))
            {
                if (!searchState.scanning_key_group)
                {
                    //{ Calculate the next quadtree key to test for }
                    if (!searchState.search_started)
                    {
                        searchState.quadtree_key = 0; // { Full quadtree }
                        searchState.search_started = true;
                    } //{ not search_started }
                    else
                    {
                        //{ Get the key which was the first key which did not match the
                        //  next_matching_key search }
                        current_key = searchState.cache_state[searchState.current_level].element[searchState.current_index].key;

                        //{ Use quadtree_key and current_key to find the next key to search for }
                        //{ quadtree_key is the last key containing the current key in key_list }
                        while (!keys_overlap(searchState.key_list.key[searchState.list_index], current_key) &&
                               (searchState.quadtree_key.CompareTo(current_key) == -1))
                        {
                            searchState.quadtree_key = next_key(searchState.key_list.key[searchState.list_index], searchState.quadtree_key);
                            //{ Have we already searched all keys overlapping key[list_index]? }
                            if (searchState.quadtree_key == outside_quadtree && (searchState.list_index < searchState.key_list.num_keys))
                            {
                                //{ Then advance to the next key in the list }
                                searchState.list_index++;
                                // Find the largest(in area) possible key which overlaps
                                //  key[list_index] but not key[list_index - 1] }
                                searchState.quadtree_key = 0;
                                while (searchState.quadtree_key.CompareTo(searchState.key_list.key[searchState.list_index - 1]) != 1)
                                    searchState.quadtree_key = next_key(searchState.key_list.key[searchState.list_index], searchState.quadtree_key);
                            }
                        }
                    }

                    //{ Test if the current_key overlaps our test key }
                    if (searchState.quadtree_key != 0 //{ not search start }
                        && keys_overlap(searchState.key_list.key[searchState.list_index], current_key))
                        entity_found = true;
                    else
                    { //{ Search for quadtree_key }
                        if ((searchState.quadtree_key == 0) &&              // { if starting search }
                            (searchState.key_list.num_keys == 1) &&        //  { for only entities outside the quadtree }
                            (searchState.key_list.key[0] == outside_quadtree))
                        {
                            searchState.quadtree_key = outside_quadtree;// { then only consider outside_quadtree keys }
                        }
                        searchState.current_level = 0;// { root}
                        searchState.current_block = searchState.cache_state[0].block_nbr;
                        entity_found = find_first_key(searchState.quadtree_key, searchState.cache_state, ref searchState.current_level,
                                                        ref searchState.current_block, ref searchState.current_index);

                        //  If we didn't find the key, and the next lower key is the last in a
                        //  block, then we need to point to the next greater key(which is in a higher level). 
                        if (searchState.current_index > searchState.cache_state[searchState.current_level].Count - 1)
                            pop_to_next_element(searchState.cache_state, ref searchState.current_level, ref searchState.current_block, ref searchState.current_index);

                        //{ Test if the key found overlaps the new current key }
                        if (!entity_found)
                            entity_found = (searchState.current_index <= searchState.cache_state[searchState.current_level].Count - 1) &&
                                       keys_overlap(searchState.key_list.key[searchState.list_index], searchState.cache_state[searchState.current_level].element[searchState.current_index].key);
                    }

                    searchState.scanning_key_group = entity_found;
                }

                //{ If we are in the middle of scanning linearly through the quadtree,
                //  get the next element from the quadtree. }
                if (searchState.scanning_key_group && !entity_found)
                {
                    entity_found = next_matching_key(searchState.key_list.key[searchState.list_index], searchState.cache_state,
                                                     ref searchState.current_level, ref searchState.current_block, ref searchState.current_index);
                    searchState.scanning_key_group = entity_found;
                }

                if (entity_found)
                {
                    entity_found = searchState.return_all
                        || (searchState.cache_state[searchState.current_level].element[searchState.current_index].key == outside_quadtree)
                        || ((searchState.cache_state[searchState.current_level].element[searchState.current_index].key & searchState.resolution_mask) != searchState.current_resolved_square)
                        || ((searchState.cache_state[searchState.current_level].element[searchState.current_index].key & searchState.square_size_mask) == 0);

                    //{ Report the entity }
                    if (entity_found)
                    {
                        // { Load the entity data }
                        data = ReadEntityRef(searchState.cache_state[searchState.current_level].element[searchState.current_index].entity_index);

                        if (data == null)
                            entity_found = false;
                        else
                        {
                            ent_index = searchState.cache_state[searchState.current_level].element[searchState.current_index].entity_index;
                            searchState.current_resolved_square = (searchState.cache_state[searchState.current_level].element[searchState.current_index].key & searchState.resolution_mask);
                        }
                    }
                }
            }
            return entity_found;
        } 

        bool find_nearest(double world_x, double world_y,
                          double proximity_dist,
                          ref object best_data)
        {
            Tsearch_state_rec searchState = Tsearch_state_rec.Init();

            int index = -1;
            object data = null;

            if (tree_empty())
                return false;

            searchState.start_search(world_x - proximity_dist, world_y - proximity_dist,
                                     world_x + proximity_dist, world_y + proximity_dist,
                                     true, this);

            while (next_entity(ref searchState, ref index, ref data))
                if (InsideTriangle(data, world_x, world_y))
                {
                    best_data = data; //ReadEntityRef(index);
                    return true;
                }

            return false;
        }

        bool find_unique_key(uint key, // : keytype;
                             Ttree_block_type[] search_path_cache, // var search_path_cache : Tpath_cache_type;
                             int ent_index, // : longint;
                             ref int tree_level, // : level_index_type;
                             ref int key_block, // : block_index_type;
                             ref int key_index) // : element_index_type ) : boolean;
                                                //{ Finds the position of the entity reference in the B-tree.
                                                //  If not found, the returned index is the insert position of the key. }
        {
            Tentity_element tree_element = new Tentity_element();

            //{ Check for empty tree }
            if (search_path_cache[0].Count == 0)
                return false;
            else
            {
                tree_element.key = key;
                tree_element.entity_index = ent_index;

                tree_level = 0; //{root}
                key_block = search_path_cache[0].block_nbr;
                return find_key(tree_element, search_path_cache, ref tree_level, ref key_block, ref key_index);
            }
        }

        void split_block(int tree_level, // : level_index_type;
                         int block_index, // : block_index_type;
                         int cur_element_index, // : element_index_type;
                         ref int new_level, // : level_index_type;
                         ref int new_block_index)  // : block_index_type; 
                                                    //{ Frees up space in the 'block_index' block by
                                                    //    a) Moving some elements to the next block('Balancing'), or if the
                                                    // neighbour block is full,
                                                    //    b) Splits the block into two nodes, and moves the middle entry up one level.
        {
            int index_in_parent(int _parent_level, // : level_index_type;
                                int _block_index) // : block_index_type ) : ext_element_type;
            {
                if (block_index == non_search_path_cache[_parent_level].back_pointer)
                    return -1;
                else
                {
                    int Result = 0;
                    while (non_search_path_cache[_parent_level].element[Result].next != _block_index)
                        Result++;
                    return Result;
                }
            }

            const int min_balance_move = elements_per_block / 10;  //{ Minimum number of blocks to move in a balance }

            int parent_level = -1; // : level_index_type;
            int middle_index = -1; // : element_index_type;
            int cur_block_element = -1; // : Integer; //0..elements_per_block; cur_block_element = 0; // We want a failure if this value is used (Shouldn't be)
            int neighbour_block = -1,
            parent_block = -1; // : block_index_type;
            Ttree_block_type new_block = null; // new Ttree_block_type();
            Tentity_element middle_element; // = new Tentity_element();
            int Work_Block_Index; // : block_index_type;
            bool balancing = false;
            int count_to_move; // : element_index_type;

            if (tree_level > 0)
            {
                //{ Find the index of the element pointing to our block in the parent }
                cur_block_element = index_in_parent(tree_level - 1, block_index);

                // Check if the parent block needs splitting 
                if (non_search_path_cache[tree_level - 1].Count == elements_per_block)
                {
                    // Set a dummy index so we can find the position in the new parent block we'll be adding to 
                    int dummy_index = cur_block_element == -1 ? 0 : cur_block_element;

                    split_block(tree_level - 1, non_search_path_cache[tree_level - 1].block_nbr, dummy_index,
                                 ref parent_level, ref parent_block);

                    //{ Check if the root has been split }
                    if (parent_level == tree_level)
                        tree_level++;

                    //{ Make sure we have the correct block loaded in parent position }
                    load_block(non_search_path_cache, parent_level, parent_block);

                    // { Find new index in parent }
                    cur_block_element = index_in_parent(parent_level, block_index);

                    // Check if we can overflow into the neighbour block 'greater than' this
                    if (cur_block_element < non_search_path_cache[tree_level - 1].Count - 1)
                    {
                        // { Load neighbour }
                        neighbour_block = non_search_path_cache[tree_level - 1].element[cur_block_element + 1].next;
                        new_block = BATree[neighbour_block];

                        //{ Test if there is enough room to overflow }
                        if (new_block.Count <= (elements_per_block - 2 * min_balance_move))
                            balancing = true; //{ Balance instead }
                    }
                }
            }

            //  Fill up about half of the space available in the new_block 
            if (balancing)
                count_to_move = (elements_per_block + 1 - new_block.Count) / 2;
            else
                count_to_move = elements_per_block / 2;

            middle_index = non_search_path_cache[tree_level].Count - count_to_move; // + 1
            middle_element = non_search_path_cache[tree_level].element[middle_index];

            // Determine the index for the new (or neighbour) block
            if (balancing)
                Work_Block_Index = neighbour_block;
            else
            {
                Work_Block_Index = num_Btree_blocks++;
                BATree.Add(new Ttree_block_type()
                {
                    block_nbr = Work_Block_Index,
                    Count = 0,
                    back_pointer = nilptr
                });
            }

            // Get the new block 
            new_block = BATree[Work_Block_Index];

            // Set up the new block 
            // First move any existing elements
            if (balancing)
            {
                Array.Copy(new_block.element, 0, new_block.element, count_to_move, new_block.Count);

                // { Copy in the associated element from the parent }
                new_block.element[count_to_move - 1] = non_search_path_cache[tree_level - 1].element[cur_block_element + 1];
                new_block.element[count_to_move - 1].next = new_block.back_pointer;
            }

            // { Then move in elements from the full block }
            if (count_to_move > 1)
                Array.Copy(non_search_path_cache[tree_level].element, middle_index + 1, new_block.element, 0, count_to_move - 1);

            if (balancing)
                new_block.Count = new_block.Count + count_to_move;
            else
                new_block.Count = count_to_move - 1;
            new_block.back_pointer = middle_element.next;

            // { Set the pointer to the new block }
            middle_element.next = Work_Block_Index;

            //  { update the old block }
            non_search_path_cache[tree_level].Count = middle_index; // - 1; // Note: this is now a zero-based index

            //  { Update the index of the passed element }
            if (cur_element_index == middle_index && tree_level < Btree_levels - 1)
                new_block_index = Work_Block_Index;  //  Need to return index of parent block of the block being split at level tree_level + 1 
            else if (cur_element_index > middle_index)
                new_block_index = Work_Block_Index;
            else
                new_block_index = block_index;

            if (tree_level == 0) // { Add one level to the tree }
            {
                // { Check we can add to the tree }
                if (Btree_levels >= max_Btree_levels)
                    throw new Exception("Maximum number of BTree levels exceeded");

                // { Move all blocks down one level }
                for (int level = max_Btree_levels - 1; level > 0; level--)
                    non_search_path_cache[level] = non_search_path_cache[level - 1];

                //  { increment the current level }
                tree_level = 1;

                //  Set up the root 
                BATree.Add(new Ttree_block_type()
                {
                    block_nbr = num_Btree_blocks,
                    Count = 0,
                    back_pointer = non_search_path_cache[1].block_nbr
                });

                non_search_path_cache[0] = BATree[num_Btree_blocks++];

                cur_block_element = -1;
                Btree_levels++;
            } // { if tree_level = 0 }

            // { Insert the middle element }
            if (!balancing)
            {
                // move the other elements down 
                if (cur_block_element < non_search_path_cache[tree_level - 1].Count - 1)
                {
                    Array.Copy(non_search_path_cache[tree_level - 1].element, cur_block_element + 1,
                               non_search_path_cache[tree_level - 1].element, cur_block_element + 2,
                               non_search_path_cache[tree_level - 1].Count - cur_block_element - 1);
                }
                non_search_path_cache[tree_level - 1].Count++;
            }

            //  { Insert element }
            non_search_path_cache[tree_level - 1].element[cur_block_element + 1] = middle_element;

            new_level = tree_level; 

            //  { Check the returned block is loaded }
            load_block(non_search_path_cache, tree_level, new_block_index);
        }

        public void add_entity_key(uint key, // : keytype;
                                   int ent_index) // : longint );
        {
            Tentity_element entity_key;
            int key_index = -1; // : element_index_type;
            int key_level = -1; // : level_index_type;
            int key_block = -1; // : block_index_type;

            // { Set up fields of entity key record }
            entity_key.key = key;
            entity_key.entity_index = ent_index;
            entity_key.next = nilptr;

            // { Find the position to insert the entity }
            if (!find_unique_key(key, non_search_path_cache, ent_index, ref key_level, ref key_block, ref key_index))
            {
                if (non_search_path_cache[0].Count == 0)
                {
                    // { Empty tree, so add in root }
                    non_search_path_cache[0].element[0] = entity_key;
                    non_search_path_cache[0].Count = 1;
                }
                else
                {
                    // Need to add the key at the position given 
                    //  Check if the indicated block is full 
                    if (non_search_path_cache[key_level].Count == elements_per_block)
                    {
                        split_block(key_level, key_block, key_index, ref key_level, ref key_block);

                        // After splitting the block re-perform the element insertion on the modified tree.
                        // This is to reduce complexiy of tracking the correct insertion position in the  modified tree
                        add_entity_key(key, ent_index);

                        return;
                    }

                    //  { Shift the elements after the current index to make room }
                    if (key_index <= non_search_path_cache[key_level].Count - 1)
                    {
                        Array.Copy(non_search_path_cache[key_level].element, key_index,
                                   non_search_path_cache[key_level].element, key_index + 1,
                                   non_search_path_cache[key_level].Count - key_index);
                    }

                    // { and add the new element }
                    non_search_path_cache[key_level].element[key_index] = entity_key;
                    non_search_path_cache[key_level].Count++;
                }
            }
        }

        public bool Initialize_BTree()
        {
            //{ Set up the B-tree data structure }
            Btree_levels = 1; //{ the root }

            //{ Set up the root }
            num_Btree_blocks = 1;
            BATree = new List<Ttree_block_type>
            {
                new Ttree_block_type()
                {
                    Count = 0,
                    back_pointer = nilptr,
                    block_nbr = 0
                }
            };

            non_search_path_cache[0] = BATree[0]; 

            tree_range_initialized = false;

            return true;
        }

        /// <summary>
        ///  Calculates the set of(up to 4) keys covering the rectangle bounding the  entity 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key_list"></param>
        void find_entity_keys(object data, ref key_list_rec key_list)
        {
            find_tri_keys(data, ref key_list);
        }

        void add_entity_keys(int ent_index, object data)
        {
            key_list_rec key_list = new key_list_rec();

            find_entity_keys(data, ref key_list);

            for (int k = 0; k < key_list.num_keys; k++)
                add_entity_key(key_list.key[k], ent_index);
        }

        public TriangleQuadTree()
        {
            Initialize_BTree();
            set_tree_range(-2000, -2000, 2000, 2000);
        }

        public void dump_tree(string Title)
        {
            using (StreamWriter writer = new StreamWriter(@"C:\Temp\DTree-csharp.txt"))
            {
                writer.WriteLine("===================================================================");
                writer.WriteLine(Title);
                writer.WriteLine($"root block: {non_search_path_cache[0].block_nbr} levels: {Btree_levels} Num blocks: {num_Btree_blocks}");

                writer.WriteLine($"Path cache: ");
                for (int I = 0; I < Btree_levels; I++)
                {
                    writer.Write($"[{I}={non_search_path_cache[I].block_nbr}]");
                }
                writer.WriteLine();

                long Total_count = 0;
                int block_index = 0;
                foreach (Ttree_block_type block in BATree)
                {
                    writer.WriteLine($"Block {block_index++} [{block.Count}] =  ({block.back_pointer})");
                    Total_count += block.Count;
                    for (int c = 0; c < block.Count; c++)
                        writer.Write($"{block.element[c].key} ({block.element[c].next}) ");
                    writer.WriteLine();
                    writer.WriteLine();
                }

                writer.WriteLine($"Total elements used: {Total_count}");
            }
        }
    }
}
