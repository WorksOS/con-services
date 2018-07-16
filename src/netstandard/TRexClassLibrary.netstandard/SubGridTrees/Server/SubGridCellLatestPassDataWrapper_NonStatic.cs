using System;
using System.IO;
using VSS.TRex.Cells;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.SubGridTrees.Utilities;
using VSS.TRex.Types;

namespace VSS.TRex.SubGridTrees.Server
{
    public class SubGridCellLatestPassDataWrapper_NonStatic : SubGridCellLatestPassDataWrapperBase, ISubGridCellLatestPassDataWrapper
    {
        /// <summary>
        /// The array of 32x32 cells containing a cell pass representing the latest known values for a variety of cell attributes
        /// </summary>
        public CellPass[,] PassData = new CellPass[SubGridTree.SubGridTreeDimension, SubGridTree.SubGridTreeDimension];

        /// <summary>
        /// Implement the last pass indexer from the interface.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public CellPass this[int x, int y]
        {
            get
            {
                return PassData[x, y];
            }
            set
            {
                PassData[x, y] = value;
            }
        }

        /// <summary>
        /// Provides the 'NonStatic' behaviour for clearing the passes in the latest pass information
        /// </summary>
        public override void ClearPasses()
        {
            base.ClearPasses();

            SubGridUtilities.SubGridDimensionalIterator((x, y) => PassData[x, y].Clear());
        }

    public bool HasCCVData() => true;

      public bool HasRMVData() => true;

      public bool HasFrequencyData() => true;

      public bool HasAmplitudeData() => true;

      public bool HasGPSModeData() => true;

      public bool HasTemperatureData() => true;

      public bool HasMDPData() => true;

      public bool HasCCAData() => true;

      public override void Read(BinaryReader reader, byte [] buffer)
        {
            base.Read(reader, buffer);

            // Read in the latest call passes themselves
            SubGridUtilities.SubGridDimensionalIterator((i, j) => PassData[i, j].Read(reader));
        }

        /// <summary>
        /// ReadTime will read the Time from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public DateTime ReadTime(int Col, int Row)
        {
            return PassData[Col, Row].Time;
        }

        /// <summary>
        /// ReadHeiht will read the Height from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public float ReadHeight(int Col, int Row)
        {
            return PassData[Col, Row].Height;
        }

        /// <summary>
        /// ReadCCV will read the CCV from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public short ReadCCV(int Col, int Row)
        {
            return PassData[Col, Row].CCV;
        }

        /// <summary>
        /// ReadRMV will read the RMV from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public short ReadRMV(int Col, int Row)
        {
            return PassData[Col, Row].RMV;
        }

        /// <summary>
        /// ReadFrequency will read the Frequency from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public ushort ReadFrequency(int Col, int Row)
        {
            return PassData[Col, Row].Frequency;
        }

        // ReadAmplitude will read the Amplitude from the latest cell identified by the Row and Col
        public ushort ReadAmplitude(int Col, int Row)
        {
            return PassData[Col, Row].Amplitude;
        }

        /// <summary>
        /// ReadCCA will read the CCA from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public byte ReadCCA(int Col, int Row)
        {
            return PassData[Col, Row].CCA;
        }

        /// <summary>
        /// ReadGPSMode will read the GPSMode from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public GPSMode ReadGPSMode(int Col, int Row)
        {
            return PassData[Col, Row].gpsMode;
        }

        /// <summary>
        /// ReadMDP will read the MDP from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public short ReadMDP(int Col, int Row)
        {
            return PassData[Col, Row].MDP;
        }

        /// <summary>
        /// ReadTemperature will read the Temperature from the latest cell identified by the Row and Col
        /// </summary>
        /// <param name="Col"></param>
        /// <param name="Row"></param>
        /// <returns></returns>
        public ushort ReadTemperature(int Col, int Row)
        {
            return PassData[Col, Row].MaterialTemperature;
        }

        /// <summary>
        /// Writes the contents of the NonStatic latest passes using a supplied BinaryWriter
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="buffer"></param>
        public override void Write(BinaryWriter writer, byte [] buffer)
        {
            base.Write(writer, buffer);

            // Write out the latest call passes themselves
            SubGridUtilities.SubGridDimensionalIterator((i, j) => PassData[i, j].Write(writer));
        }
    }
}
