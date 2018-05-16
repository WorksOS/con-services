using System;
using System.Collections.Generic;
using System.Linq;

namespace VSS.TRex.Designs.TTM
{
    public class TriVertices : List<TriVertex>
    {

        private HashOrdinate HashOrdinate;
        private double MinHashOrdinate;
        private double MaxHashOrdinate;
        private List<TriVertex>[] HashArray;

        /// <summary>
        /// Computes the hash index for the given vertex location
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        /// <returns></returns>
        private int GetHashIndex(double X, double Y, double Z)
        {
            double HashValue = HashOrdinate == HashOrdinate.hoX ? X : Y;

            int result = (int)Math.Round((HashValue - MinHashOrdinate) / (MaxHashOrdinate - MinHashOrdinate) * HashArray.Count());

            if (result < 0)
            {
                result = 0;
            }
            else
            if (result >= HashArray.Length)
            {
                result = HashArray.Length - 1;
            }

            return result;
        }

        /// <summary>
        /// Locate a vertex based on a position using the vertex hash map
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        /// <param name="HashIndex"></param>
        /// <returns></returns>
        private TriVertex SearchForPoint(double X, double Y, double Z, out int HashIndex)
        {
            List<TriVertex> CollisionList;
            HashIndex = GetHashIndex(X, Y, Z);

            // Search the list of vertices hashed to this collision list
            CollisionList = HashArray[HashIndex];
            if (CollisionList == null)
            {
                return null;
            }

            foreach (TriVertex vertex in CollisionList)
            {
                if (vertex.IsEqual(X, Y, Z, SearchTolerance))
                {
                    return vertex;
                }
            }

            return null;
        }

        /// <summary>
        /// Create a new vertex from X, Y and Z coordinates
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        /// <returns></returns>
        protected virtual TriVertex CreateVertex(double X, double Y, double Z)
        {
            return new TriVertex(X, Y, Z);
        }

        /// <summary>
        /// The tolerance to be used when searching for points in the list of vertices. Expressed in meters.
        /// </summary>
        public double SearchTolerance { get; set; }

        /// <summary>
        /// Base no-arg constructor
        /// </summary>
        public TriVertices()
        {
        }

        public void GetLimits(ref double MinX, ref double MinY, ref double MinZ, ref double MaxX, ref double MaxY, ref double MaxZ)
        {
            MinX = 1E99; MinY = MinX; MinZ = MinX;
            MaxX = -MinX; MaxY = MaxX; MaxZ = MaxX;

            foreach (TriVertex vertex in this)
                vertex.AdjustLimits(ref MinX, ref MinY, ref MinZ, ref MaxX, ref MaxY, ref MaxZ);
        }

        /// <summary>
        /// Adds a new vertex to the TTM 
        /// </summary>
        /// <returns></returns>
        public TriVertex AddPoint(double X, double Y, double Z)
        {
            TriVertex Result = SearchForPoint(X, Y, Z, out int HashIdx);

            if (Result == null)
            {
                Result = CreateVertex(X, Y, Z);
                Add(Result);

                List<TriVertex> CollisionList = HashArray[HashIdx];

                // Add the item to the collision list for this HashIndex, creating a new one
                // if no hashed item has been added to this entry.
                if (CollisionList == null)
                {
                    CollisionList = new List<TriVertex>();
                    HashArray[HashIdx] = CollisionList;
                }

                CollisionList.Add(Result);
            }

            return Result;
        }

        /// <summary>
        /// Locates an existing vertex based on given X, Y, Z coordinates
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Z"></param>
        /// <returns></returns>
        public TriVertex FindPoint(double X, double Y, double Z)
        {
            return SearchForPoint(X, Y, Z, out int _ /*HashIdx*/);
        }

        /// <summary>
        /// Initialise the internal hashmap for storing vertices across the given geospatial range
        /// </summary>
        /// <param name="MinX"></param>
        /// <param name="MinY"></param>
        /// <param name="MaxX"></param>
        /// <param name="MaxY"></param>
        /// <param name="ExpectedPointCount"></param>
        public void InitPointSearch(double MinX, double MinY, double MaxX, double MaxY,
                                   int ExpectedPointCount)
        {
            // Use largest range to calculate hash index
            if ((MaxX - MinX) > (MaxY - MinY))
            {
                HashOrdinate = HashOrdinate.hoX;
                MinHashOrdinate = MinX;
                MaxHashOrdinate = MaxX;
            }
            else {

                HashOrdinate = HashOrdinate.hoY;
                MinHashOrdinate = MinY;
                MaxHashOrdinate = MaxY;
            }

            SearchTolerance = Consts.DefaultCoordinateResolution;
            HashArray = new List<TriVertex>[ExpectedPointCount * 2];
        }


        public void NumberVertices()
        {
            for (int i = 0; i < Count; i++)
            {
                this[i].Tag = i + 1;
            }
        }

        // procedure DumpVertexList(FileName : TFileName);
    }
}
