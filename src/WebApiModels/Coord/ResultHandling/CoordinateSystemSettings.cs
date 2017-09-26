using System;
using VSS.Common.ResultsHandling;

namespace VSS.Productivity3D.WebApiModels.Coord.ResultHandling
{
  /// <summary>
  /// Coordinate system settings result object.
  /// </summary>
  ///    
  public class CoordinateSystemSettings : ContractExecutionResult
    {
        /// <summary>
        /// The coordinate system file name.
        /// </summary>
        /// 
        public string csName { get; private set; }

        /// <summary>
        /// The name of the coordinate system group.
        /// </summary>
        /// 
        public string csGroup { get; private set; }

        /// <summary>
        /// The coordinate system definition as an array of bytes.
        /// </summary>
        /// 
        public byte[] csib { get; private set; }

        /// <summary>
        /// The coordinate system datum name.
        /// </summary>
        /// 
        public string datumName { get; private set; }

        /// <summary>
        /// The flag indicates whether or not there are site calibration data in a coordinate system definition.
        /// </summary>
        /// 
        public bool siteCalibration { get; private set; }

        /// <summary>
        /// The coordinate system geoid model file name.
        /// </summary>
        /// 
        public string geoidFileName { get; private set; }

        /// <summary>
        /// The coordinate system geoid model name.
        /// </summary>
        /// 
        public string geoidName { get; private set; }

        /// <summary>
        /// The flag indicates whether or not there are datum grid data in a coordinate system definition.
        /// </summary>
        /// 
        public bool isDatumGrid { get; private set; }

        /// <summary>
        /// The flag indicates whether or not an assigned coordinate system projection is supported by the application.
        /// </summary>
        /// 
        public bool unsupportedProjection { get; private set; }

        /// <summary>
        /// The coordinate system latitude datum grid file name.
        /// </summary>
        /// 
        public string latitudeDatumGridFileName { get; private set; }

        /// <summary>
        /// The coordinate system longitude datum grid file name.
        /// </summary>
        /// 
        public string longitudeDatumGridFileName { get; private set; }

        /// <summary>
        /// The coordinate system height datum grid file name.
        /// </summary>
        /// 
        public string heightDatumGridFileName { get; private set; }

        /// <summary>
        /// The coordinate system shift grid file name.
        /// </summary>
        /// 
        public string shiftGridName { get; private set; }

        /// <summary>
        /// The coordinate system snake grid file name.
        /// </summary>
        /// 
        public string snakeGridName { get; private set; }

        /// <summary>
        /// The coordinate system vertical datum name.
        /// </summary>
        /// 
        public string verticalDatumName { get; private set; }

        /// <summary>
        /// Private constructor
        /// </summary>
        private CoordinateSystemSettings() 
        {}
    
        public static CoordinateSystemSettings CreateCoordinateSystemSettings
          (   
                string csName, 
                string csGroup, 
                byte[] csib, 
                string datumName, 
                bool siteCalibration, 
                string geoidFileName,
                string geoidName,
                bool isDatumGrid,
                string latitudeDatumGridFileName,
                string longitudeDatumGridFileName,
                string heightDatumGridFileName,
                string shiftGridName,
                string snakeGridName,
                string verticalDatumName,
                bool unsupportedProjection
            )
        {
            return new CoordinateSystemSettings
            {
                csName = csName, 
                csGroup = csGroup, 
                csib = csib, 
                datumName = datumName, 
                siteCalibration = siteCalibration, 
                geoidFileName = geoidFileName,
                geoidName = geoidName,
                isDatumGrid = isDatumGrid,
                latitudeDatumGridFileName = latitudeDatumGridFileName,
                longitudeDatumGridFileName = longitudeDatumGridFileName,
                heightDatumGridFileName = heightDatumGridFileName,
                shiftGridName = shiftGridName,
                snakeGridName = snakeGridName,
                verticalDatumName = verticalDatumName,
                unsupportedProjection = unsupportedProjection
            };
        }

        /// <summary>
        /// Validation method.
        /// </summary>
        public override string ToString()
        {
            return String.Format("csName:{0}, csGroup:{1}, csib:{2}, datumName:{3}, siteCalibration:{4}, geoidFileName:{5}, geoidName:{6}, isDatumGrid:{7}, latitudeDatumGridFileName:{8}, longitudeDatumGridFileName:{9}, heightDatumGridFileName:{10}, shiftGridName:{11}, snakeGridName:{12}, verticalDatumName:{13}, unsupportedProjection:{14}",
                                 this.csName, this.csGroup, this.csib.ToString(), this.datumName, this.siteCalibration, this.geoidFileName, this.geoidName, this.isDatumGrid, this.latitudeDatumGridFileName, this.longitudeDatumGridFileName, this.heightDatumGridFileName, this.shiftGridName, this.snakeGridName, this.verticalDatumName, this.unsupportedProjection);       
        }

    }
}