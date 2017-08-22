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
    "gridDistanceBetweenProfilePoints": 1.6069349835347946,
    "points": [
        {
            "type": "firstPass",
            "data": [
                {
                    "cellType": 1,
                    "x": 0,
                    "y": 597.353,
                    "value": 597.353,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": 597.3581,
                    "value": 597.3581,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": 597.359,
                    "value": 597.359,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": 597.36084,
                    "value": 597.36084,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": 597.386,
                    "value": 597.386,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": 597.3832,
                    "value": 597.3832,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": 597.382,
                    "value": 597.382,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": 597.3828,
                    "value": 597.3828,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": 597.384,
                    "value": 597.384,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": 597.3836,
                    "value": 597.3836,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": 597.383,
                    "value": 597.383,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": 597.383,
                    "value": 597.383,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": 597.383,
                    "value": 597.383,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": 597.382935,
                    "value": 597.382935,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": 597.382,
                    "value": 597.382,
                    "valueType": -1,
                    "y2": "NaN"
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
                    "value": 597.396,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": 597.388367,
                    "value": 597.388367,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": 597.387,
                    "value": 597.387,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": 597.386963,
                    "value": 597.386963,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": 597.386,
                    "value": 597.386,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": 597.3832,
                    "value": 597.3832,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": 597.382,
                    "value": 597.382,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": 597.3828,
                    "value": 597.3828,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": 597.384,
                    "value": 597.384,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": 597.3836,
                    "value": 597.3836,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": 597.383,
                    "value": 597.383,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": 597.383,
                    "value": 597.383,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": 597.383,
                    "value": 597.383,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": 597.382935,
                    "value": 597.382935,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": 597.382,
                    "value": 597.382,
                    "valueType": -1,
                    "y2": "NaN"
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
                    "value": 597.396,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": 597.388367,
                    "value": 597.388367,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": 597.387,
                    "value": 597.387,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": 597.386963,
                    "value": 597.386963,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": 597.386,
                    "value": 597.386,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": 597.3832,
                    "value": 597.3832,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": 597.382,
                    "value": 597.382,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": 597.3828,
                    "value": 597.3828,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": 597.384,
                    "value": 597.384,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": 597.376,
                    "value": 597.376,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": 597.364,
                    "value": 597.364,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": 597.374634,
                    "value": 597.374634,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": 597.376,
                    "value": 597.376,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": 597.375549,
                    "value": 597.375549,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": 597.371,
                    "value": 597.371,
                    "valueType": -1,
                    "y2": "NaN"
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
                    "value": 597.353,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": 597.3581,
                    "value": 597.3581,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": 597.359,
                    "value": 597.359,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": 597.36084,
                    "value": 597.36084,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": 597.386,
                    "value": 597.386,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": 597.3832,
                    "value": 597.3832,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": 597.382,
                    "value": 597.382,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": 597.3828,
                    "value": 597.3828,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": 597.384,
                    "value": 597.384,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": 597.376,
                    "value": 597.376,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": 597.364,
                    "value": 597.364,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": 597.374634,
                    "value": 597.374634,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": 597.376,
                    "value": 597.376,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": 597.375549,
                    "value": 597.375549,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": 597.371,
                    "value": 597.371,
                    "valueType": -1,
                    "y2": "NaN"
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
                    "value": 597.1041,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": 597.1135,
                    "value": 597.1135,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": 597.1152,
                    "value": 597.1152,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": 597.115234,
                    "value": 597.115234,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": 597.1158,
                    "value": 597.1158,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": 597.11676,
                    "value": 597.11676,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": 597.1172,
                    "value": 597.1172,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": 597.1214,
                    "value": 597.1214,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": 597.127441,
                    "value": 597.127441,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": 597.128,
                    "value": 597.128,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": 597.128845,
                    "value": 597.128845,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": 597.138062,
                    "value": 597.138062,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": 597.1392,
                    "value": 597.1392,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": 597.139343,
                    "value": 597.139343,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": 597.1405,
                    "value": 597.1405,
                    "valueType": -1,
                    "y2": "NaN"
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
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
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
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
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
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
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
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
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
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": "NaN",
                    "value": "NaN",
                    "valueType": -1,
                    "y2": "NaN"
                }
            ]
        },
        {
            "type": "speedSummary",
            "data": [
                {
                    "cellType": 1,
                    "x": 0,
                    "y": 0,
                    "value": 0,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": 0,
                    "value": 0,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": 0,
                    "value": 0,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": 0,
                    "value": 0,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": 0,
                    "value": 0,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": 0,
                    "value": 0,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": 0,
                    "value": 0,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": 0,
                    "value": 0,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": 0,
                    "value": 0,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": 0,
                    "value": 0,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": 0,
                    "value": 0,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": 0,
                    "value": 0,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": 0,
                    "value": 0,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": 0,
                    "value": 0,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": 0,
                    "value": 0,
                    "valueType": -1,
                    "y2": "NaN"
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
                    "valueType": 0,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": 597.388367,
                    "value": 2,
                    "valueType": 0,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": 597.387,
                    "value": 2,
                    "valueType": 0,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": 597.386963,
                    "value": 1,
                    "valueType": 0,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": 597.386,
                    "value": 1,
                    "valueType": 0,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": 597.3832,
                    "value": 1,
                    "valueType": 0,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": 597.382,
                    "value": 1,
                    "valueType": 0,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": 597.3828,
                    "value": 1,
                    "valueType": 0,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": 597.384,
                    "value": 1,
                    "valueType": 0,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": 597.376,
                    "value": 2,
                    "valueType": 0,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": 597.364,
                    "value": 2,
                    "valueType": 0,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": 597.374634,
                    "value": 2,
                    "valueType": 0,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": 597.376,
                    "value": 2,
                    "valueType": 0,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": 597.375549,
                    "value": 2,
                    "valueType": 0,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": 597.371,
                    "value": 2,
                    "valueType": 0,
                    "y2": "NaN"
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
                    "value": 3,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": 597.388367,
                    "value": 2,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": 597.387,
                    "value": 2,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": 597.386963,
                    "value": 1,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": 597.386,
                    "value": 1,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": 597.3832,
                    "value": 1,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": 597.382,
                    "value": 1,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": 597.3828,
                    "value": 1,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": 597.384,
                    "value": 1,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": 597.376,
                    "value": 2,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": 597.364,
                    "value": 2,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": 597.374634,
                    "value": 2,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": 597.376,
                    "value": 2,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": 597.375549,
                    "value": 2,
                    "valueType": -1,
                    "y2": "NaN"
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": 597.371,
                    "value": 2,
                    "valueType": -1,
                    "y2": "NaN"
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
                    "value": -0.3338623,
                    "valueType": -1,
                    "y2": 597.4387
                },
                {
                    "cellType": 0,
                    "x": 0.085205803092608473,
                    "y": 597.1135,
                    "value": -0.326843262,
                    "valueType": -1,
                    "y2": 597.4384
                },
                {
                    "cellType": 1,
                    "x": 0.10001382414230234,
                    "y": 597.1152,
                    "value": -0.326843262,
                    "valueType": -1,
                    "y2": 597.438354
                },
                {
                    "cellType": 0,
                    "x": 0.11482184519199619,
                    "y": 597.115234,
                    "value": -0.3222046,
                    "valueType": -1,
                    "y2": 597.4383
                },
                {
                    "cellType": 1,
                    "x": 0.31995441034733291,
                    "y": 597.1158,
                    "value": -0.3222046,
                    "valueType": -1,
                    "y2": 597.4375
                },
                {
                    "cellType": 0,
                    "x": 0.52508697550266958,
                    "y": 597.11676,
                    "value": -0.317810059,
                    "valueType": -1,
                    "y2": 597.4367
                },
                {
                    "cellType": 1,
                    "x": 0.60891033030474173,
                    "y": 597.1172,
                    "value": -0.317810059,
                    "valueType": -1,
                    "y2": 597.43634
                },
                {
                    "cellType": 0,
                    "x": 0.69273368510681388,
                    "y": 597.1214,
                    "value": -0.310546875,
                    "valueType": -1,
                    "y2": 597.436035
                },
                {
                    "cellType": 1,
                    "x": 0.81404289546017272,
                    "y": 597.127441,
                    "value": -0.310546875,
                    "valueType": -1,
                    "y2": 597.4356
                },
                {
                    "cellType": 0,
                    "x": 0.93535210581353156,
                    "y": 597.128,
                    "value": -0.305175781,
                    "valueType": -1,
                    "y2": 597.435364
                },
                {
                    "cellType": 1,
                    "x": 1.1178068364676852,
                    "y": 597.128845,
                    "value": -0.305175781,
                    "valueType": -1,
                    "y2": 597.435059
                },
                {
                    "cellType": 0,
                    "x": 1.3002615671218389,
                    "y": 597.138062,
                    "value": -0.2998047,
                    "valueType": -1,
                    "y2": 597.434753
                },
                {
                    "cellType": 1,
                    "x": 1.3229394016228948,
                    "y": 597.1392,
                    "value": -0.2998047,
                    "valueType": -1,
                    "y2": 597.434753
                },
                {
                    "cellType": 0,
                    "x": 1.3456172361239505,
                    "y": 597.139343,
                    "value": -0.294494629,
                    "valueType": -1,
                    "y2": 597.4347
                },
                {
                    "cellType": 1,
                    "x": 1.6069349835347946,
                    "y": 597.1405,
                    "value": -0.294494629,
                    "valueType": -1,
                    "y2": 597.434265
                }
            ]
        }
    ],
    "Code": 0,
    "Message": "success"
}
"""
	
