function createPoint(worldPosition) {
    var point = viewer.entities.add({
        position : worldPosition,
        point : {
            color : Cesium.Color.WHITE,
            pixelSize : 5,
            heightReference: Cesium.HeightReference.CLAMP_TO_GROUND
        }
    });
    return point;
}

function drawShape(positionData) {
    var shape;
    if (drawingMode === 'line') {
        shape = viewer.entities.add({
            polyline : {
                positions : positionData,
                clampToGround : true,
                width : 3
            }
        });
    }
    else if (drawingMode === 'polygon') {
        shape = viewer.entities.add({
            polygon: {
                hierarchy: positionData,
                material: new Cesium.ColorMaterialProperty(Cesium.Color.WHITE.withAlpha(0.5))
            }
        });
    }
    return shape;
}

function terminateShape() {
    activeShapePoints.pop();
    drawShape(activeShapePoints);
    viewer.entities.remove(floatingPoint);
    viewer.entities.remove(activeShape);
    floatingPoint = undefined;
    activeShape = undefined;
    activeShapePoints = [];
}
