Feature: AssetService
   References : A. Contract Document - None
   Dependencies:  Internal -  Kafka Topic
       User Story 7349  : Asset Service (Master Data Manaetement)
	   User Story 21042 : Migrate Asset Service to IKVM Kafka Driver

#______________________________________________________________________________________________________________________________________________________
@Automated @Sanity @Positive
@AssetService @US7349 @US21042
Scenario: AssetService_CreateHappyPath
	Given AssetService Is Ready To Verify 'AssetService_CreateHappyPath'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Post Valid AssetServiceCreate Request
	Then The AssetCreated Details must be stored in MySql DB
	#And The Processed AssetServiceCreate Message must be available in Kafka topic

@Automated @Sanity @Positive
@AssetService @US7349 @US21042
Scenario: AssetService_UpdateHappyPath
	Given AssetService Is Ready To Verify 'AssetService_UpdateHappyPath'
	And AssetServiceUpdate Request Is Setup With Default Values
	When I Post Valid AssetServiceUpdate Request
	Then The AssetUpdated Details must be stored in MySql DB
	#And The Processed AssetServiceUpdate Message must be available in Kafka topic

@Automated @Sanity @Positive
@AssetService @US7349 @US21042
Scenario: AssetService_DeleteHappyPath

	Given AssetService Is Ready To Verify 'AssetService_DeleteHappyPath'
	And AssetServiceDelete Request Is Setup With Default Values
	When I Post Valid AssetServiceDelete Request
	Then The AssetDeleted Details must be stored in MySql DB
	#And The Processed AssetServiceDelete Message must be available in Kafka topic

@Automated @Regression @Positive
@AssetService @US7349 @US21042
Scenario Outline: AssetService_CreateValidMakeCode
	Given AssetService Is Ready To Verify 'AssetService_CreateValidMakeCode'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Set AssetServiceCreate MakeCode To '<MakeCode>'
	And I Post Valid AssetServiceCreate Request
	Then The AssetCreated Details must be stored in MySql DB
	#And The Processed AssetServiceCreate Message must be available in Kafka topic

	Examples:
		| Description | MakeCode |
		| InLowerCase | cat      |
		| InUpperCase | CAT      |

@Automated @Regression @Positive
@AssetService @US7349 @US21042
Scenario Outline: AssetService_CreateValidSerialNumber
	Given AssetService Is Ready To Verify 'AssetService_CreateValidSerialNumber'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Set AssetServiceCreate SerialNumber To '<SerialNumber>'
	And I Post Valid AssetServiceCreate Request
	Then The AssetCreated Details must be stored in MySql DB
	#And The Processed AssetServiceCreate Message must be available in Kafka topic

	Examples:
		| Description   | SerialNumber         |
		| SingleQuotes  | A0544 - '08 730EJ    |
		| SplCharacters | JEGMA CONST. & DEV'T |
		| DoubleQuotes  | TRK"                 |

@Automated @Regression @Positive
@AssetService @US7349 @US21042
Scenario Outline: AssetService_CreateValidAssetName
	Given AssetService Is Ready To Verify 'AssetService_CreateValidAssetName'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Set AssetServiceCreate AssetName To '<AssetName>'
	And I Post Valid AssetServiceCreate Request
	Then The AssetCreated Details must be stored in MySql DB
	#And The Processed AssetServiceCreate Message must be available in Kafka topic

	Examples:
		| Description   | AssetName              |
		| NULL          | NULL_NULL              |
		| EMPTY         | EMPTY_EMPTY            |
		| SingleQuotes  | A0544 - '08 730EJ      |
		| SplCharacters | JEGMA CONST. & DEV'T   |
		| DoubleQuotes  | 213-9632 15.75" BUCKET |

@Automated @Regression @Positive
@AssetService @US7349 @US21042
Scenario Outline: AssetService_CreateValidAssetType
	Given AssetService Is Ready To Verify 'AssetService_CreateValidAssetType'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Set AssetServiceCreate AssetType To '<AssetType>'
	And I Post Valid AssetServiceCreate Request
	Then The AssetCreated Details must be stored in MySql DB
	#And The Processed AssetServiceCreate Message must be available in Kafka topic

	Examples:
		| Description   | AssetType              |
		| NULL          | NULL_NULL              |
		| EMPTY         | EMPTY_EMPTY            |
		| SingleQuotes  | A0544 - '08 730EJ      |
		| SplCharacters | JEGMA CONST. & DEV'T   |
		| DoubleQuotes  | 213-9632 15.75" BUCKET |

@Automated @Regression @Positive
@AssetService @US7349 @US21042
Scenario Outline: AssetService_CreateValidModel
	Given AssetService Is Ready To Verify 'AssetService_CreateValidModel'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Set AssetServiceCreate Model To '<Model>'
	And I Post Valid AssetServiceCreate Request
	Then The AssetCreated Details must be stored in MySql DB
	#And The Processed AssetServiceCreate Message must be available in Kafka topic

	Examples:
		| Description   | Model                  |
		| NULL          | NULL_NULL              |
		| EMPTY         | EMPTY_EMPTY            |
		| SingleQuotes  | A0544 - '08 730EJ      |
		| SplCharacters | JEGMA CONST. & DEV'T   |
		| DoubleQuotes  | 213-9632 15.75" BUCKET |

