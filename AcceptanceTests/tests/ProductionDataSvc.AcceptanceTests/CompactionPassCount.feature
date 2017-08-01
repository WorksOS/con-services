Feature: CompactionPassCount
I should be able to request compaction data

Scenario: Compaction Get Passcount Summary 
Given the Compaction Passcount Summary service URI "/api/v2/compaction/passcounts/summary"
And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
When I request Passcount summary
Then the Passcount summary result should be
"""
{
"passCountSummaryData": {
    "percentEqualsTarget": 3.3255301569815479,
    "percentGreaterThanTarget": 45.311209033324154,
    "percentLessThanTarget": 51.3632608096943,
    "totalAreaCoveredSqMeters": 1678.9744000000003,
    "passCountTarget": {
    "minPassCountMachineTarget": 5,
    "maxPassCountMachineTarget": 5,
    "targetVaries": true
    }
},
"Code": 0,
"Message": "success"
}
"""

Scenario: Compaction Get Passcount Details 
Given the Compaction Passcount Details service URI "/api/v2/compaction/passcounts/details"
And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
When I request Passcount details
Then the Passcount details result should be
"""
{
	"passCountDetailsData": {
		"percents": [
			19.632901900694421,
			8.5406274791075756,
			6.160684206522566,
			5.132635651332877,
			4.1589237005401056,
			3.5981699431639118,
			2.8341972853432442,
			2.0126278268618436,
			47.929232006433452
		],
		"totalCoverageArea": 1678.9744000000003,
		"passCountTarget": {
			"minPassCountMachineTarget": 0,
			"maxPassCountMachineTarget": 0,
			"targetVaries": true
		}
	},
	"Code": 0,
	"Message": "success"
}
"""


Scenario: Compaction Get Passcount Summary  with project settings
Given the Compaction Passcount Summary service URI "/api/v2/compaction/passcounts/summary"
And a projectUid "3335311a-f0e2-4dbe-8acd-f21135bafee4"
When I request Passcount summary
Then the Passcount summary result should be
"""
{
  "passCountSummaryData": {
    "percentEqualsTarget": 14.701311685630142,
    "percentGreaterThanTarget": 65.665786413675448,
    "percentLessThanTarget": 19.632901900694421,
    "totalAreaCoveredSqMeters": 10637.396400000001,
    "PassCountTarget": {
      "minPassCountMachineTarget": 2.0,
      "maxPassCountMachineTarget": 3.0,
      "targetVaries": false
    }
  },
  "Code": 0,
  "Message": "success"
}
"""

Scenario: Compaction Get Passcount Details with project settings
Given the Compaction Passcount Details service URI "/api/v2/compaction/passcounts/details"
And a projectUid "3335311a-f0e2-4dbe-8acd-f21135bafee4"
When I request Passcount details
Then the Passcount details result should be
"""
{
  "passCountDetailsData": {
    "percents": [
      19.632901900694421,
      8.5406274791075756,
      6.160684206522566,
      5.132635651332877,
      14.624153707386517,
      10.799943489931426,
      6.4073723904845741,
      0.59661591627815991,
      28.105065258261881
    ],
    "totalCoverageArea": 10637.396400000001,
    "PassCountTarget": {
      "minPassCountMachineTarget": 2.0,
      "maxPassCountMachineTarget": 3.0,
      "targetVaries": false
    }
  },
  "Code": 0,
  "Message": "success"
}
"""
