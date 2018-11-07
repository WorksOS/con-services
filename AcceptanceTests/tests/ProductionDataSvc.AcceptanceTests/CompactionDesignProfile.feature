Feature: CompactionDesignProfile
  I should be able to request Compaction Design Profile data.

Scenario Outline: Compaction Get Slicer Design Profile
  Given only the service route "/api/v2/profiles/design/slicer"
  And with parameter "projectUid" with value "7925f179-013d-4aaf-aff4-7b9833bb06d6"
  And with parameter "startLatDegrees" with value "36.207310"
  And with parameter "startLonDegrees" with value "-115.019584"
  And with parameter "endLatDegrees" with value "36.207322"
  And with parameter "endLonDegrees" with value "-115.019574"
  And with parameter "importedFileUid" with value "220e12e5-ce92-4645-8f01-1942a2d5a57f"
  When I send the GET request I expect response code 200
  Then the response should be:
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

Scenario Outline: Compaction Get Slicer Empty Design Profile
  Given only the service route "/api/v2/profiles/design/slicer"
  And with parameter "projectUid" with value "7925f179-013d-4aaf-aff4-7b9833bb06d6"
  And with parameter "startLatDegrees" with value "36.207310"
  And with parameter "startLonDegrees" with value "-115.019584"
  And with parameter "endLatDegrees" with value "36.207322"
  And with parameter "endLonDegrees" with value "-115.019574"
  And with parameter "importedFileUid" with value "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff"
  When I send the GET request I expect response code 200
  Then the response should be:
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
#Until solved problem with variance in values returned from Raptor. Tests may have to compensate for inaccuraces at high decimal precion.
#@Ignore

Scenario Outline: Compaction Get Slicer Design Profile With Added Endpoints
  Given only the service route "/api/v2/profiles/design/slicer"
  And with parameter "projectUid" with value "7925f179-013d-4aaf-aff4-7b9833bb06d6"
  And with parameter "startLatDegrees" with value "36.207250"
  And with parameter "startLonDegrees" with value "-115.019584"
  And with parameter "endLatDegrees" with value "36.207322"
  And with parameter "endLonDegrees" with value "-115.019574"
  And with parameter "importedFileUid" with value "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff"
  When I send the GET request I expect response code 200
  Then the response should be:
  """
  {
    "gridDistanceBetweenProfilePoints": 8.0405782499336258,
    "results": [
      {
        "designFileUid": "dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff",
        "data": [
          {
            "x": 0.0,
            "y": "NaN"
          },
          {
            "x": 1.4989359778937124,
            "y": 597.107849
          },
          {
            "x": 2.4363884597116843,
            "y": 597.317444
          },
          {
            "x": 3.0398711095204827,
            "y": 597.4535
          },
          {
            "x": 3.8040219492274239,
            "y": 597.46875
          },
          {
            "x": 4.3387533320107821,
            "y": 597.4797
          },
          {
            "x": 5.2303524440560505,
            "y": 597.466736
          },
          {
            "x": 5.5624914389921845,
            "y": 597.4633
          },
          {
            "x": 6.7969372766692437,
            "y": 597.4468
          },
          {
            "x": 7.7815432993860476,
            "y": 597.437439
          },
          {
            "x": 8.0405782499336258,
            "y": 597.434265
          }
        ]
      }
    ],
    "Code": 0,
    "Message": "success"
  }
  """