@Automated @Regression @Positive
@AssetService @US7349 @US21042
Scenario: AssetService_CreateValidModelYear_NULL
	Given AssetService Is Ready To Verify 'AssetService_CreateValidModelYear_NULL'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Set AssetServiceCreate ModelYear To 'NULL_NULL'
	And I Post Valid AssetServiceCreate Request
	Then The AssetCreated Details must be stored in MySql DB
	#And The Processed AssetServiceCreate Message must be available in Kafka topic

@Automated @Regression @Positive
@AssetService @US7349 @US21042
Scenario Outline: AssetService_CreateValidEquipmentVIN
	Given AssetService Is Ready To Verify 'AssetService_CreateValidEquipmentVIN'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Set AssetServiceCreate EquipmentVIN To '<EquipmentVIN>'
	And I Post Valid AssetServiceCreate Request
	Then The AssetCreated Details must be stored in MySql DB
	#And The Processed AssetServiceCreate Message must be available in Kafka topic

	Examples:
		| Description   | EquipmentVIN           |
		| NULL          | NULL_NULL              |
		| EMPTY         | EMPTY_EMPTY            |
		| SingleQuotes  | A0544 - '08 730EJ      |
		| SplCharacters | JEGMA CONST. & DEV'T   |
		| DoubleQuotes  | 213-9632 15.75" BUCKET |

@Automated @Regression @Positive
@AssetService @US7349 @US21042
Scenario: AssetService_CreateValidIconKey_NULL
	Given AssetService Is Ready To Verify 'AssetService_CreateValidIconKey_NULL'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Set AssetServiceCreate IconKey To 'NULL_NULL'
	And I Post Valid AssetServiceCreate Request
	Then The AssetCreated Details must be stored in MySql DB
	#And The Processed AssetServiceCreate Message must be available in Kafka topic

@Automated @Regression @Positive
@AssetService @US7349 @US21042
Scenario: AssetService_CreateValidLegacyAssetID_NULL
	Given AssetService Is Ready To Verify 'AssetService_CreateValidLegacyAssetID_NULL'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Set AssetServiceCreate LegacyAssetID To 'NULL_NULL'
	And I Post Valid AssetServiceCreate Request
	Then The AssetCreated Details must be stored in MySql DB
	#And The Processed AssetServiceCreate Message must be available in Kafka topic

@Automated @Regression @Positive
@AssetService @US7349 @US21042
Scenario: AssetService_CreateValidOptionalFields_NULL
	Given AssetService Is Ready To Verify 'AssetService_CreateValidOptionalFields_NULL'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Post Valid AssetServiceCreate Request With The Below Values
		| AssetName | AssetType | Model     | ModelYear | EquipmentVIN | IconKey   | LegacyAssetID |
		| NULL_NULL | NULL_NULL | NULL_NULL | NULL_NULL | NULL_NULL    | NULL_NULL | NULL_NULL     |
	Then The AssetCreated Details must be stored in MySql DB
	#And The Processed AssetServiceCreate Message must be available in Kafka topic

@Automated @Regression @Positive
@AssetService @US7349 @US21042
Scenario: AssetService_UpdateValidAssetName_NULL
	Given AssetService Is Ready To Verify 'AssetService_UpdateValidAssetName_NULL'
	And AssetServiceUpdate Request Is Setup With Default Values
	When I Post Valid AssetServiceUpdate Request With The Below Values
		| AssetName | AssetType | Model | ModelYear | EquipmentVIN | IconKey | LegacyAssetID |
		| NULL_NULL | Loader    | A60   | 2010      | TestAsset123 | 30      | 109289009947  |
	Then The AssetUpdated Details must be stored in MySql DB
	#And The Processed AssetServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@AssetService @US7349 @US21042 @Bug22230
Scenario Outline: AssetService_UpdateValidAssetName
	Given AssetService Is Ready To Verify 'AssetService_UpdateValidAssetName'
	And AssetServiceUpdate Request Is Setup With Default Values
	When I Set AssetServiceUpdate AssetName To '<AssetName>'
	And I Post Valid AssetServiceUpdate Request
	Then The AssetUpdated Details must be stored in MySql DB
	#And The Processed AssetServiceUpdate Message must be available in Kafka topic

	Examples:
		| Description   | AssetName              |
		| EMPTY         | EMPTY_EMPTY            |
		| SingleQuotes  | A0544 - '08 730EJ      |
		| SplCharacters | JEGMA CONST. & DEV'T   |
		| DoubleQuotes  | 213-9632 15.75" BUCKET |

@Automated @Regression @Positive
@AssetService @US7349 @US21042
Scenario: AssetService_UpdateValidAssetType_NULL
	Given AssetService Is Ready To Verify 'AssetService_UpdateValidAssetType_NULL'
	And AssetServiceUpdate Request Is Setup With Default Values
	When I Post Valid AssetServiceUpdate Request With The Below Values
		| AssetName    | AssetType | Model | ModelYear | EquipmentVIN | IconKey | LegacyAssetID |
		| TestAsset256 | NULL_NULL | B89   | 2013      | TestAsset256 | 17      | 109289009947  |
	Then The AssetUpdated Details must be stored in MySql DB
	#And The Processed AssetServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@AssetService @US7349 @US21042 @Bug22230
