using System;

namespace VSS.VisionLink.Raptor.Utilities
{
    /// <summary>
    /// Provides a set of useful functions for Range Truncation not supported in Math namespace
    /// </summary>
    public static class Range
    {
        /// <summary>
        /// Ensure Range for byte values
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static byte EnsureRange(byte value, byte min, byte max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Ensure Range for short values
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static short EnsureRange(short value, short min, short max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Ensure Range for ushort values
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static ushort EnsureRange(ushort value, ushort min, ushort max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Ensure Range for int values
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int EnsureRange(int value, int min, int max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Ensure Range for uint values
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static uint EnsureRange(uint value, uint min, uint max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Ensure Range for long values
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static long EnsureRange(long value, long min, long max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Ensure Range for float values
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float EnsureRange(float value, float min, float max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Ensure Range for long values
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static double EnsureRange(double value, double min, double max) => value < min ? min : value > max ? max : value;

        /// <summary>
        /// Test the given value is in the range of values given by min and max parameters
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool InRange(byte value, byte min, byte max) => value >= min && value <= max;

        /// <summary>
        /// Test the given value is in the range of values given by min and max parameters
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool InRange(short value, short min, short max) => value >= min && value <= max;

        /// <summary>
        /// Test the given value is in the range of values given by min and max parameters
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool InRange(ushort value, ushort min, ushort max) => value >= min && value <= max;

        /// <summary>
        /// Test the given value is in the range of values given by min and max parameters
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool InRange(int value, int min, int max) => value >= min && value <= max;

        /// <summary>
        /// Test the given value is in the range of values given by min and max parameters
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool InRange(uint value, uint min, uint max) => value >= min && value <= max;

        /// <summary>
        /// Test the given value is in the range of values given by min and max parameters
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool InRange(long value, long min, long max) =>  value >= min && value <= max;

        /// <summary>
        /// Test the given value is in the range of values given by min and max parameters
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool InRange(float value, float min, float max) =>  value >= min && value <= max;

        /// <summary>
        /// Test the given value is in the range of values given by min and max parameters
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool InRange(double value, double min, double max) => value >= min && value <= max;

        /// <summary>
        /// Test the given value is in the range of values given by min and max parameters
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool InRange(DateTime value, DateTime min, DateTime max) => value >= min && value<=max;

    }
}
