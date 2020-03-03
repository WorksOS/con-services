Feature: FuelBurnRateFeature

User Stroy:44875 Implementation - VisionLink Administrator - Fuel burn rate targets
#------------------------------------------------------------------------------------------------------------
@Manual @Sanity @Positive
@MileageTargetAPIFeature @US44875
Scenario:FuelBurnRateAPI_HappyPath
Given FuelBurnRateAPI is ready to verify '<Decription>'
   And I create Asset using create asset requests
   And FuelBurnRateAPI request is setup with default valid values
When I Put valid FuelBurnRateAPI request
Then Same FuelBurnRate details should be displayed

#@Manual @Sanity @Positive
#@MileageTargetAPIFeature @US44875
#Scenario:FuelBurnRateAPI_HappyPath
#Given FuelBurnRateAPI is ready to verify '<Decription>'
#   And I create Asset using create asset requests
#   And FuelBurnRateAPI request is setup with default valid values
#When I Put valid FuelBurnRateAPI request
#Then Same details should be displayed
#
#@Manual @Sanity @Positive
#@MileageTargetAPIFeature @US44875
#Scenario:FuelBurnRateAPI_HappyPath
#Given FuelBurnRateAPI is ready to verify '<Decription>'
#   And I create Asset using create asset requests
#   And FuelBurnRateAPI request is setup with default valid values
#When I Put valid FuelBurnRateAPI request
#Then Same details should be displayed
#
#@Manual @Sanity @Positive
#@MileageTargetAPIFeature @US44875
#Scenario:FuelBurnRateAPI_HappyPath
#Given FuelBurnRateAPI is ready to verify '<Decription>'
#   And I create Asset using create asset requests
#   And FuelBurnRateAPI request is setup with default valid values
#When I Put valid FuelBurnRateAPI request
#Then Same details should be displayed