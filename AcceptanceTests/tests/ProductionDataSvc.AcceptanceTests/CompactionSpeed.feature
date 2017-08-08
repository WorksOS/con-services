Feature: CompactionSpeed
	 I should be able to request compaction speed data

Scenario: Compaction Get Speed Summary 
Given the Compaction Speed Summary service URI "/api/v2/compaction/speed/summary"
And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
When I request Speed summary
Then the Speed result should be
"""
{
"speedSummaryData": {
    "percentEqualsTarget": 36.9,
    "percentGreaterThanTarget": 39.3,
    "percentLessThanTarget": 23.8,
    "totalAreaCoveredSqMeters": 10636.7028,
    "minTarget": 5.0,
    "maxTarget": 10.0
},
"Code": 0,
"Message": "success"
}
"""

Scenario: Compaction Get Speed Summary with summary settings
Given the Compaction Speed Summary service URI "/api/v2/compaction/speed/summary"
And a projectUid "3335311a-f0e2-4dbe-8acd-f21135bafee4"
When I request Speed summary
Then the Speed result should be
"""
{
  "speedSummaryData": {
    "percentEqualsTarget": 25.0,
    "percentGreaterThanTarget": 35.0,
    "percentLessThanTarget": 40.1,
    "totalAreaCoveredSqMeters": 10636.7028,
    "minTarget": 7.0,
    "maxTarget": 11.0
  },
  "Code": 0,
  "Message": "success"
}
"""
 