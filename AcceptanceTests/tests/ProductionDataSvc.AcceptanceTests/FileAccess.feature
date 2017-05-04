Feature: FileAccess
	I should be able to access (TCC) files.

Scenario Outline: FileAccess - Download a file to current directory
	Given the FileAccess service URI "/api/v1/files" 
	And "<localPath>" does not already exist
	When I download "<fileName>" at "<path>" from "<filespaceId>" to "<localPath>"
	Then "<localPath>" should be present
	Examples: 
	| fileName                            | path        | filespaceId                           | localPath                           |
	| Large Sites Road - Trimble Road.ttm | /77561/1158 | u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01 | Large Sites Road - Trimble Road.ttm |

Scenario Outline: FileAccess - Download a file that already exists
	Given the FileAccess service URI "/api/v1/files" 
	And "<localPath>" already exists
	When I download "<fileName>" at "<path>" from "<filespaceId>" to "<localPath>"
	Then the response should have Code 0 and Message "success"
	Examples: 
	| fileName                            | path        | filespaceId                           | localPath                           |
	| Large Sites Road - Trimble Road.ttm | /77561/1158 | u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01 | Large Sites Road - Trimble Road.ttm |

Scenario Outline: FileAccess - Download a non-existent file
	Given the FileAccess service URI "/api/v1/files"
	When I download "<fileName>" at "<path>" from "<filespaceId>" to "<localPath>" expecting BadRequest response
	Then the response should have Code -3 and Message "Failed to download file from TCC"
	Examples: 
	| fileName       | path        | filespaceId                           | localPath |
	| IDontExist.ttm | /77561/1158 | u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01 | Random    |
  
Scenario Outline: FileAccess - Download a file to invalid directory
	Given the FileAccess service URI "/api/v1/files"
	When I download "<fileName>" at "<path>" from "<filespaceId>" to "<localPath>" expecting BadRequest response
	Then the response should have Code -3 and Message "Failed to download file from TCC"
	Examples: 
	| fileName                            | path        | filespaceId                           | localPath                                       |
	| Large Sites Road - Trimble Road.ttm | /77561/1158 | u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01 | IDontExist\\Large Sites Road - Trimble Road.ttm |
	
Scenario Outline: FileAccess - Download a file and extract its contents to bytes 
	Given the FileAccess service for file content URI "/api/v1/rawfiles" 
	When I download "<fileName>" at "<path>" from "<filespaceId>" expecting the downloaded file
	Then the response should have Code 0 and Message "success" and the file contents should be present
	Examples: 
	| fileName                            | path        | filespaceId                           |
	| Large Sites Road - Trimble Road.ttm | /77561/1158 | u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01 |

Scenario Outline: FileAccess - Download a non-existent file to extract its contents to bytes
	Given the FileAccess service for file content URI "/api/v1/rawfiles" 
	When I download "<fileName>" at "<path>" from "<filespaceId>" expecting no downloaded file and BadRequest response
	Then the response should have Code -3 and Message "Failed to download file from TCC" and no file contents should be present
	Examples: 
	| fileName       | path        | filespaceId                           |
	| IDontExist.ttm | /77561/1158 | u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01 |
