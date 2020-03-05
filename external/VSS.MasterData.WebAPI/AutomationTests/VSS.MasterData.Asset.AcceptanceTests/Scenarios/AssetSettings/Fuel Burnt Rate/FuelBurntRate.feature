Feature: FuelBurntRate


#Backend has no restrictions for the limit (on assigning target settings value).It is handled by UI
Scenario Outline:FuelBurntRate_HappyPath
Given FuelBurntRate is ready to verify '<Description>'
   And I fetch all the assets for a customer
   And fuel burnt rate is set with working burnt rate as '<Working Burnt Rate>' and Idle Burnt Rate as'<Idle Burnt Rate>'
When I POST valid FuelBurntRate request
And I Get the Fuel burnt rate 
Then both the values should match
Examples: 
| Description            | Working Burnt Rate | Idle Burnt Rate |
| FuelBurntRate_MinValue | 0                  | 0               |
| FuelBurntRate_MaxValue | 999.99             | 999.99          |

Scenario:FuelBurntRate_InvalidAssetUID
Given FuelBurntRate is ready to verify '<Description>'
   And fuel burnt rate is set with working burnt rate as '<Working Burnt Rate>' and Idle Burnt Rate as'<Idle Burnt Rate>'
When I POST invalid FuelBurntRate request
Then Valid Error Code should be received
