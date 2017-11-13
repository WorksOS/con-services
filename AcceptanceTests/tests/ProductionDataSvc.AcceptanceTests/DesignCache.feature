Feature: DesignCache
	I should be able to delete files from the design cache.

#Scenario: DesignCache - Delete a Design Surface From Cache
#	Given the design cache file delete uri "/api/v1/designcache/delete", a project 1001158 and a file "Building Pad - Building_Pad.ttm"
#		And the following Summary Volumes request is sent to "/api/v1/volumes/summary"
#			"""
#			{
#				"projectID": 1001158,
#				"volumeCalcType": 6,
#				"baseDesignDescriptor": {
#					"id": -1,
#					"file": {
#						"filespaceId": "u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01",
#						"path": "/77561/1158",
#						"fileName": "Building Pad - Building_Pad.ttm"
#					},
#					"offset": 0
#				},
#				"topFilter": {
#					"polygonGrid": [
#						{ "x": 2860.065, "y": 1221.277 }, { "x": 2860.766, "y": 1221.284 },
#						{ "x": 2860.758, "y": 1220.609 }, { "x": 2860.072, "y": 1220.594 }
#				],
#				"returnEarliest": false
#				}
#			}
#			"""
#		And the response code is OK 200
#	When I delete this file
#		And the following Summary Volumes request is sent to "/api/v1/volumes/summary"
#			"""
#			{
#				"projectID": 1001158,
#				"volumeCalcType": 6,
#				"baseDesignDescriptor": {
#					"id": -1,
#					"file": {
#						"filespaceId": "u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01",
#						"path": "/77561/1158",
#						"fileName": "Building Pad - Building_Pad.ttm"
#					},
#					"offset": 0
#				},
#				"topFilter": {
#					"polygonGrid": [
#						{ "x": 2860.065, "y": 1221.277 }, { "x": 2860.766, "y": 1221.284 },
#						{ "x": 2860.758, "y": 1220.609 }, { "x": 2860.072, "y": 1220.594 }
#				],
#				"returnEarliest": false
#				}
#			}
#			"""
#	Then the response code should be BadRequest 400

#@ignore
#Scenario: DesignCache - Delete a Design Surface From Cache
#	Given the DeleteDesignCacheFile service URI "/api/v1/designcache/delete", a project 1001158 and a file named "Building Pad - Building_Pad.ttm"
#		And the following Summary Volumes request is sent to "/api/v1/volumes/summary" to make sure the design file is downloaded if required
#			"""
#			{
#				"projectID": 1001158,
#				"volumeCalcType": 6,
#				"baseDesignDescriptor": {
#					"id": -1,
#					"file": {
#						"filespaceId": "u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01",
#						"path": "/77561/1158",
#						"fileName": "Building Pad - Building_Pad.ttm"
#					},
#					"offset": 0
#				},
#				"topFilter": {
#					"polygonGrid": [
#						{ "x": 2860.065, "y": 1221.277 }, { "x": 2860.766, "y": 1221.284 },
#						{ "x": 2860.758, "y": 1220.609 }, { "x": 2860.072, "y": 1220.594 }
#				],
#				"returnEarliest": false
#				}
#			}
#			"""
#	When I delete this file
#	Then the file should no longer exist in the design cache
#
#@ignore
#Scenario: DesignCache - Automatically Download a Design Surface Into Cache
#	Given the DeleteDesignCacheFile service URI "/api/v1/designcache/delete", a project 1001158 and a file named "Building Pad - Building_Pad.ttm"
#		And the file does not already exist in the design cache
#	When the following Summary Volumes request is sent to "/api/v1/volumes/summary"
#		"""
#		{
#			"projectID": 1001158,
#			"volumeCalcType": 6,
#			"baseDesignDescriptor": {
#				"id": -1,
#				"file": {
#					"filespaceId": "u3bdc38d6-1afe-470e-8c1c-fc241d4c5e01",
#					"path": "/77561/1158",
#					"fileName": "Building Pad - Building_Pad.ttm"
#				},
#				"offset": 0
#			},
#			"topFilter": {
#				"polygonGrid": [
#					{ "x": 2860.065, "y": 1221.277 }, { "x": 2860.766, "y": 1221.284 },
#					{ "x": 2860.758, "y": 1220.609 }, { "x": 2860.072, "y": 1220.594 }
#			],
#			"returnEarliest": false
#			}
#		}
#		"""
#	Then the file should be automatically downloaded into the design cache
