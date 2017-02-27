Feature: FilterServices
	I should be able to save, retrieve and apply (by ID) filters.

Scenario: FilterServices - Save and Retrieve a Filter
	Given the Filter service URI "/api/v1/projects/{0}/filters" with a test project ID 1001158
		And a unique filter
		And I can successfully save this unique filter
	When I try to retrieve all saved filters for the test project
		And I try to retrieve the single unique filter I just saved 
	Then the all-filter list retrieved should contain the filter I just saved
		And the single filter retrieved should match the filter I just saved

Scenario: FilterServices - Filter Contention
	Given the Tile service URI "/api/v1/tiles" and the following request
		"""
		{
			"projectId": 1001158,
			"mode": 23,
			"palettes": [
				{ "color": 16711680, "value": 0 }, { "color": 65280, "value": 63 },
				{ "color": 255, "value": 149 }, { "color": 16776960, "value": 271 },
				{ "color": 16711935, "value": 388 }
			],
			"liftBuildSettings": {
				"cCVRange": { "min": 80, "max": 130 }, "cCVSummarizeTopLayerOnly": false,
				"deadBandLowerBoundary": 0.2, "deadBandUpperBoundary": 0.05,
				"firstPassThickness": 0, "liftDetectionType": 2, "liftThicknessType": 1, 
				"mDPRange": { "min": 80, "max": 130 }, "mDPSummarizeTopLayerOnly": false
			},
			"filter1": {
				"polygonGrid": [
					{ "x": 2321.520, "y": 1206.662 }, { "x": 2322.880, "y": 1206.662 },
					{ "x": 2322.880, "y": 1206.322 }, { "x": 2321.520, "y": 1206.322 }
				]
			},
			"filterId1": 3,
			"filterLayerMethod": 0,
			"boundBoxGrid": {
				"bottomLeftX": 2321.520, "bottomleftY": 1206.322,
				"topRightX": 2323.216, "topRightY": 1206.653
			},
			"width": 320,
			"height": 64
		}
		"""
		And the Filter service URI "/api/v1/projects/{0}/filters" with a test project ID 1001158
		And a unique filter
		And I can successfully save this unique filter
	When I request the tile
	Then the response should contain the same tile data as the following one
		"""
		{
			"TileData": "iVBORw0KGgoAAAANSUhEUgAAAUAAAABACAIAAADkhTlJAAAABnRSTlMA/wD/AP83WBt9AAACUUlEQVR42u2TwQnAQAzDfPsPfYV2hD5sgTRBZKJzw+aEbXDvaZ/wC/b1L5f8Qgd8+ydAXj8GPIABVwXI68eABzDgqgB5/RjwAAZcFSCvHwMewICrAuT1Y8ADGHBVgLx+DHgAA64KkNePAQ9gwFUB8vox4AEMuCpAXj8GPIABVwXI68eABzDgqgB5/RjwAAZcFSCvHwMewICrAuT1Y8ADGHBVgLx+DHgAA64KkNePAQ9gwFUB8vox4AEMuCpAXj8GPIABVwXI68eABzDgqgB5/RjwAAZcFSCvHwMewICrAuT1Y8ADGHBVgLx+DHgAA64KkNePAQ9gwFUB8vox4AEMuCpAXj8GPIABVwXI68eABzDgqgB5/RjwAAZcFSCvHwMewICrAuT1Y8ADGHBVgLx+DHgAA64KkNePAQ9gwFUB8vox4AEMuCpAXj8GPIABVwXI68eABzDgqgB5/RjwAAZcFSCvHwMewICrAuT1Y8ADGHBVgLx+DHgAA64KkNePAQ9gwFUB8vox4AEMuCpAXj8GPIABVwXI68eABzDgqgB5/RjwAAZcFSCvHwMewICrAuT1Y8ADGHBVgLx+DHgAA64KkNePAQ9gwFUB8vox4AEMuCpAXj8GPIABVwXI68eABzDgqgB5/RjwAAZcFSCvHwMewICrAuT1Y8ADGHBVgLx+DHgAA64KkNePAQ9gwFUB8vox4AEMuCpAXj8GPIABVwXI68eABzDgqgB5/RjwAAZcFSCvHwMewICrAuT1Y8ADGHBVgLx+DHgAA64KkNePAQ+ADvgBVGR/kMQQZR0AAAAASUVORK5CYII=",
			"Code": 0,
			"Message": "success"
		}
		"""