Scenario Outline: AssetService_UpdateValidAssetType
	Given AssetService Is Ready To Verify 'AssetService_UpdateValidAssetType'
	And AssetServiceUpdate Request Is Setup With Default Values
	When I Set AssetServiceUpdate AssetType To '<AssetType>'
	And I Post Valid AssetServiceUpdate Request
	Then The AssetUpdated Details must be stored in MySql DB
	#And The Processed AssetServiceUpdate Message must be available in Kafka topic

	Examples:
		| Description   | AssetType              |
		| EMPTY         | EMPTY_EMPTY            |
		| SingleQuotes  | A0544 - '08 730EJ      |
		| SplCharacters | JEGMA CONST. & DEV'T   |
		| DoubleQuotes  | 213-9632 15.75" BUCKET |

@Automated @Regression @Positive
@AssetService @US7349 @US21042
Scenario: AssetService_UpdateValidModel_NULL
	Given AssetService Is Ready To Verify 'AssetService_UpdateValidModel_NULL'
	And AssetServiceUpdate Request Is Setup With Default Values
	When I Post Valid AssetServiceUpdate Request With The Below Values
		| AssetName    | AssetType | Model     | ModelYear | EquipmentVIN | IconKey | LegacyAssetID |
		| TestAsset892 | LOADER    | NULL_NULL | 2011      | TestAsset256 | 30      | 109289009947  |
	Then The AssetUpdated Details must be stored in MySql DB
	#And The Processed AssetServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@AssetService @US7349 @US21042 @Bug22230
Scenario Outline: AssetService_UpdateValidModel
	Given AssetService Is Ready To Verify 'AssetService_UpdateValidModel'
	And AssetServiceUpdate Request Is Setup With Default Values
	When I Set AssetServiceUpdate Model To '<Model>'
	And I Post Valid AssetServiceUpdate Request
	Then The AssetUpdated Details must be stored in MySql DB
	#And The Processed AssetServiceUpdate Message must be available in Kafka topic

	Examples:
		| Description   | Model                  |
		| EMPTY         | EMPTY_EMPTY            |
		| SingleQuotes  | A0544 - '08 730EJ      |
		| SplCharacters | JEGMA CONST. & DEV'T   |
		| DoubleQuotes  | 213-9632 15.75" BUCKET |

@Automated @Regression @Positive
@AssetService @US7349 @US21042
Scenario: AssetService_UpdateValidModelYear_NULL
	Given AssetService Is Ready To Verify 'AssetService_UpdateValidModelYear_NULL'
	And AssetServiceUpdate Request Is Setup With Default Values
	When I Post Valid AssetServiceUpdate Request With The Below Values
		| AssetName    | AssetType             | Model | ModelYear | EquipmentVIN | IconKey | LegacyAssetID |
		| TestAsset892 | MULTI TERRAIN LOADERS | B89   | NULL_NULL | TestAsset256 | 17      | 109289009947  |
	Then The AssetUpdated Details must be stored in MySql DB
	#And The Processed AssetServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@AssetService @US7349 @US21042
Scenario: AssetService_UpdateValidEquipmentVIN_NULL
	Given AssetService Is Ready To Verify 'AssetService_UpdateValidEquipmentVIN_NULL'
	And AssetServiceUpdate Request Is Setup With Default Values
	When I Post Valid AssetServiceUpdate Request With The Below Values
		| AssetName    | AssetType  | Model | ModelYear | EquipmentVIN | IconKey | LegacyAssetID |
		| TestAssetayt | PIPELAYERS | H88   | 2011      | NULL_NULL    | 30      | 109289009947  |
	Then The AssetUpdated Details must be stored in MySql DB
	#And The Processed AssetServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@AssetService @US7349 @US21042 @Bug22230
Scenario Outline: AssetService_UpdateValidEquipmentVIN
	Given AssetService Is Ready To Verify 'AssetService_UpdateValidEquipmentVIN'
	And AssetServiceUpdate Request Is Setup With Default Values
	When I Set AssetServiceUpdate EquipmentVIN To '<EquipmentVIN>'
	And I Post Valid AssetServiceUpdate Request
	Then The AssetUpdated Details must be stored in MySql DB
	#And The Processed AssetServiceUpdate Message must be available in Kafka topic

	Examples:
		| Description   | EquipmentVIN           |
		| EMPTY         | EMPTY_EMPTY            |
		| SingleQuotes  | A0544 - '08 730EJ      |
		| SplCharacters | JEGMA CONST. & DEV'T   |
		| DoubleQuotes  | 213-9632 15.75" BUCKET |

@Automated @Regression @Positive
@AssetService @US7349 @US21042
Scenario: AssetService_UpdateValidIconKey_Blank
	Given AssetService Is Ready To Verify 'AssetService_UpdateValidIconKey_NULL'
	And AssetServiceUpdate Request Is Setup With Default Values
	When I Post Valid AssetServiceUpdate Request With The Below Values
		| AssetName    | AssetType  | Model | ModelYear | EquipmentVIN | IconKey   | LegacyAssetID       |
		| TestAsset145 | PIPELAYERS | K90   | 2011      | TestAsset256 | NULL_NULL | 6991574262784860160 |
	Then The AssetUpdated Details must be stored in MySql DB
	#And The Processed AssetServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@AssetService @US7349 @US21042
Scenario: AssetService_UpdateValidLegacyAssetID_Blank
	Given AssetService Is Ready To Verify 'AssetService_UpdateValidLegacyAssetID_NULL'
	And AssetServiceUpdate Request Is Setup With Default Values
	When I Post Valid AssetServiceUpdate Request With The Below Values
		| AssetName    | AssetType  | Model | ModelYear | EquipmentVIN | IconKey | LegacyAssetID |
		| TestAsset145 | PIPELAYERS | K90   | 2011      | TestAsset256 | 25      | NULL_NULL     |
	Then The AssetUpdated Details must be stored in MySql DB
	#And The Processed AssetServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@AssetService @US83964
