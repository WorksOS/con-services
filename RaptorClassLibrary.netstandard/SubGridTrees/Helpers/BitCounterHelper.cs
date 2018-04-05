namespace VSS.VisionLink.Raptor.SubGridTrees.Helpers
{
    public static class BitCounterHelper
    {
        /// <summary>
        /// Counts the number of set bits in a uint using some fancy bit-twiddling
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static uint CountSetBits(uint i)
        {
            i = i - ((i >> 1) & 0x55555555);
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }

        /// <summary>
        /// Internal test rig range of values checker for bit counting
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="failedItem"></param>
        /// <returns></returns>
        private static bool TestCountSetBitsRange(uint start, uint end, out uint failedItem)
        {
            failedItem = 0;

            for (uint i = start; i < end; i++)
            {
                uint result = CountSetBits(i);

                // Compare using dum method
                int count = 0;
                for (int j = 0; j < 32; j++)
                    if ((i & (1 << j)) != 0)
                        count++;

                if (result != count)
                {
                    failedItem = i;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Self test rig for ensuring the fancy bit-twiddling gets the right answers
        /// </summary>
        /// <param name="failedItem"></param>
        /// <returns></returns>
        public static bool TestCountSetBits(out uint failedItem)
        {
            return TestCountSetBitsRange(0, uint.MaxValue, out failedItem);
/*
 *            return TestCountSetBitsRange(0, 1000000, out failedItem) &&
                   TestCountSetBitsRange(1000000000, 1001000000, out failedItem) &&
                   TestCountSetBitsRange(2000000000, 2001000000, out failedItem) &&
                   TestCountSetBitsRange(3000000000, 3001000000, out failedItem) &&
                   TestCountSetBitsRange(uint.MaxValue - 1000000, uint.MaxValue, out failedItem); */
        }

    }
}
