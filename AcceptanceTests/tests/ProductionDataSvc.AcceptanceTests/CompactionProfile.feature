Feature: CompactionProfile
	I should be able to request Compaction Profile data.


Scenario: Compaction Get Slicer Profile
	Given the Compaction Profile service URI "/api/v2/profiles/productiondata/slicer"
  And a projectUid "7925f179-013d-4aaf-aff4-7b9833bb06d6"
  And a startLatDegrees "36.207310" and a startLonDegrees "-115.019584" and an endLatDegrees "36.207322" And an endLonDegrees "-115.019574"
  And a cutfillDesignUid "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff"
	When I request a Compaction Profile 
	Then the Compaction Profile should be
"""
{
    "designFileUid": "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff",
    "gridDistanceBetweenProfilePoints": 1.6069349835347946,
    "points": [
        {
            "type": "firstPass",
            "data": [
                {
                    "cellType": 1,
                    "x": 0,
                    "y": 597.353,
                    "value": 597.353
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": 597.3581,
                    "value": 597.3581
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": 597.359,
                    "value": 597.359
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": 597.36084,
                    "value": 597.36084
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": 597.386,
                    "value": 597.386
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": 597.3832,
                    "value": 597.3832
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": 597.382,
                    "value": 597.382
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": 597.3828,
                    "value": 597.3828
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": 597.384,
                    "value": 597.384
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": 597.3836,
                    "value": 597.3836
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": 597.383,
                    "value": 597.383
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": 597.383,
                    "value": 597.383
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": 597.383,
                    "value": 597.383
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": 597.382935,
                    "value": 597.382935
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": 597.382,
                    "value": 597.382
                }
            ]
        },
        {
            "type": "highestPass",
            "data": [
                {
                    "cellType": 1,
                    "x": 0,
                    "y": 597.396,
                    "value": 597.396
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": 597.388367,
                    "value": 597.388367
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": 597.387,
                    "value": 597.387
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": 597.386963,
                    "value": 597.386963
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": 597.386,
                    "value": 597.386
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": 597.3832,
                    "value": 597.3832
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": 597.382,
                    "value": 597.382
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": 597.3828,
                    "value": 597.3828
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": 597.384,
                    "value": 597.384
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": 597.3836,
                    "value": 597.3836
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": 597.383,
                    "value": 597.383
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": 597.383,
                    "value": 597.383
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": 597.383,
                    "value": 597.383
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": 597.382935,
                    "value": 597.382935
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": 597.382,
                    "value": 597.382
                }
            ]
        },
        {
            "type": "lastPass",
            "data": [
                {
                    "cellType": 1,
                    "x": 0,
                    "y": 597.396,
                    "value": 597.396
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": 597.388367,
                    "value": 597.388367
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": 597.387,
                    "value": 597.387
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": 597.386963,
                    "value": 597.386963
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": 597.386,
                    "value": 597.386
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": 597.3832,
                    "value": 597.3832
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": 597.382,
                    "value": 597.382
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": 597.3828,
                    "value": 597.3828
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": 597.384,
                    "value": 597.384
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": 597.376,
                    "value": 597.376
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": 597.364,
                    "value": 597.364
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": 597.374634,
                    "value": 597.374634
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": 597.376,
                    "value": 597.376
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": 597.375549,
                    "value": 597.375549
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": 597.371,
                    "value": 597.371
                }
            ]
        },
        {
            "type": "lowestPass",
            "data": [
                {
                    "cellType": 1,
                    "x": 0,
                    "y": 597.353,
                    "value": 597.353
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": 597.3581,
                    "value": 597.3581
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": 597.359,
                    "value": 597.359
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": 597.36084,
                    "value": 597.36084
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": 597.386,
                    "value": 597.386
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": 597.3832,
                    "value": 597.3832
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": 597.382,
                    "value": 597.382
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": 597.3828,
                    "value": 597.3828
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": 597.384,
                    "value": 597.384
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": 597.376,
                    "value": 597.376
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": 597.364,
                    "value": 597.364
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": 597.374634,
                    "value": 597.374634
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": 597.376,
                    "value": 597.376
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": 597.375549,
                    "value": 597.375549
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": 597.371,
                    "value": 597.371
                }
            ]
        },
        {
            "type": "lastComposite",
            "data": [
                {
                    "cellType": 1,
                    "x": 0,
                    "y": 597.1041,
                    "value": 597.1041
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": 597.1135,
                    "value": 597.1135
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": 597.1152,
                    "value": 597.1152
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": 597.115234,
                    "value": 597.115234
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": 597.1158,
                    "value": 597.1158
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": 597.11676,
                    "value": 597.11676
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": 597.1172,
                    "value": 597.1172
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": 597.1214,
                    "value": 597.1214
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": 597.127441,
                    "value": 597.127441
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": 597.128,
                    "value": 597.128
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": 597.128845,
                    "value": 597.128845
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": 597.138062,
                    "value": 597.138062
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": 597.1392,
                    "value": 597.1392
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": 597.139343,
                    "value": 597.139343
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": 597.1405,
                    "value": 597.1405
                }
            ]
        },
        {
            "type": "cmvSummary",
            "data": [
                {
                    "cellType": 1,
                    "x": 0,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                }
            ]
        },
        {
            "type": "cmvDetail",
            "data": [
                {
                    "cellType": 1,
                    "x": 0,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": "NaN",
                    "value": "NaN"
                }
            ]
        },
        {
            "type": "cmvPercentChange",
            "data": [
                {
                    "cellType": 1,
                    "x": 0,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": "NaN",
                    "value": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": "NaN",
                    "value": "NaN"
                }
            ]
        },
        {
            "type": "mdpSummary",
            "data": [
                {
                    "cellType": 1,
                    "x": 0,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                }
            ]
        },
        {
            "type": "temperatureSummary",
            "data": [
                {
                    "cellType": 1,
                    "x": 0,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1
                }
            ]
        },
        {
            "type": "speedSummary",
            "data": [
                {
                    "cellType": 1,
                    "x": 0,
                    "y": 597.396,
                    "value": 11.34,
                    "valueType": 2,
                    "value2": 21.744
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": 597.387,
                    "value": 10.224,
                    "valueType": 2,
                    "value2": 11.844
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": 597.387,
                    "value": 10.224,
                    "valueType": 2,
                    "value2": 11.844
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": 597.386,
                    "value": 11.34,
                    "valueType": 2,
                    "value2": 11.34
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": 597.386,
                    "value": 11.34,
                    "valueType": 2,
                    "value2": 11.34
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": 597.382,
                    "value": 11.34,
                    "valueType": 2,
                    "value2": 11.34
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": 597.382,
                    "value": 11.34,
                    "valueType": 2,
                    "value2": 11.34
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": 597.384,
                    "value": 10.224,
                    "valueType": 2,
                    "value2": 10.224
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": 597.384,
                    "value": 10.224,
                    "valueType": 2,
                    "value2": 10.224
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": 597.364,
                    "value": 10.224,
                    "valueType": 2,
                    "value2": 12.204
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": 597.364,
                    "value": 10.224,
                    "valueType": 2,
                    "value2": 12.204
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": 597.376,
                    "value": 10.224,
                    "valueType": 2,
                    "value2": 12.204
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": 597.376,
                    "value": 10.224,
                    "valueType": 2,
                    "value2": 12.204
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": 597.371,
                    "value": 10.224,
                    "valueType": 2,
                    "value2": 12.204
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": 597.371,
                    "value": 10.224,
                    "valueType": 2,
                    "value2": 12.204
                }
            ]
        },
        {
            "type": "passCountSummary",
            "data": [
                {
                    "cellType": 1,
                    "x": 0,
                    "y": 597.396,
                    "value": 3,
                    "valueType": 0
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": 597.388367,
                    "value": 2,
                    "valueType": 0
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": 597.387,
                    "value": 2,
                    "valueType": 0
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": 597.386963,
                    "value": 1,
                    "valueType": 0
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": 597.386,
                    "value": 1,
                    "valueType": 0
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": 597.3832,
                    "value": 1,
                    "valueType": 0
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": 597.382,
                    "value": 1,
                    "valueType": 0
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": 597.3828,
                    "value": 1,
                    "valueType": 0
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": 597.384,
                    "value": 1,
                    "valueType": 0
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": 597.376,
                    "value": 2,
                    "valueType": 0
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": 597.364,
                    "value": 2,
                    "valueType": 0
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": 597.374634,
                    "value": 2,
                    "valueType": 0
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": 597.376,
                    "value": 2,
                    "valueType": 0
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": 597.375549,
                    "value": 2,
                    "valueType": 0
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": 597.371,
                    "value": 2,
                    "valueType": 0
                }
            ]
        },
        {
            "type": "passCountDetail",
            "data": [
                {
                    "cellType": 1,
                    "x": 0,
                    "y": 597.396,
                    "value": 3
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": 597.388367,
                    "value": 2
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": 597.387,
                    "value": 2
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": 597.386963,
                    "value": 1
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": 597.386,
                    "value": 1
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": 597.3832,
                    "value": 1
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": 597.382,
                    "value": 1
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": 597.3828,
                    "value": 1
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": 597.384,
                    "value": 1
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": 597.376,
                    "value": 2
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": 597.364,
                    "value": 2
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": 597.374634,
                    "value": 2
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": 597.376,
                    "value": 2
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": 597.375549,
                    "value": 2
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": 597.371,
                    "value": 2
                }
            ]
        },
        {
            "type": "cutFill",
            "data": [
                {
                    "cellType": 1,
                    "x": 0,
                    "y": 597.1041,
                    "value": -0.333740234,
                    "y2": 597.4387
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": 597.1135,
                    "value": -0.326782227,
                    "y2": 597.4384
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": 597.1152,
                    "value": -0.326782227,
                    "y2": 597.438354
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": 597.115234,
                    "value": -0.321960449,
                    "y2": 597.4383
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": 597.1158,
                    "value": -0.321960449,
                    "y2": 597.4375
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": 597.11676,
                    "value": -0.3173828,
                    "y2": 597.4367
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": 597.1172,
                    "value": -0.3173828,
                    "y2": 597.43634
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": 597.1214,
                    "value": -0.311035156,
                    "y2": 597.436035
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": 597.127441,
                    "value": -0.311035156,
                    "y2": 597.4356
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": 597.128,
                    "value": -0.3048706,
                    "y2": 597.435364
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": 597.128845,
                    "value": -0.3048706,
                    "y2": 597.435059
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": 597.138062,
                    "value": -0.299743652,
                    "y2": 597.434753
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": 597.1392,
                    "value": -0.299743652,
                    "y2": 597.434753
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": 597.139343,
                    "value": -0.2944336,
                    "y2": 597.4347
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": 597.1405,
                    "value": -0.2944336,
                    "y2": 597.434265
                }
            ]
        }
    ],
    "Code": 0,
    "Message": "success"
}
"""
	
