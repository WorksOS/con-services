Feature: CompactionDesignProfile
	I should be able to request Compaction Design Profile data.


Scenario: Compaction Get Slicer Design Profile
	Given the Compaction Profile service URI "/api/v2/profiles/design/slicer"
  And a projectUid "7925f179-013d-4aaf-aff4-7b9833bb06d6"
  And a startLatDegrees "36.207310" and a startLonDegrees "-115.019584" and an endLatDegrees "36.207322" And an endLonDegrees "-115.019574"
  And a importedFileUid "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff" 
  And a importedFileUid "220e12e5-ce92-4645-8f01-1942a2d5a57f"
	When I request a Compaction Design Profile 
	Then the Compaction Design Profile should be
"""
{
    "gridDistanceBetweenProfilePoints": 1.6069349835347946,
    "results": [
        {
            "designFileUid": "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff",
            "data": [
                {
                    "x": 0,
                    "y": 597.4387
                },
                {
                    "x": 0.80197204271533173,
                    "y": 597.4356
                },
                {
                    "x": 1.6069349835347948,
                    "y": 597.434265
                }
            ]
        },
        {
            "designFileUid": "220e12e5-ce92-4645-8f01-1942a2d5a57f",
            "data": []
        }
    ],
    "Code": 0,
    "Message": "success"
}
"""

Scenario: Compaction Get Slicer Empty Design Profile
	Given the Compaction Profile service URI "/api/v2/profiles/design/slicer"
  And a projectUid "7925f179-013d-4aaf-aff4-7b9833bb06d6"
  And a startLatDegrees "36.209310" and a startLonDegrees "-115.019584" and an endLatDegrees "36.209322" And an endLonDegrees "-115.019574"
  And a importedFileUid "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff"
	When I request a Compaction Design Profile 
	Then the Compaction Design Profile should be
"""
{
    "gridDistanceBetweenProfilePoints": 0,
    "results": [
        {
            "designFileUid": "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff",
            "data": []
        }
    ],
    "Code": 0,
    "Message": "success"
}
"""
@ignore
Scenario: Compaction Get Slicer Design Profile With Added Endpoints
	Given the Compaction Profile service URI "/api/v2/profiles/design/slicer"
  And a projectUid "7925f179-013d-4aaf-aff4-7b9833bb06d6"
  And a startLatDegrees "36.207250" and a startLonDegrees "-115.019584" and an endLatDegrees "36.207322" And an endLonDegrees "-115.019574"
  And a importedFileUid "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff" 
	When I request a Compaction Design Profile 
	Then the Compaction Design Profile should be
"""
{
    "gridDistanceBetweenProfilePoints": 8.0405782488513378,
    "results": [
        {
            "designFileUid": "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff",
            "data": [
                {
                    "x": 0,
                    "y": "NaN"
                },
                {
                    "x": 1.4989359768016426,
                    "y": 597.107849
                },
                {
                    "x": 2.4363884583427549,
                    "y": 597.317444
                },
                {
                    "x": 3.0398711084175734,
                    "y": 597.4535
                },
                {
                    "x": 3.8040219479024642,
                    "y": 597.46875
                },
                {
                    "x": 4.3387533308993378,
                    "y": 597.4797
                },
                {
                    "x": 5.2303524427833983,
                    "y": 597.466736
                },
                {
                    "x": 5.5624914376821266,
                    "y": 597.4633
                },
                {
                    "x": 6.7969372754612811,
                    "y": 597.4468
                },
                {
                    "x": 7.7815432982108987,
                    "y": 597.437439
                },
                {
                    "x": 8.040578248851336,
                    "y": 597.434265
                },
                {
                    "x": 8.0405782488513378,
                    "y": "NaN"
                }
            ]
        }
    ],
    "Code": 0,
    "Message": "success"
}
"""