export class AttributeFilter {
  public field2: string;
}

export class SpatialFilter {
  public field1: string;
}

export class CombinedFilter {
  // The filter reponsible for selection of cell passes based on attribute filtering criteria related to cell passes
  public attributeFilter: AttributeFilter;

/// The filter responsible for selection of cells based on spatial filtering criteria related to cell location
  public spatialFilter: SpatialFilter;
}
