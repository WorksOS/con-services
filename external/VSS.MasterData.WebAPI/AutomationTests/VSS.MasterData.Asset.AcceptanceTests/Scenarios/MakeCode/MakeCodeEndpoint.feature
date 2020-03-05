Feature: US84264_MasterDataMakeCodeEndPoint


@Automated @Positive @Regression @US84264
Scenario: MakeCodeEndpoint_HappyPath
	Given 'MakeCodeEndPoint' Is Ready to Verify 'MakeCodeEndpoint_HappyPath'
	When I Hit MakeCode Endpoint
	Then The MakeCode Endpoint Should Return Valid Response

@Automated @Positive @Regression @US84264
Scenario: MakeCodeEndpoint_CreateMakeCode_HappyPath
	Given 'MakeCodeEndPoint' Is Ready to Verify 'MakeCodeEndpoint_CreateMakeCode_HappyPath'
	And I Perform Create New MakeCode
	When I Hit MakeCode Endpoint
	Then The MakeCode Endpoint Should Return Valid Response

	@Automated @Positive @Regression @US84264
Scenario: MakeCodeEndpoint_UpdateMakeCode_HappyPath
	Given 'MakeCodeEndPoint' Is Ready to Verify 'MakeCodeEndpoint_UpdateMakeCode_HappyPath'
	And I Perform Update MakeCode
	When I Hit MakeCode Endpoint
	Then The MakeCode Endpoint Should Return Valid Response

	@Automated @Positive @Regression @US84264
Scenario Outline: MakeCodeEndpoint_ValidUser_HappyPath
	Given 'MakeCodeEndPoint' Is Ready to Verify 'MakeCodeEndpoint_ValidUser_HappyPath'
	When I Hit MakeCode Endpoint With <Credentials> 
	Then The MakeCode Endpoint Should Return <ResponseCode> ResponseCode
	Examples: 
	| Description  | Credentials         | ResponseCode |
	| Valid User   | Valid_Credentials   | 200          |
	| Invalid User | Invalid_Credentials | 401          |

		@Manual @Positive @Regression @US84264
Scenario: MakeCodeEndpoint_InternalServerError_HappyPath
	Given 'MakeCodeEndPoint' Is Ready to Verify 'MakeCodeEndpoint_InternalServerError_HappyPath'
	When I Hit MakeCode Endpoint When Service Is Down
	Then The MakeCode Endpoint Should Return '<500>' ResponseCode
