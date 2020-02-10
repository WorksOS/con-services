Feature: SAVSubscription


@Automated @Positive @US63219 @Regression
Scenario: ServiceViewTerminationWithEndDateOfSAVLessThanEndDateOfOwnerCustomer
	Given SAVSubscription Service Is Ready To Verify 'ServiceViewTerminationWithEndDateOfSAVLessThanEndDateOfOwnerCustomer'
	And I Have Asset With Shared Asset View With 'Single' Service Plan
	When I 'Terminate' ServicePlan To Existing Shared Asset
	Then Subscription Update Should Reflect in VSS DB

	@Manual @Positive @US63219 @Regression
Scenario: ServiceViewTerminationWithEndDateOfSAVMoreThanEndDateOfOwnerCustomer
	Given SAVSubscription Service Is Ready To Verify 'ServiceViewTerminationWithEndDateOfSAVMoreThanEndDateOfOwnerCustomer'
	And I Have Asset With Shared Asset View With 'Single' Service Plan
	When I 'Terminate' ServicePlan To Existing Shared Asset 
	Then Subscription Update Should Reflect in VSS DB

	@Manual @Positive @US63219 @Regression
Scenario: AddNewServicePlanToExistingSharedAsset
	Given SAVSubscription Service Is Ready To Verify 'AddNewServicePlanToExistingSharedAsset'
	And I Have Asset With Shared Asset View With 'Single' Service Plan
	When I 'Add' ServicePlan To Existing Shared Asset 
	Then Subscription Update Should Reflect in VSS DB

#		@Manual @Positive @US63219 @Regression
#Scenario: RenewServicePlanToExistingSharedAsset
#	Given SAVSubscription Service Is Ready To Verify 'RenewServicePlanToExistingSharedAsset'
#	And I Have Asset With Terminated Service Plan
#	When I 'Renew' ServicePlan To Existing Shared Asset  
#	Then Subscription Update Should Reflect in VSS DB

#		@Manual @Positive @US63219 @Regression
#Scenario: TerminateOneOfTheServicePlansToExistingSharedAsset
#	Given SAVSubscription Service Is Ready To Verify 'TerminateOneOfTheServicePlansToExistingSharedAsset'
#	And I Have Asset With Shared Asset View With 'Multiple' Service Plan
#	When I Terminate One Of The ServicePlans To Existing Shared Asset 
#	Then Subscription Update Should Reflect in VSS DB

			@Manual @Positive @US63219 @Regression
Scenario: TerminateCoreBasicServicePlanToExistingSharedAsset
	Given SAVSubscription Service Is Ready To Verify 'TerminateCoreBasicTheServicePlanToExistingSharedAsset'
	And I Have Asset With Shared Asset View With 'Multiple' Service Plan
	When I Terminate 'Core Basic' ServicePlan To Existing Shared Asset 
	Then Subscription Update Should Reflect in VSS DB

			@Manual @Positive @US63219 @Regression
Scenario: TerminateAddOnServicePlanToExistingSharedAsset
	Given SAVSubscription Service Is Ready To Verify 'TerminateAddonServicePlanToExistingSharedAsset'
	And I Have Asset With Shared Asset View With 'Multiple' Service Plan
	When I Terminate 'Addon' ServicePlan To Existing Shared Asset 
	Then Subscription Update Should Reflect in VSS DB
