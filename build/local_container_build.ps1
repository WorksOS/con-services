#Use this script to build images locally on a development pc.
#This should be run from the repository root.
docker build -t trex:buildimage -f ./build/DockerfileBuild .
$trexComponents = (
    "ApplicationServer",
    "DesignElevation",
    "MutableData",
    "PSNode",
    "TileRendering",
    "TINSurfaceExport",
    "Gateway",
    "MutableGateway",
    "Webtools",
    "Utils"
)

Foreach ($component in $trexComponents) {
    Write-Host "Building $component"
    docker build -t trex:app.$component -f ./build/DockerfileRuntime --build-arg BUILD_CONTAINER=trex:buildimage --build-arg COMPONENT=$component ./build
}