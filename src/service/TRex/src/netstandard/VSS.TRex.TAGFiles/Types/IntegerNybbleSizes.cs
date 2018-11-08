using System;

namespace VSS.TRex.TAGFiles.Types
{
    /// <summary>
    /// Supplies information regarding how many nybbles in the TAG file each integer field requires
    /// </summary>
    public static class IntegerNybbleSizes
    {
        /// <summary>
        /// Function to return the number of nybbles an integer TAG file requires to be read from the file
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static byte Nybbles(TAGDataType field)
        {
            switch (field)
            {
                case TAGDataType.t4bitInt:
                case TAGDataType.t4bitUInt:
                    return 1;

                case TAGDataType.t8bitInt:
                case TAGDataType.t8bitUInt:
                    return 2;

                case TAGDataType.t12bitInt:
                case TAGDataType.t12bitUInt:
                    return 3;

                case TAGDataType.t16bitInt:
                case TAGDataType.t16bitUInt:
                    return 4;

                case TAGDataType.t32bitInt:
                case TAGDataType.t32bitUInt:
                    return 8;

                default:
                    throw new ArgumentOutOfRangeException($"Unknown integer TAG field type {field}", "field");
            }
        }
    }
}
