Feature: MasterDataSubscriptions

	Dependencies:	Internal  - AutomationCoreAPI
					External  - Kafka queue, mySql database


@Sanity @Positive @Automated
@MasterDataSubscriptions
Scenario: Create Customer, subscription and project and link them togeather
Given I inject the following master data event "CreateCustomerEvent" into kafka
And I inject the following master data event "AssociateCustomerUserEvent" into kafka
And I inject the following master data event "CreateProjectEvent" into kafka
And I inject the following master data event "AssociateProjectSubscriptionEvent" into kafka
Then I verify the correct subscription event in the database


#@Sanity @Positive @Automated
#@MasterDataSubscriptions
#Scenario: Create Project Subscription Event
#Given I inject the following master data event "CreateProjectSubscriptionEvent" into kafka
#Then I verify the correct subscription event in the database
#
#
#@Sanity @Positive @Automated
#@MasterDataSubscriptions
#Scenario: Update Project Subscription Event
#Given I inject the following master data event "CreateProjectSubscriptionEvent" into kafka
#And I inject the following master data event "UpdateProjectSubscriptionEvent" into kafka
#Then I verify the correct subscription event in the database
#
#
#@Sanity @Positive @Automated
#@MasterDataSubscriptions
#Scenario: Create Customer Subscription Event
#Given I inject the following master data event "CreateCustomerSubscriptionEvent" into kafka
#Then I verify the correct subscription event in the database
#
#
#@Sanity @Positive @Automated
#@MasterDataSubscriptions
#Scenario: Update Customer Subscription Event
#Given I inject the following master data event "CreateCustomerSubscriptionEvent" into kafka
#And I inject the following master data event "UpdateCustomerSubscriptionEvent" into kafka
#Then I verify the correct subscription event in the database
#
#
#@Sanity @Positive @Automated
#@MasterDataSubscriptions
#Scenario: Associate Project Subscription Event
#Given I inject the following master data event "CreateProjectSubscriptionEvent" into kafka
#And I inject the following master data event "AssociateProjectSubscriptionEvent" into kafka
#Then I verify the correct subscription event in the database
#
#
#@Sanity @Positive @Automated
#@MasterDataSubscriptions
#Scenario: Dissociate Project Subscription Event
#Given I inject the following master data event "CreateProjectSubscriptionEvent" into kafka
#And I inject the following master data event "DissociateProjectSubscriptionEvent" into kafka
#Then I verify the correct subscription event in the database