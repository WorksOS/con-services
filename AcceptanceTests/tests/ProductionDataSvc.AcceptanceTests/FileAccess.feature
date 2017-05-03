Feature: FileAccess
	I should be able to access (TCC) files.

Background: 
	Given the FileAccess service URI "/api/v1/files"

Scenario Outline: FileAccess - Download a file to current directory
	Given "<localPath>" does not already exist
	When I download "<fileName>" at "<path>" from "<filespaceId>" to "<localPath>"
	Then "<localPath>" should be present
	Examples: 
	| fileName                            | path        | filespaceId                           | localPath                           |
	| Large Sites Road - Trimble Road.ttm | /77561/1158 | u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01 | Large Sites Road - Trimble Road.ttm |

Scenario Outline: FileAccess - Download a file that already exists
	Given "<localPath>" already exists
	When I download "<fileName>" at "<path>" from "<filespaceId>" to "<localPath>"
	Then the response should have Code 0 and Message "success"
	Examples: 
	| fileName                            | path        | filespaceId                           | localPath                           |
	| Large Sites Road - Trimble Road.ttm | /77561/1158 | u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01 | Large Sites Road - Trimble Road.ttm |

Scenario Outline: FileAccess - Download a non-existent file
	When I download "<fileName>" at "<path>" from "<filespaceId>" to "<localPath>" expecting BadRequest response
	Then the response should have Code -3 and Message "Failed to download file from TCC"
	Examples: 
	| fileName       | path        | filespaceId                           | localPath |
	| IDontExist.ttm | /77561/1158 | u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01 | Random    |
  
Scenario Outline: FileAccess - Download a file to invalid directory
	When I download "<fileName>" at "<path>" from "<filespaceId>" to "<localPath>" expecting BadRequest response
	Then the response should have Code -3 and Message "Failed to download file from TCC"
	Examples: 
	| fileName                            | path        | filespaceId                           | localPath                                       |
	| Large Sites Road - Trimble Road.ttm | /77561/1158 | u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01 | IDontExist\\Large Sites Road - Trimble Road.ttm |