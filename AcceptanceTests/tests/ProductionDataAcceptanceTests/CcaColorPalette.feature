Feature: CcaColorPalette
	I should be able to request CcaColorPalette

Scenario: Get CCA color palette
	When I request CCA color palette for machine 1 in project 1999999
	Then the following color is returned
	"""
	{
	  "palettes": [
		{
		  "Colour": 12632064,
		  "Value": 1
		},
		{
		  "Colour": 16711680,
		  "Value": 2
		},
		{
		  "Colour": 65535,
		  "Value": 3
		},
		{
		  "Colour": 32768,
		  "Value": 4
		}
	  ],
	  "Code": 0,
	  "Message": "success"
	}
	"""
