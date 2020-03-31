Feature: GeofenceThumbnail

Scenario: Dimensions
  Given The geofence thumbnail URI is "/api/v1/geofencethumbnail/png" for operation "png"
  And the expected response is in the "GeofenceThumbnailResponse.json" respository
	When I request a Thumbnail for geofence UID "eee23e91-5682-45ec-a4a7-9dfe0d6b7a64" 
	Then The resulting thumbnail should match "Dimensions" from the response repository within "3" percent

Scenario: DimensionsBase64
  Given The geofence thumbnail URI is "/api/v1/geofencethumbnail/base64" for operation "base64"
  And the expected response is in the "GeofenceThumbnailResponse.json" respository
	When I request a Thumbnail for geofence UID "eee23e91-5682-45ec-a4a7-9dfe0d6b7a64" 
	Then The resulting thumbnail should match "Dimensions" from the response repository within "3" percent

Scenario: Multiple
  Given The geofence thumbnail URI is "/api/v1/geofencethumbnails/base64" for operation "multiple"
  And the expected response is in the "MultiGeofenceThumbnailResponse.json" respository
	When I request multiple Thumbnails "c910d127-5e3c-453f-82c3-e235848ac20e,d4edddc9-d07f-4d56-ad50-5e9671631f70,eee23e91-5682-45ec-a4a7-9dfe0d6b7a64,ba35221d-cc46-48ce-970c-8b1509a0c737"
	Then The result should match "Multiple" from the response repository

Scenario: Multiple with missing geofences
  Given The geofence thumbnail URI is "/api/v1/geofencethumbnails/base64" for operation "multiple"
  And the expected response is in the "MultiGeofenceThumbnailResponse.json" respository
	When I request multiple Thumbnails "c910d127-5e3c-453f-82c3-e235848ac20e,d4edddc9-d07f-4d56-ad50-5e9671631f70,81bf5a62-7beb-4c57-ac82-1ea59f794e47"
	Then The result should match "MultipleMissing" from the response repository

Scenario: Point
  Given The geofence thumbnail URI is "/api/v1/geofencethumbnail/png" for operation "png"
  And the expected response is in the "GeofenceThumbnailResponse.json" respository
	When I request a Thumbnail for geofence UID "ba35221d-cc46-48ce-970c-8b1509a0c737" 
	Then The resulting thumbnail should match "Point" from the response repository within "3" percent

# Disabled due to CCSSSCON-153
#Scenario: Good polygons
#  Given The geofence thumbnail URI is "/api/v1/geofencethumbnailsraw/base64" for operation "geojson"
#  And the expected request is in the "MultiGeofenceThumbnailRequest.json" respository
#  And the expected response is in the "MultiGeofenceThumbnailResponse.json" respository
#	When I request a Thumbnail for "GoodPolygons" from the request repository expecting "OK"  
#	Then The result should match "MultipleRaw" from the response repository

Scenario: No polygons 
  Given The geofence thumbnail URI is "/api/v1/geofencethumbnailsraw/base64" for operation "geojson"
  And the expected request is in the "MultiGeofenceThumbnailRequest.json" respository
  And the expected response is in the "MultiGeofenceThumbnailResponse.json" respository
  When I request a Thumbnail for "NoPolygons" from the request repository expecting "BadRequest" 
  Then I should get error code -1 and message "No boundaries found for geofence thumbnails"

Scenario: Too many polygons 
  Given The geofence thumbnail URI is "/api/v1/geofencethumbnailsraw/base64" for operation "geojson"
  And the expected request is in the "MultiGeofenceThumbnailRequest.json" respository
  And the expected response is in the "MultiGeofenceThumbnailResponse.json" respository
  When I request a Thumbnail for "TooManyPolygons" from the request repository expecting "BadRequest" 
  Then I should get error code -1 and message "A maximum of 10 boundaries allowed for geofence thumbnails"