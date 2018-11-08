Feature: Weights

Scenario: Add weight to a site - yesterday
	When I add weights for the past 1 days to site '1863fb2e-fd25-e311-9e53-0050568824d7' of project 'Pegasus'
	Then the weights are added for the past 1 days to site '1863fb2e-fd25-e311-9e53-0050568824d7' of project 'Pegasus'

Scenario: Add weight to a site - multi days
	When I add weights for the past 3 days to site '1863fb2e-fd25-e311-9e53-0050568824d7' of project 'Pegasus'
	Then the weights are added for the past 3 days to site '1863fb2e-fd25-e311-9e53-0050568824d7' of project 'Pegasus'

Scenario: Add weight to a site - specific date
	When I add weight for '2016-04-01' to site 'fb4f0e9d-12f4-11e5-b129-0050568838e5' of project 'Casella-Stanley Landfill'
	Then the weight is added for '2016-04-01' to site 'fb4f0e9d-12f4-11e5-b129-0050568838e5' of project 'Casella-Stanley Landfill'

Scenario: Get weights of all sites
	When I add weight for '2016-04-01' to site 'fb4f0e9d-12f4-11e5-b129-0050568838e5' of project 'Casella-Stanley Landfill'
		And I request all weights for all sites of project 'Casella-Stanley Landfill'
	Then project 'Casella-Stanley Landfill' has the correct weight for site 'fb4f0e9d-12f4-11e5-b129-0050568838e5' on '2016-04-01' 