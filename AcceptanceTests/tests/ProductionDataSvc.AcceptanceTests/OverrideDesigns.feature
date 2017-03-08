Feature: OverrideDesigns
	I should be able to override designs for a machine, undo overriden design for a machine, filter by overriden design by getting elevation in a cell with multiple cell passes

Background: 
	Given  the edit data service URI "/api/v1/productiondata/edit", the get data service URI "/api/v1/productiondata/getedits", request parameter repository file "OverrideDesignParamRepository.json", result repository file "OverrideDesignResultRepository.json"

Scenario Outline: Override design, get list of overriden designs, undo override design
	When I override design with "<OverrideDesignsRequest>" supplying a request from a repository
	And I get the list of overriden designs supplying a request "<GetOverrideDesignsRequest>" from a repository and save it to "<AfterOverride>"
	And I undo design "<UndoDesignsRequest>" supplying a request from a repository
	And I get the list of overriden designs supplying a request "<GetOverrideDesignsRequest>" from a repository and save it to "<AfterUndo>"
	Then The override design list before undo should include design "<GetWithOverrideDesignListResponse>"
	And The override design list should include only initial designs in "<GetInitialDesignListResponse>" 

Examples: 
	| OverrideDesignsRequest           | GetOverrideDesignsRequest                   | UndoDesignsRequest                           | GetInitialDesignListResponse		                    | GetWithOverrideDesignListResponse					 |
	| DesignOverrideBlankInitialEvents | GetDesignOverrideBlankInitialEventsRequest  | UndoDesignOverrideBlankInitialEventsRequest  | DesignOverrideAfterUndoBlankInitialEventsResponse		| DesignOverrideBeforeUndoBlankInitialEventsResponse |
	| LayerOverrideBlankInitialEvents  | GetLayerOverrideBlankInitialEventsRequest   | UndoLayerOverrideBlankInitialEventsRequest   | LayerOverrideAfterUndoBlankInitialEventsResponse		| LayerOverrideBeforeUndoBlankInitialEventsResponse  |

Scenario Outline: Override design, filter data by overriden design getting cell datum elevation, undo override design
	Given the CellDatum service URI "/api/v1/productiondata/cells/datum", CellDatum request parameter repository file "OverrideDesignParamRepository.json", CellDatum result repository file "OverrideDesignResultRepository.json"
	When I override design with "<OverrideDesignsRequest>" supplying a request from a repository
	And I request Production Data Cell Datum supplying "<CellDatumRequest>" paramters from the repository
	Then the Production Data Cell Datum response should match "<ResultDatumAfterOverride>" result from the repository
	When I undo design "<UndoDesignsForCellDatum>" supplying a request from a repository
	And I request Production Data Cell Datum supplying "<CellDatumRequestAfterUndo>" paramters from the repository
	Then the Production Data Cell Datum response should match "<ResultDatumInitialResult>" result from the repository

Examples: 
	| OverrideDesignsRequest     | CellDatumRequest                  | ResultDatumAfterOverride              | UndoDesignsForCellDatum        | CellDatumRequestAfterUndo         | ResultDatumInitialResult    |
	| DesignOverrideForFiltering | CellDatumRequestFilteredByDesign4 | ResultDatumAfterOverrideResponse      | UndoDesignsForCellDatumRequest | CellDatumRequestFilteredByDesign4 | NoneDatumResult         |
	| LayerOverrideForFiltering  | CellDatumRequestFilteredByLayer15 | ResultDatumAfterLayerOverrideResponse | UndoLayerForCellDatumRequest   | CellDatumRequestFilteredByLayer15  | NoneDatumResult |
