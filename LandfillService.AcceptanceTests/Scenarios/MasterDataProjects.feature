Feature: MasterDataProjects

	Dependencies:	Internal  - AutomationCoreAPI
					External  - Kafka queue, Landfill WebApi, mySql database, project monitoring and Foreman Web Api's
Background: 

@Manual @Sanity @Positive
@MasterDataProjects
Scenario: Create a new landfill project 
Given I inject the following projects master data events
| DaysToExpire | Boundaries | ProjectName    | Type     | TimeZone        |
| 10           | boundary   | AcceptanceTest | LandFill | America/Chicago |
When I request the project details from landfill web api 
Then I the project details result from the Web Api should be 
| DaysToExpire | Boundaries | ProjectName    | Type     | TimeZone        |
| 10           | boundary   | AcceptanceTest | LandFill | America/Chicago |