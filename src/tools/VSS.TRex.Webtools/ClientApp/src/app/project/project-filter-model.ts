/*
{"attributeFilter":{"siteModel":null,"startTime":"0001-01-01T00:00:00","endTime":"9999-12-31T23:59:59.9999999",
"machinesList":null,"designNameID":0,"vibeState":2,"machineDirection":2,"passTypeSet":0,"minElevationMapping":false,
"positioningTech":2,"gpsTolerance":65535,"gpsAccuracyIsInclusive":false,"gpsAccuracy":3,"gpsToleranceIsGreaterThan":false,
"elevationType":0,"gcsGuidanceMode":0,"returnEarliestFilteredCellPass":false,"elevationRangeLevel":1E+308,"elevationRangeOffset":1E+308,
"elevationRangeThickness":1E+308,"elevationRangeDesignID":"00000000-0000-0000-0000-000000000000","elevationRangeIsInitialised":false,
"elevationRangeIsLevelAndThicknessOnly":false,"elevationRangeTopElevationForCell":1E+308,"elevationRangeBottomElevationForCell":1E+308,
"layerState":2,"layerID":65535,"restrictFilteredDataToCompactorsOnly":false,"surveyedSurfaceExclusionList":[],"machineIDs":null,
"machineIDSet":null,"materialTemperatureMin":4096,"materialTemperatureMax":4096,"passcountRangeMin":0,"passcountRangeMax":0,
"lastRecordedCellPassSatisfiesFilter":true,"requestedGridDataType":0,"hasTimeFilter":false,"hasMachineFilter":false,"hasMachineDirectionFilter":false,
"hasDesignFilter":false,"hasVibeStateFilter":false,"hasLayerStateFilter":false,"hasMinElevMappingFilter":false,"hasElevationTypeFilter":false,
"hasGCSGuidanceModeFilter":false,"hasGPSAccuracyFilter":false,"hasGPSToleranceFilter":false,"hasPositioningTechFilter":false,"hasLayerIDFilter":false,
"hasElevationRangeFilter":false,"hasPassTypeFilter":false,"hasCompactionMachinesOnlyFilter":false,"hasTemperatureRangeFilter":false,
"filterTemperatureByLastPass":false,"hasPassCountRangeFilter":false,"anyFilterSelections":false,"anyMachineEventFilterSelections":false,
"anyNonMachineEventFilterSelections":false},
"spatialFilter":{"fence":{"points":[],"minX":10000000000.0,"maxX":-10000000000.0,"minY":10000000000.0,"maxY":-10000000000.0,"isRectangle":false, "isSquare":false,"hasVertices":false,"numVertices":0},
"alignmentFence":{"points":[],"minX":10000000000.0,"maxX":-10000000000.0,"minY":10000000000.0,"maxY":-10000000000.0,"isRectangle":false,"isSquare":false,"hasVertices":false,"numVertices":0},
"positionX":1E+308,"positionY":1E+308,"positionRadius":1E+308,"isSquare":false,
"overrideSpatialCellRestriction":{"minX":0,"minY":0,"maxX":0,"maxY":0,"isValidExtent":true,"sizeX":0,"sizeY":0},
"startStation":null,"endStation":null,"leftOffset":null,"rightOffset":null,"coordsAreGrid":false,"isSpatial":false,"isPositional":false,"isDesignMask":false,
"surfaceDesignMaskDesignUid":"00000000-0000-0000-0000-000000000000","isAlignmentMask":false,"alignmentMaskDesignUID":"00000000-0000-0000-0000-000000000000",
"hasSurfaceDesignMask":false,"hasSpatialOrPostionalFilters":false}}
*/

export class FencePoint {
  public X: number;
  public Y: number;

  public constructor(x: number, y: number) {
    this.X = x;
    this.Y = y;
  }
}

export class Fence {
  public Points: FencePoint[];
  public isRectangle: boolean = false;
}

export class AttributeFilter {
  public startTime: string;//"startTime": "0001-01-01T00:00:00"
  public endTime: string; //"endTime": "9999-12-31T23:59:59.9999999"

  public returnEarliestFilteredCellPass: boolean;
}

export class SpatialFilter {
  public coordsAreGrid: boolean = false;
  public isSpatial: boolean = false;
  public isPositional: boolean = false;
  public isSquare: boolean = false;
  public minX: number = 0;
  public minY: number = 0;
  public maxX: number = 0;
  public maxY: number = 0;
  public positionX:number = 0;
  public positionY: number = 0;
  public positionRadius: number = 0;

  public Fence: Fence = new Fence();
}

export class CombinedFilter {
  // The filter reponsible for selection of cell passes based on attribute filtering criteria related to cell passes
  public attributeFilter: AttributeFilter = new AttributeFilter();

  // The filter responsible for selection of cells based on spatial filtering criteria related to cell location
  public spatialFilter: SpatialFilter = new SpatialFilter();

//  public constructor() {
//    this.attributeFilter = new AttributeFilter();
//  }
}
