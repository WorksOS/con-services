Feature: CCASummary
  I should be able to request CCA summary.

Scenario Outline: CCASummary
  Given the service route "/api/v1/compaction/cca/summary" request repo "CCASummaryRequest.json" and result repo "CCASummaryResponse.json"
  When I POST with parameter "<ParameterName>" I expect response code <HttpCode>
  Then the response should match "<ResultName>" from the repository
  Examples: 
  | ParameterName        | ResultName           | HttpCode |
  | NoFilterAtAll        | NoFilterAtAll        | 200      |
  | Over33Under33On33    | Over33Under33On33    | 200      |
  | FilterByLayerAndTime | FilterByLayerAndTime | 200      |
