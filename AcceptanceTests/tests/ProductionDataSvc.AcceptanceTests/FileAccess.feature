Feature: FileAccess
	I should be able to access (TCC) files.

Background: 
	Given the FileAccess service for file contents URI "/api/v1/rawfiles" 

Scenario Outline: FileAccess - Download a file and extract its contents to bytes 	
	When I download "<fileName>" at "<path>" from "<filespaceId>" expecting the downloaded file
	Then the file contents should be present
	Examples: 
	| fileName                            | path        | filespaceId                           |
	| Large Sites Road - Trimble Road.ttm | /77561/1158 | u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01 |

Scenario Outline: FileAccess - Download a non-existent file to extract its contents to bytes
	When I download "<fileName>" at "<path>" from "<filespaceId>" expecting no downloaded file and BadRequest response
	Then the response should have Code -3 and Message "Failed to download file from TCC" and no file contents should be present
	Examples: 
	| fileName       | path        | filespaceId                           |
	| IDontExist.ttm | /77561/1158 | u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01 |