Scenario: AssetService_CreateAssetWithValidMakeCode
	Given AssetService Is Ready To Verify 'AssetService_CreateAssetWithValidMakeCode'
	And AssetServiceCreate Request Is Setup With Default Values and Valid MakeCode
	When I Post Valid AssetServiceCreate Request
	Then The AssetCreated Details must be stored in MySql DB
	#And The Processed AssetServiceUpdate Message must be available in Kafka topic

@Automated @Regression @Positive
@AssetService @US83964
Scenario Outline:  AssetService_CreateAssetWithValidObjectType
	Given AssetService Is Ready To Verify 'AssetService_CreateAssetWithValidObjectType'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Set AssetServiceCreate ObjectType To '<ObjectType>'
	And I Post Valid AssetServiceCreate Request
	Then The AssetCreated Details must be stored in MySql DB

	#And The Processed AssetServiceCreate Message must be available in Kafka topic
	Examples:
		| Description              | ObjectType |
		| ObjectType_Empty         | EMPTY      |
		| ObjectType_Null          | NULL       |
		| ObjectType_MinCharacter  | MinValue   |
		| ObjectType_MaxCharacters | MaxValue   |
		| ObjectType_ValidValue    | Valid      |

@Automated @Regression @Positive
@AssetService @US83964
Scenario Outline:  AssetService_CreateAssetWithValidCategory
	Given AssetService Is Ready To Verify 'AssetService_CreateAssetWithValidCategory'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Set AssetServiceCreate Category To '<Category>' 
	And I Post Valid AssetServiceCreate Request
	Then The AssetCreated Details must be stored in MySql DB

	#And The Processed AssetServiceCreate Message must be available in Kafka topic
	Examples:
		| Description            | Category | 
		| Category_Empty         | EMPTY    |
		| Category_Null          | NULL     |
		| Category_MinCharacter  | MinValue |
		| Category_MaxCharacters | MaxValue |
		| Category_ValidValue    | Valid    |


@Automated @Regression @Positive
@AssetService @US83964
Scenario Outline:  AssetService_CreateAssetWithValidProject
	Given AssetService Is Ready To Verify 'AssetService_CreateAssetWithValidProject'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Set AssetServiceCreate Project To '<Project>'
	And I Post Valid AssetServiceCreate Request
	Then The AssetCreated Details must be stored in MySql DB

	#And The Processed AssetServiceCreate Message must be available in Kafka topic
	Examples:
		| Description           | Project  |
		| Project_Empty         | EMPTY    |
		| Project_Null          | NULL     |
		| Project_MinCharacter  | MinValue |
		| Project_MaxCharacters | MaxValue |
		| Project_ValidValue    | Valid    |

@Automated @Regression @Positive
@AssetService @US83964
Scenario Outline:  AssetService_CreateAssetWithValidProjectStatus
	Given AssetService Is Ready To Verify 'AssetService_CreateAssetWithValidProjectStatus'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Set AssetServiceCreate ProjectStatus To '<ProjectStatus>'
	And I Post Valid AssetServiceCreate Request
	Then The AssetCreated Details must be stored in MySql DB

	#And The Processed AssetServiceCreate Message must be available in Kafka topic
	Examples:
		| Description                 | ProjectStatus |
		| ProjectStatus_Empty         | EMPTY         |
		| ProjectStatus_Null          | NULL          |
		| ProjectStatus_MinCharacter  | MinValue      |
		| ProjectStatus_MaxCharacters | MaxValue      |
		| ProjectStatus_ValidValue    | Valid         |

@Automated @Regression @Positive
@AssetService @US83964
Scenario Outline:  AssetService_CreateAssetWithValidSortField
	Given AssetService Is Ready To Verify 'AssetService_CreateAssetWithValidSortField'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Set AssetServiceCreate SortField To '<SortField>'
	And I Post Valid AssetServiceCreate Request
	Then The AssetCreated Details must be stored in MySql DB

	#And The Processed AssetServiceCreate Message must be available in Kafka topic
	Examples:
		| Description             | SortField |
		| SortField_Empty         | EMPTY     |
		| SortField_Null          | NULL      |
		| SortField_MinCharacter  | MinValue  |
		| SortField_MaxCharacters | MaxValue  |
		| SortField_ValidValue    | Valid     |

@Automated @Regression @Positive
@AssetService @US83964
Scenario Outline:  AssetService_CreateAssetWithValidSource
	Given AssetService Is Ready To Verify 'AssetService_CreateAssetWithValidSource'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Set AssetServiceCreate Source To '<Source>'
	And I Post Valid AssetServiceCreate Request
	Then The AssetCreated Details must be stored in MySql DB

	#And The Processed AssetServiceCreate Message must be available in Kafka topic
	Examples:
		| Description          | Source   |
		| Source_Empty         | EMPTY    |
		| Source_Null          | NULL     |
		| Source_MinCharacter  | MinValue |
		| Source_MaxCharacters | MaxValue |
		| Source_ValidValue    | Valid    |

