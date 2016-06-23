Feature: CCA

Scenario: CCA Ratio -  whole project
	Given I have a landfill project 'Maddington' with landfill sites 'MarylandsLandfill,AmiStadiumLandfill'
		And I have the following machines
		| Name  | IsJonhDoe |
		| Cat A | false     |
		| Cat B | true      |
		And I have the following CCA data
		| Project    | Site       | DateAsAnOffsetFromToday | Machine | LiftID | Incomplete | Complete | Overcomplete |
		| Maddington | Marylands  | -3                      | Cat A   | null   | 20         | 35       | 45           |
		| Maddington | AmiStadium | -2                      | Cat A   | 1      | 15         | 5        | 80           |
		| Maddington | Maddington | -1                      | Cat A   | 2      | 65         | 5        | 30           |
		| Maddington | Marylands  | -3                      | Cat B   | null   | 10         | 35       | 55           |
		| Maddington | AmiStadium | -2                      | Cat B   | 1      | 35         | 5        | 60           |
		| Maddington | Maddington | -1                      | Cat B   | 2      | 25         | 5        | 70           |
	When I request CCA ratio for site 'Maddington' for the past 3 days
	Then the response contains the following CCA ration data
	| Machine | DateAsAnOffsetFromToday | CCARatio |
	| Cat A   | -3                      | 80       |
	| Cat A   | -2                      | 85       |
	| Cat A   | -1                      | 85       |
	| Cat B   | -3                      | 90       |
	| Cat B   | -2                      | 65       |
	| Cat B   | -1                      | 75       |