Feature: MasterDataProjects

	Dependencies:	Internal  - AutomationCoreAPI
					External  - Kafka queue, Landfill WebApi, mySql database, project monitoring and Foreman Web Api's
Background: 

@Sanity @Positive
@MasterDataProjects
Scenario: Create a new landfill project 
Given I inject the following master data events
| Event              | DaysToExpire | Boundaries | ProjectName    | Type     | TimeZone        |
| CreateProjectEvent | 10           | boundary   | AcceptanceTest | LandFill | America/Chicago |
When I request a list of projects from landfill web api 
Then I find the project I created in the list  


@Sanity @Positive
@MasterDataProjects
Scenario: Update a new landfill project 
Given I inject the following master data events
| Event              | DaysToExpire | Boundaries | ProjectName      | Type     | TimeZone        |
| CreateProjectEvent | 10           | boundary   | AcceptanceUpdate | LandFill | America/Chicago |
And I inject the following master data events
| Event              | DaysToExpire | ProjectName      |  Type     |
| UpdateProjectEvent | 50           | AcceptanceUpdate |  Full3D   |
When I request a list of projects from landfill web api 
Then I find update project details in the project list  

@Sanity @Positive
@MasterDataProjects
Scenario: Delete a new landfill project 
Given I inject the following master data events
| Event              | DaysToExpire | Boundaries | ProjectName      | Type     | TimeZone        |
| CreateProjectEvent | 10           | boundary   | AcceptanceDelete | LandFill | America/Chicago |
And I inject the following master data events
| Event              | 
| DeleteProjectEvent |
When I request a list of projects from landfill web api 
Then I dont find the project I created in the list  