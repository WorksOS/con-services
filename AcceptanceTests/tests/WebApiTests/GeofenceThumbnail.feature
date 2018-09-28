﻿Feature: GeofenceThumbnail

Scenario: Dimensions
  Given The geofence thumbnail URI is "/api/v1/geofencethumbnail/png" for operation "png"
  And the expected response is in the "GeofenceThumbnailResponse.json" respository
	When I request a Report Tile for geofence UID "eee23e91-5682-45ec-a4a7-9dfe0d6b7a64" 
	Then The resulting thumbnail should match "Dimensions" from the response repository within "3" percent

Scenario: DimensionsBase64
  Given The geofence thumbnail URI is "/api/v1/geofencethumbnail/base64" for operation "base64"
  And the expected response is in the "GeofenceThumbnailResponse.json" respository
	When I request a Report Tile for geofence UID "eee23e91-5682-45ec-a4a7-9dfe0d6b7a64" 
	Then The resulting thumbnail should match "Dimensions" from the response repository within "3" percent