@Automated @Regression @Positive
@AssetService @US83964
Scenario Outline:  AssetService_CreateAssetWithValidUserEnteredRuntimeHours
	Given AssetService Is Ready To Verify 'AssetService_CreateAssetWithValidUserEnteredRuntimeHours'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Set AssetServiceCreate UserEnteredRuntimeHours To '<UserEnteredRuntimeHours>'
	And I Post Valid AssetServiceCreate Request
	Then The AssetCreated Details must be stored in MySql DB

	#And The Processed AssetServiceCreate Message must be available in Kafka topic
	Examples:
		| Description                           | UserEnteredRuntimeHours |
		| UserEnteredRuntimeHours_Empty         | EMPTY                   |
		| UserEnteredRuntimeHours_Null          | NULL                    |
		| UserEnteredRuntimeHours_MinCharacter  | MinValue                |
		| UserEnteredRuntimeHours_MaxCharacters | MaxValue                |
		| UserEnteredRuntimeHours_ValidValue    | Valid                   |

@Automated @Regression @Positive
@AssetService @US83964
Scenario Outline:  AssetService_CreateAssetWithValidClassification
	Given AssetService Is Ready To Verify 'AssetService_CreateAssetWithValidClassification'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Set AssetServiceCreate Classification To '<Classification>'
	And I Post Valid AssetServiceCreate Request
	Then The AssetCreated Details must be stored in MySql DB

	#And The Processed AssetServiceCreate Message must be available in Kafka topic
	Examples:
		| Description                  | Classification |
		| Classification_Empty         | EMPTY          |
		| Classification_Null          | NULL           |
		| Classification_MinCharacter  | MinValue       |
		| Classification_MaxCharacters | MaxValue       |
		| Classification_ValidValue    | Valid          |

@Automated @Regression @Positive
@AssetService @US83964
Scenario Outline: AssetService_UpdateObjectTypeHappyPath
	Given AssetService Is Ready To Verify 'AssetService_UpdateObjectTypeHappyPath'
	And AssetServiceUpdate Request Is Setup With Default Values
	When I Set AssetServiceUpate ObjectType To '<ObjectType>'
	And I Post Valid AssetServiceUpdate Request
	Then The AssetUpdated Details must be stored in MySql DB

	#And The Processed AssetServiceUpdate Message must be available in Kafka topic
	Examples:
		| Description              | ObjectType |
		| ObjectType_Empty         | EMPTY      |
		| ObjectType_Null          | NULL       |
		| ObjectType_MinCharacter  | MinValue   |
		| ObjectType_MaxCharacters | MaxValue   |
		| ObjectType_ValidValue    | Valid      |

@Automated @Regression @Positive
@AssetService @US83964
Scenario Outline: AssetService_UpdateCategoryHappyPath
	Given AssetService Is Ready To Verify 'AssetService_UpdateCategoryHappyPath'
	And AssetServiceUpdate Request Is Setup With Default Values
	When I Set AssetServiceUpate Category To '<Category>'
	And I Post Valid AssetServiceUpdate Request
	Then The AssetUpdated Details must be stored in MySql DB

	#And The Processed AssetServiceUpdate Message must be available in Kafka topic
	Examples:
		| Description            | Category |
		| Category_Empty         | EMPTY    |
		| Category_Null          | NULL     |
		| Category_MinCharacter  | MinValue |
		| Category_MaxCharacters | MaxValue |
		| Category_ValidValue    | Valid    |

@Automated @Regression @Positive
@AssetService @US83964
Scenario Outline: AssetService_UpdateProjectHappyPath
	Given AssetService Is Ready To Verify 'AssetService_UpdateProjectHappyPath'
	And AssetServiceUpdate Request Is Setup With Default Values
	When I Set AssetServiceUpate Project To '<Project>'
	And I Post Valid AssetServiceUpdate Request
	Then The AssetUpdated Details must be stored in MySql DB

	#And The Processed AssetServiceUpdate Message must be available in Kafka topic
	Examples:
		| Description           | Project  |
		| Project_Empty         | EMPTY    |
		| Project_Null          | NULL     |
		| Project_MinCharacter  | MinValue |
		| Project_MaxCharacters | MaxValue |
		| Project_ValidValue    | Valid    |

@Automated @Regression @Positive
@AssetService @US83964
Scenario Outline: AssetService_UpdateProjectStatusHappyPath
	Given AssetService Is Ready To Verify 'AssetService_UpdateProjectStatusHappyPath'
	And AssetServiceUpdate Request Is Setup With Default Values
	When I Set AssetServiceUpate ProjectStatus To '<ProjectStatus>'
	And I Post Valid AssetServiceUpdate Request
	Then The AssetUpdated Details must be stored in MySql DB

	#And The Processed AssetServiceUpdate Message must be available in Kafka topic
	Examples:
		| Description                 | ProjectStatus |
		| ProjectStatus_Empty         | EMPTY         |
		| ProjectStatus_Null          | NULL          |
		| ProjectStatus_MinCharacter  | MinValue      |
		| ProjectStatus_MaxCharacters | MaxValue      |
		| ProjectStatus_ValidValue    | Valid         |

@Automated @Regression @Positive
@AssetService @US83964
Scenario Outline: AssetService_UpdateSortFieldHappyPath
	Given AssetService Is Ready To Verify 'AssetService_UpdateSortFieldHappyPath'
	And AssetServiceUpdate Request Is Setup With Default Values
	When I Set AssetServiceUpate SortField To '<SortField>'
	And I Post Valid AssetServiceUpdate Request
	Then The AssetUpdated Details must be stored in MySql DB

	#And The Processed AssetServiceUpdate Message must be available in Kafka topic
	Examples:
		| Description             | SortField |
		| SortField_Empty         | EMPTY     |
		| SortField_Null          | NULL      |
		| SortField_MinCharacter  | MinValue  |
		| SortField_MaxCharacters | MaxValue  |
		| SortField_ValidValue    | Valid     |