@ignore
Scenario: FilterServices - Apply a Saved Filter
	Given the Tile service URI "/api/v1/tiles" and the following request
		"""
		{
			"projectId": 1001158,
			"mode": 23,
			"palettes": [
				{ "color": 16711680, "value": 0 }, { "color": 65280, "value": 63 },
				{ "color": 255, "value": 149 }, { "color": 16776960, "value": 271 },
				{ "color": 16711935, "value": 388 }
			],
			"liftBuildSettings": {
				"cCVRange": { "min": 80, "max": 130 }, "cCVSummarizeTopLayerOnly": false,
				"deadBandLowerBoundary": 0.2, "deadBandUpperBoundary": 0.05,
				"firstPassThickness": 0, "liftDetectionType": 2, "liftThicknessType": 1,
				"mDPRange": { "min": 80, "max": 130 }, "mDPSummarizeTopLayerOnly": false
			},
			"filterId1": 3,
			"filterLayerMethod": 0,
			"boundBoxGrid": {
				"bottomLeftX": 2321.520, "bottomleftY": 1206.322,
				"topRightX": 2323.216, "topRightY": 1206.653
			},
			"width": 320,
			"height": 64
		}
		"""
		And the Filter service URI "/api/v1/projects/{0}/filters" with a test project ID 1001158
		And a unique filter
		And I can successfully save this unique filter
	When I request the tile
	Then the response should contain the same tile data as the following one
		"""
		{
			"TileData": "iVBORw0KGgoAAAANSUhEUgAAAUAAAABACAIAAADkhTlJAAAABnRSTlMA/wD/AP83WBt9AAABYElEQVR42u3TiQ3DMBAEMav/op0iNsBECFnBPZjzPnc7z90bvO+pR/jCEvUA/+vcfnsB/4C7X3A1AccEzELAMQGzEHBMwCwEHBMwCwHHBMxCwDEBsxBwTMAsBBwTMAsBxwTMQsAxAbMQcEzALAQcEzALAccEzELAMQGzEHBMwCwEHBMwCwHHBMxCwDEBsxBwTMAsBBwTMAsBxwTMQsAxAbMQcEzALAQcEzALAccEzELAMQGzEHBMwCwEHBMwCwHHBMxCwDEBsxBwTMAsBBwTMAsBxwTMQsAxAbMQcEzALAQcEzALAccEzELAMQGzEHBMwCwEHBMwCwHHBMxCwDEBsxBwTMAsBBwTMAsBxwTMQsAxAbMQcEzALAQcEzALAccEzELAMQGzEHBMwCwEHBMwCwHHBMxCwDEBsxBwTMAsBBwTMAsBxwTMQsAxAbMQcEzALAQcEzALAccEzELAMQGz+AAYJT+QN+Ri4wAAAABJRU5ErkJggg==",		
			"Code": 0,
			"Message": "success"
		}
		"""

Scenario: FilterServices - Bad Request (Retrieve Invalid Filter ID)
	Given the Filter service URI "/api/v1/projects/{0}/filters" with a test project ID 1001158
	When I try to retrieve filter with ID 0 expecting BadRequest
	Then the response should contain Code -2 and Message "Invalid filter ID: 0"

Scenario: FilterServices - Bad Request (Retrieve NonExistent Filter ID)
	Given the Filter service URI "/api/v1/projects/{0}/filters" with a test project ID 1001158
	When I try to retrieve filter with ID 9223372036854775807 expecting BadRequest
	Then the response should contain Code -4 and Message "Failed to get requested filter details"

Scenario: FilterServices - Bad Request (Retrieve Invalid Project ID)
	Given the Filter service URI "/api/v1/projects/{0}/filters" with a test project ID 0
	When I try to retrieve all saved filters for the test project expecting BadRequest
	Then the response should contain Code -2 and Message "Invalid project ID: 0"

Scenario: FilterServices - Retrieve NonExistent Project ID
	Given the Filter service URI "/api/v1/projects/{0}/filters" with a test project ID 9223372036854775807
	When I try to retrieve all saved filters for the test project
	Then the FiltersArray in the response should be empty