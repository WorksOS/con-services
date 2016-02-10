Feature: MasterDataSubscriptions

	Dependencies:	Internal  - AutomationCoreAPI
					External  - Kafka queue, mySql database

@Sanity @Positive @Automated
@MasterDataSubscriptions
Scenario: Create Asset Subscription
Given I inject the following master data subscription event "CreateAssetSubscriptionEvent" into kafka
Then I verify the correct subscription event in the database


@Sanity @Positive @Automated
@MasterDataSubscriptions
Scenario: Update Asset Subscription Event
Given I inject the following master data subscription event "CreateAssetSubscriptionEvent" into kafka
And I inject the following master data subscription event "UpdateAssetSubscriptionEvent" into kafka
Then I verify the correct subscription event in the database

@Sanity @Positive @Automated
@MasterDataSubscriptions
Scenario: Create Project Subscription Event
Given I inject the following master data subscription event "CreateProjectSubscriptionEvent" into kafka
Then I verify the correct subscription event in the database


@Sanity @Positive @Automated
@MasterDataSubscriptions
Scenario: Update Project Subscription Event
Given I inject the following master data subscription event "CreateProjectSubscriptionEvent" into kafka
And I inject the following master data subscription event "UpdateProjectSubscriptionEvent" into kafka
Then I verify the correct subscription event in the database


@Sanity @Positive @Automated
@MasterDataSubscriptions
Scenario: Create Customer Subscription Event
Given I inject the following master data subscription event "CreateCustomerSubscriptionEvent" into kafka
Then I verify the correct subscription event in the database


@Sanity @Positive @Automated
@MasterDataSubscriptions
Scenario: Update Customer Subscription Event
Given I inject the following master data subscription event "CreateCustomerSubscriptionEvent" into kafka
And I inject the following master data subscription event "UpdateCustomerSubscriptionEvent" into kafka
Then I verify the correct subscription event in the database


@Sanity @Positive @Automated
@MasterDataSubscriptions
Scenario: Associate Project Subscription Event
Given I inject the following master data subscription event "CreateProjectSubscriptionEvent" into kafka
And I inject the following master data subscription event "AssociateProjectSubscriptionEvent" into kafka
Then I verify the correct subscription event in the database


@Sanity @Positive @Automated
@MasterDataSubscriptions
Scenario: Dissociate Project Subscription Event
Given I inject the following master data subscription event "CreateProjectSubscriptionEvent" into kafka
And I inject the following master data subscription event "DissociateProjectSubscriptionEvent" into kafka
Then I verify the correct subscription event in the database