@Automated @Regression @Positive
@AssetService @US83964
Scenario Outline: AssetService_UpdateSourceHappyPath
	Given AssetService Is Ready To Verify 'AssetService_UpdateSourceHappyPath'
	And AssetServiceUpdate Request Is Setup With Default Values
	When I Set AssetServiceUpate Source To '<Source>'
	And I Post Valid AssetServiceUpdate Request
	Then The AssetUpdated Details must be stored in MySql DB

	#And The Processed AssetServiceUpdate Message must be available in Kafka topic
	Examples:
		| Description          | Source   |
		| Source_Empty         | EMPTY    |
		| Source_Null          | NULL     |
		| Source_MinCharacter  | MinValue |
		| Source_MaxCharacters | MaxValue |
		| Source_ValidValue    | Valid    |

@Automated @Regression @Positive
@AssetService @US83964
Scenario Outline: AssetService_UpdateUserEnteredRuntimeHoursHappyPath
	Given AssetService Is Ready To Verify 'AssetService_UpdateUserEnteredRuntimeHoursHappyPath'
	And AssetServiceUpdate Request Is Setup With Default Values
	When I Set AssetServiceUpate UserEnteredRuntimeHours To '<UserEnteredRuntimeHours>'
	And I Post Valid AssetServiceUpdate Request
	Then The AssetUpdated Details must be stored in MySql DB

	#And The Processed AssetServiceUpdate Message must be available in Kafka topic
	Examples:
		| Description                           | UserEnteredRuntimeHours |
		| UserEnteredRuntimeHours_Empty         | EMPTY                   |
		| UserEnteredRuntimeHours_Null          | NULL                    |
		| UserEnteredRuntimeHours_MinCharacter  | MinValue                |
		| UserEnteredRuntimeHours_MaxCharacters | MaxValue                |
		| UserEnteredRuntimeHours_ValidValue    | Valid                   |

@Automated @Regression @Positive
@AssetService @US83964
Scenario Outline: AssetService_UpdateClassificationHappyPath
	Given AssetService Is Ready To Verify 'AssetService_UpdateClassificationHappyPath'
	And AssetServiceUpdate Request Is Setup With Default Values
	When I Set AssetServiceUpate Classification To '<Classification>'
	And I Post Valid AssetServiceUpdate Request
	Then The AssetUpdated Details must be stored in MySql DB

	#And The Processed AssetServiceUpdate Message must be available in Kafka topic
	Examples:
		| Description                  | Classification |
		| Classification_Empty         | EMPTY          |
		| Classification_Null          | NULL           |
		| Classification_MinCharacter  | MinValue       |
		| Classification_MaxCharacters | MaxValue       |
		| Classification_ValidValue    | Valid          |

@Automated @Regression @Negative
@AssetService @US83964
Scenario: AssetService_CreateAssetWithInValidMakeCode
	Given AssetService Is Ready To Verify 'AssetService_CreateAssetWithValidMakeCode'
	And AssetServiceCreate Request Is Setup With Default Values and Invalid MakeCode
	When I Post Valid AssetServiceCreate Request
	Then AssetService Response With 'ERR_InvalidMakeCode' Should Be Returned

@Automated @Regression @Negative
@AssetService @US7349
Scenario: AssetService_CreateInvalid_DuplicateAssetUID
	Given AssetService Is Ready To Verify '<Description>'
	And AssetServiceCreate Request Is Setup With Invalid Default Values
	When I Set Invalid AssetServiceCreate AssetUID To Duplicate AssetUID
	And I Post Invalid AssetServiceCreate Request
	Then AssetService Response With 'ERR_Duplicate' Should Be Returned

@Automated @Regression @Negative
@AssetService @US7349
Scenario: AssetService_UpdateInvalid_NonExistingAsset
	Given AssetService Is Ready To Verify '<Description>'
	And AssetServiceUpdate Request Is Setup With Invalid Default Values
	When I Post Invalid AssetServiceUpdate Request
	Then AssetService Response With 'ERR_NonExistingAsset' Should Be Returned

@Automated @Regression @Negative
@AssetService @US7349
Scenario: AssetService_DeleteInvalid_NonExistingAsset
	Given AssetService Is Ready To Verify '<Description>'
	And AssetServiceDelete Request Is Setup With Invalid Default Values
	When I Post Invalid AssetServiceDelete Request
	Then AssetService Response With 'ERR_NonExistingAsset' Should Be Returned

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_CreateInvalidAssetUID
	Given AssetService Is Ready To Verify '<Description>'
	And AssetServiceCreate Request Is Setup With Invalid Default Values
	When I Set Invalid AssetServiceCreate AssetUID To '<AssetUID>'
	And I Post Invalid AssetServiceCreate Request
	Then AssetService Response With '<ErrorMessage>' Should Be Returned

	Examples:
		| Description        | AssetUID                             | ErrorMessage        |
		| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_AssetUIDInvalid |
		| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_AssetUIDInvalid |
		| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_AssetUIDInvalid |
		| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_AssetUIDInvalid |
		| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_AssetUIDInvalid |
		| NULL               | NULL_NULL                            | ERR_AssetUIDInvalid |

