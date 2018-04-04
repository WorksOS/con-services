namespace VSS.VisionLink.Raptor.Cells
{
    /// <summary>
    /// Internal mapping of remaining GPSMode flag bits into usable fields in the cell pass structure
    /// The last 4 bits of GPSStore byte are flags. Zero based
    /// </summary>
    public enum GPSFlagBits
    {
        /// <summary>
        ///  bit 5 of 8, 0=fullpass,1=halfpass
        /// </summary>
        GPSSBitHalfPass = 4,

        /// <summary>
        /// Next two bits 6&7 will be read together value 0 = front, 1=rear, 2=track,3=wheel
        /// </summary>
        GPSSBit6 = 5,

        /// <summary>
        /// Next two bits 6&7 will be read together value 0 = front, 1=rear, 2=track,3=wheel
        /// </summary>
        GPSSBit7 = 6,

        /// <summary>
        /// Unused, spare bit
        /// </summary>
        GPSSBitSpare = 7
    };
}
