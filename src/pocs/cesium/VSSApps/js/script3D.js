function doPosition2() {
    viewModel.showLatLonPosition = !viewModel.showLatLonPosition;
}	
    
function shomInspector() {
    viewer.extend(Cesium.viewerCesiumInspectorMixin);  
}		
    
function zoomToDimensions() {
    viewer.scene.camera.flyTo({
        destination : Cesium.Cartesian3.fromDegrees(-115.020839, 36.204504,700),
        orientation : {
            heading : Cesium.Math.toRadians(30),
            pitch : Cesium.Math.toRadians(-15),
            roll : 0.0
        }
        });
}


function doPosition() {
  viewModel.showLatLonPosition = !viewModel.showLatLonPosition;
}	
    
function doShading() {
  if (	viewModel.selectedShading == 'elevation')
    viewModel.selectedShading = '';
  else
    viewModel.selectedShading = 'elevation';
  updateMaterial();
}		


function doProfile() {
  terminateShape();
  if (viewModel.selectedPicker == '')
  {
    viewModel.selectedPicker = 'line';
    drawingMode = 'line';
  }
  else if (viewModel.selectedPicker == 'line')
  {
    viewModel.selectedPicker = 'area';
    drawingMode = 'polygon';
  }
  else
    viewModel.selectedPicker = '';
}		


function drawCorridor() {
  var e = viewer.entities.add({
  corridor : {
    positions : Cesium.Cartesian3.fromDegreesArray([
        -122.19, 46.1914,
        -122.21, 46.21,
        -122.23, 46.21
    ]),
    width : 200.0,
    material : Cesium.Color.GREEN.withAlpha(0.5)
    }
  });

  viewer.zoomTo(e);
}
    


function drawTexturedPolygon() {
  if (!Cesium.Entity.supportsMaterialsforEntitiesOnTerrain(viewer.scene)) {
      window.alert('Terrain Entity materials are not supported on this platform');
      return;
      }

    var e = viewer.entities.add({
        polygon : {
            hierarchy : {
                positions : [new Cesium.Cartesian3(-2358138.847340281, -3744072.459541374, 4581158.5714175375),
                             new Cesium.Cartesian3(-2357231.4925370603, -3745103.7886602185, 4580702.9757762635),
                             new Cesium.Cartesian3(-2355912.902205431, -3744249.029778454, 4582402.154378103),
                             new Cesium.Cartesian3(-2357208.0209552636, -3743553.4420488174, 4581961.863286629)]
            },
            material : '../../../../Apps/SampleData/vss/Cesium_Logo_Color.jpg',
            classificationType : Cesium.ClassificationType.TERRAIN,
            stRotation : Cesium.Math.toRadians(05)
        }
    });

    viewer.zoomTo(e);
}

function showBillboard() {
    var e = viewer.entities.add({
        position : Cesium.Cartesian3.fromDegrees(-122.1958, 46.1915),
        billboard : {
                        image : '../../../../Apps/SampleData/vss/ylw-pushpin.png',
            heightReference : Cesium.HeightReference.CLAMP_TO_GROUND
        }
    });

    viewer.trackedEntity = e;

}
        

function showContour() {
    viewModel.enableContour = !viewModel.enableContour;
    updateMaterial();
};