@Automated @Regression @Negative
@AssetService @US7349
Scenario: AssetService_CreateInvalidSerialNumber_NULL
	Given AssetService Is Ready To Verify '<Description>'
	And AssetServiceCreate Request Is Setup With Invalid Default Values
	When I Set Invalid AssetServiceCreate SerialNumber To 'NULL_NULL'
	And I Post Invalid AssetServiceCreate Request
	Then AssetService Response With 'ERR_SerialNumberInvalid' Should Be Returned

@Automated @Regression @Negative
@AssetService @US7349
Scenario: AssetService_CreateInvalidMakeCode_NULL
	Given AssetService Is Ready To Verify '<Description>'
	And AssetServiceCreate Request Is Setup With Invalid Default Values
	When I Set Invalid AssetServiceCreate MakeCode To 'NULL_NULL'
	And I Post Invalid AssetServiceCreate Request
	Then AssetService Response With 'ERR_MakeCodeInvalid' Should Be Returned

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_CreateInvalidIconKey
	Given AssetService Is Ready To Verify '<Description>'
	And AssetServiceCreate Request Is Setup With Invalid Default Values
	When I Set Invalid AssetServiceCreate IconKey To '<IconKey>'
	And I Post Invalid AssetServiceCreate Request
	Then AssetService Response With '<ErrorMessage>' Should Be Returned

	Examples:
		| Description   | IconKey | ErrorMessage       |
		| String        | abc     | ERR_IconKeyInvalid |
		| ContainsSpace | 1 2     | ERR_IconKeyInvalid |

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_CreateInvalidModelYear
	Given AssetService Is Ready To Verify '<Description>'
	And AssetServiceCreate Request Is Setup With Invalid Default Values
	When I Set Invalid AssetServiceCreate ModelYear To '<ModelYear>'
	And I Post Invalid AssetServiceCreate Request
	Then AssetService Response With '<ErrorMessage>' Should Be Returned

	Examples:
		| Description   | ModelYear | ErrorMessage         |
		| String        | abc       | ERR_ModelYearInvalid |
		| ContainsSpace | 1 2       | ERR_ModelYearInvalid |

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_CreateInvalidActionUTC
	Given AssetService Is Ready To Verify '<Description>'
	And AssetServiceCreate Request Is Setup With Invalid Default Values
	When I Set Invalid AssetServiceCreate ActionUTC To '<ActionUTC>'
	And I Post Invalid AssetServiceCreate Request
	Then AssetService Response With '<ErrorMessage>' Should Be Returned

	Examples:
		| Description   | ActionUTC          | ErrorMessage         |
		| String        | abc                | ERR_ActionUTCInvalid |
		| ContainsSpace | 1 2 3 4            | ERR_ActionUTCInvalid |
		| NotInDateTime | 2015-2-13-14-22:02 | ERR_ActionUTCInvalid |

