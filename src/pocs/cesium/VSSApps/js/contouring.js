
function getSlopeContourMaterial() {
    // Creates a composite material with both slope shading and contour lines
    return new Cesium.Material({
        fabric: {
            type: 'SlopeColorContour',
            materials: {
                contourMaterial: {
                    type: 'ElevationContour'
                },
                slopeRampMaterial: {
                    type: 'SlopeRamp'
                }
            },
            components: {
                diffuse: 'contourMaterial.alpha == 0.0 ? slopeRampMaterial.diffuse : contourMaterial.diffuse',
                alpha: 'max(contourMaterial.alpha, slopeRampMaterial.alpha)'
            }
        },
        translucent: false
    });
}


function getColorRamp(selectedShading) {
    var ramp = document.createElement('canvas');
    ramp.width = 100;
    ramp.height = 1;
    var ctx = ramp.getContext('2d');

    var values = selectedShading === 'elevation' ? elevationRamp : slopeRamp;

    var grd = ctx.createLinearGradient(0, 0, 100, 0);
    grd.addColorStop(values[0], '#000000'); //black
    grd.addColorStop(values[1], '#2747E0'); //blue
    grd.addColorStop(values[2], '#D33B7D'); //pink
    grd.addColorStop(values[3], '#D33038'); //red
    grd.addColorStop(values[4], '#FF9742'); //orange
    grd.addColorStop(values[5], '#ffd700'); //yellow
    grd.addColorStop(values[6], '#ffffff'); //white

    ctx.fillStyle = grd;
    ctx.fillRect(0, 0, 100, 1);

    return ramp;
}


function getElevationContourMaterial() {
    // Creates a composite material with both elevation shading and contour lines
    return new Cesium.Material({
        fabric: {
            type: 'ElevationColorContour',
            materials: {
                contourMaterial: {
                    type: 'ElevationContour'
                },
                elevationRampMaterial: {
                    type: 'ElevationRamp'
                }
            },
            components: {
                diffuse: 'contourMaterial.alpha == 0.0 ? elevationRampMaterial.diffuse : contourMaterial.diffuse',
                alpha: 'max(contourMaterial.alpha, elevationRampMaterial.alpha)'
            }
        },
        translucent: false
    });
}


function updateMaterial() {
    var hasContour = viewModel.enableContour;
    var selectedShading = viewModel.selectedShading;
    var globe = viewer.scene.globe;
    var material;
    if (hasContour) {
        if (selectedShading === 'elevation') {
            material = getElevationContourMaterial();
            shadingUniforms = material.materials.elevationRampMaterial.uniforms;
            shadingUniforms.minHeight = minHeight;
            shadingUniforms.maxHeight = maxHeight;
            contourUniforms = material.materials.contourMaterial.uniforms;
        } else if (selectedShading === 'slope') {
            material = getSlopeContourMaterial();
            shadingUniforms = material.materials.slopeRampMaterial.uniforms;
            contourUniforms = material.materials.contourMaterial.uniforms;
        } else {
            material = Cesium.Material.fromType('ElevationContour');
            contourUniforms = material.uniforms;
        }
        contourUniforms.width = viewModel.contourWidth;
        contourUniforms.spacing = viewModel.contourSpacing;
        contourUniforms.color = contourColor;
    } else if (selectedShading === 'elevation') {
        material = Cesium.Material.fromType('ElevationRamp');
        shadingUniforms = material.uniforms;
        shadingUniforms.minHeight = minHeight;
        shadingUniforms.maxHeight = maxHeight;
    } else if (selectedShading === 'slope') {
        material = Cesium.Material.fromType('SlopeRamp');
        shadingUniforms = material.uniforms;
    }
    if (selectedShading !== 'none') {
        shadingUniforms.image = getColorRamp(selectedShading);
    }

    globe.material = material;
}


