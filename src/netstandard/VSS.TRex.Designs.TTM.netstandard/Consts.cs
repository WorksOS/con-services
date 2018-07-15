namespace VSS.TRex.Designs.TTM
{
    public static class Consts
    {
        public const int MaxSmallIntValue = 0x7FFF;
        public const byte TTMMajorVersion = 1;
        public const byte TTMMinorVersion = 0;

        public const string TTMFileIdentifier = "TNL TIN DTM FILE";

        public const string kDesignSubgridOverlaySubgridIndexHeader = "TTMSUBGRIDINDEX";
        public const int kDesignSubgridOverlaySubgridIndexVersion = 2; //= 3; ###RPW### See TFS item 16560
        public const string kDesignSubgridIndexFileExt = ".$DesignSubgridIndex$";

        public const string kCombinedSurfaceSubgridOverlaySubgridIndexHeader = "SpatialExistenceMap";
        public const int kCombinedSurfaceSubgridOverlaySubgridIndexVersion = 2; //= 3; ###RPW### See TFS item 16560

        public const int NoNeighbour = -1;
        public const int NullHashIndex = 0;
        public const int PointNotFound = 0;
        public const int NullTriangle = 0;
        public const int MaxStartPoints = 50;

        public const double DefaultCoordinateResolution = 0.000000001; // 0.000001 mm
        public const double DefaultElevationResolution = 0.000000001; // 0.000001 mm

        public const int keepFlagIndex = 0;
        public const ushort keepFlag = 1 << keepFlagIndex;             //Triangles inside an include boundary

        public const int delFlagIndex = 1;
        public const ushort delFlag = 1 << delFlagIndex;              //Used within UpdateBLines to determine if inside/outside boundary

        public const int side1IncFlagIndex = 2;
        public const ushort side1IncFlag = 1 << side1IncFlagIndex;    // side 1 makes up part of include boundary etc


        public const int side2IncFlagIndex = 3;
        public const ushort side2IncFlag = 1 << side2IncFlagIndex;

        public const int side3IncFlagIndex = 4;
        public const ushort side3IncFlag = 1 << side3IncFlagIndex;

        public const int side1OmitFlagIndex = 5;
        public const ushort side1OmitFlag = 1 << side1OmitFlagIndex;

        public const int side2OmitFlagIndex = 6;
        public const ushort side2OmitFlag = 1 << side2OmitFlagIndex;

        public const int side3OmitFlagIndex = 7;
        public const ushort side3OmitFlag = 1 << side3OmitFlagIndex;

        public const int side1StaticFlagIndex = 8;
        public const ushort side1StaticFlag = 1 << side1StaticFlagIndex;

        public const int side2StaticFlagIndex = 9;
        public const ushort side2StaticFlag = 1 << side2StaticFlagIndex;

        public const int side3StaticFlagIndex = 10;
        public const ushort side3StaticFlag = 1 << side3StaticFlagIndex;

        public const int deletedFlagIndex = 11;
        public const ushort deletedFlag = 1 << deletedFlagIndex;

        public const int IsDeletedFlagIndex = 12;
        public const ushort IsDeletedFlag = 1 << IsDeletedFlagIndex;     //Triangles lying outside an include boundary

        public const int IsDiscardedFlagIndex = 13;
        public const ushort IsDiscardedFlag = 1 << IsDiscardedFlagIndex;   //Triangle has been replaced by another/others

        public const int IsTriDrawnFlagIndex = 14;
        public const ushort IsTriDrawnFlag = 1 << IsTriDrawnFlagIndex;

        public const int IsContDrawnFlagIndex = 15;
        public const ushort IsContDrawnFlag = 1 << IsContDrawnFlagIndex;
    }
}