@Automated @Regression @Negative
@AssetService @US7349
Scenario: AssetService_UpdateInvalidOptionalFields_Blank
	Given AssetService Is Ready To Verify 'AssetService_UpdateValidOptionalFields_Blank'
	And AssetServiceUpdate Request Is Setup With Invalid Default Values
	When I Post Invalid AssetServiceUpdate Request With The Below Values
		| AssetName | AssetType | Model     | ModelYear | EquipmentVIN | IconKey   |
		| NULL_NULL | NULL_NULL | NULL_NULL | NULL_NULL | NULL_NULL    | NULL_NULL |
	Then AssetService Response With 'ERR_AssetUpdateInvalid' Should Be Returned

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_UpdateInvalidAssetUID
	Given AssetService Is Ready To Verify '<Description>'
	And AssetServiceUpdate Request Is Setup With Invalid Default Values
	When I Set Invalid AssetServiceUpdate AssetUID To '<AssetUID>'
	And I Post Invalid AssetServiceUpdate Request
	Then AssetService Response With '<ErrorMessage>' Should Be Returned

	Examples:
		| Description        | AssetUID                             | ErrorMessage        |
		| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_AssetUIDInvalid |
		| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_AssetUIDInvalid |
		| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_AssetUIDInvalid |
		| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_AssetUIDInvalid |
		| SplChar            | 9AB056CA3-514-E411-8AFE-24FD5231*B1F | ERR_AssetUIDInvalid |
		| NULL               | NULL_NULL                            | ERR_AssetUIDInvalid |

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_UpdateInvalidModelYear
	Given AssetService Is Ready To Verify '<Description>'
	And AssetServiceUpdate Request Is Setup With Invalid Default Values
	When I Set Invalid AssetServiceUpdate ModelYear To '<ModelYear>'
	And I Post Invalid AssetServiceUpdate Request
	Then AssetService Response With '<ErrorMessage>' Should Be Returned

	Examples:
		| Description   | ModelYear                               | ErrorMessage         |
		| String        | abc                                     | ERR_ModelYearInvalid |
		| ContainsSpace | 1 2                                     | ERR_ModelYearInvalid |
		| SplChar       | &^9AB056CA.,-3514-E411-8AF_24FD5231FB1F | ERR_ModelYearInvalid |

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_UpdateInvalidIconKey
	Given AssetService Is Ready To Verify '<Description>'
	And AssetServiceUpdate Request Is Setup With Invalid Default Values
	When I Set Invalid AssetServiceUpdate IconKey To '<IconKey>'
	And I Post Invalid AssetServiceUpdate Request
	Then AssetService Response With '<ErrorMessage>' Should Be Returned

	Examples:
		| Description   | IconKey | ErrorMessage       |
		| String        | abc     | ERR_IconKeyInvalid |
		| ContainsSpace | 1 2     | ERR_IconKeyInvalid |

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_UpdateInvalidActionUTC
	Given AssetService Is Ready To Verify '<Description>'
	And AssetServiceUpdate Request Is Setup With Invalid Default Values
	When I Set Invalid AssetServiceUpdate ActionUTC To '<ActionUTC>'
	And I Post Invalid AssetServiceUpdate Request
	Then AssetService Response With '<ErrorMessage>' Should Be Returned

	Examples:
		| Description   | ActionUTC          | ErrorMessage         |
		| String        | abc                | ERR_ActionUTCInvalid |
		| ContainsSpace | 1 2 3 4            | ERR_ActionUTCInvalid |
		| NotInDateTime | 2015-2-13-14-22:02 | ERR_ActionUTCInvalid |

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_DeleteInvalidAssetUID
	Given AssetService Is Ready To Verify '<Description>'
	And AssetServiceDelete Request Is Setup With Invalid Default Values
	When I Set Invalid AssetServiceDelete AssetUID To '<AssetUID>'
	And I Post Invalid AssetServiceDelete Request
	Then AssetService Response With '<ErrorMessage>' Should Be Returned

	Examples:
		| Description        | AssetUID                             | ErrorMessage        |
		| InvalidLength      | c9ead99b-eea3-4605-92e2-1e6497b773   | ERR_AssetUIDInvalid |
		| ContainsSpace      | c9ead99b-eea3 4605-92e2-1e6497b77369 | ERR_AssetUIDInvalid |
		| ContainsUnderScore | 9AB056CA_3514_E411_8AFE_24FD5231FB1F | ERR_AssetUIDInvalid |
		| NotInGUID          | 9AB056CA3-514-E411-8AFE-24FD5231FB1F | ERR_AssetUIDInvalid |
		| NULL               | NULL_NULL                            | ERR_AssetUIDInvalid |

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_DeleteInvalidActionUTC
	Given AssetService Is Ready To Verify '<Description>'
	And AssetServiceDelete Request Is Setup With Invalid Default Values
	When I Set Invalid AssetServiceDelete ActionUTC To '<ActionUTC>'
	And I Post Invalid AssetServiceDelete Request
	Then AssetService Response With '<ErrorMessage>' Should Be Returned

	Examples:
		| Description   | ActionUTC          | ErrorMessage  |
		| String        | abc                | ERR_ActionUTC |
		| ContainsSpace | 2015 13 15         | ERR_ActionUTC |
		| NotInDateTime | 2015-2-13-14-22:02 | ERR_ActionUTC |

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_CreateInvalidContentType
	Given AssetService Is Ready To Verify '<Description>'
	And AssetServiceCreate Request Is Setup With Invalid Default Values
	When I Post AssetServiceCreate Request With Invalid ContentType '<Value>'
	Then AssetService Response With '<ErrorMessage>' Should Be Returned

	Examples:
		| Description | Value | ErrorMessage           |
		| HTML        | HTML  | ERR_InvalidContentType |
		| XML         | XML   | ERR_InvalidContentType |

@Automated @Regression @Negative
@AssetService @US7349
Scenario Outline: AssetService_UpdateInvalidContentType
	Given AssetService Is Ready To Verify '<Description>'
	And AssetServiceUpdate Request Is Setup With Invalid Default Values
	When I Post Invalid AssetServiceUpdate Request With Invalid ContentType '<Value>'
	Then AssetService Response With '<ErrorMessage>' Should Be Returned

	Examples:
		| Description | Value | ErrorMessage           |
		| HTML        | HTML  | ERR_InvalidContentType |
		| XML         | XML   | ERR_InvalidContentType |

@Manual @Regression @Positive
@AssetService @US7349 @US21042
Scenario: AssetService_VerifyWebAPILogs
	Given AssetService Is Ready To Verify '<Description>'
	And AssetServiceCreate Request Is Setup With Default Values
	When I Post Valid AssetServiceCreate Request
	Then The WebAPI logs should contain an entry for the AssetCreated with AssetUID value
	And The logs must contain the Kafka driver name that is used for publishing the event
	And The logs must contain the Instrumentation information

@Manual @Regression @Negative
@AssetService @US7349 @US21042
Scenario: AssetService_Create_WhenKafkaIsInaccessible
	Given AssetService Is Ready To Verify '<Description>'
	And AssetServiceCreate Request Is Setup With Default Values
	And AssetService WebAPI Config file has an Incorrect entry for Kafka server
	When I Post Valid AssetServiceCreate Request
	Then The Asset Created Details should not be available in MySql DB
	And The AssetCreateEvent should not be available in kafka topic

@Manual @Regression @Negative
@AssetService @US7349 @US21042
Scenario: AssetService_Create_WhenMySqlDBIsInaccessible
	Given AssetService Is Ready To Verify '<Description>'
	And AssetServiceCreate Request Is Setup With Default Values
	And Machine Config file has an Incorrect entry for MySql server
	When I Post Valid AssetServiceCreate Request
	Then The Asset Created Details should not be available in MySql DB
	And The AssetCreateEvent should not be available in kafka topic