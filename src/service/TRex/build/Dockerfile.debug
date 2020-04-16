FROM 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:trex-3.1-build as build_container
ARG SERVICE_PATH
ARG COMPONENT
ARG DEBUG=false

# We are contructing the build container and performing the build
COPY . /build/
WORKDIR /build

#publish TRex servers
RUN if [ "$COMPONENT" = "ApplicationServer" -o "$DEBUG" != "" ] ; then dotnet publish ./$SERVICE_PATH/src/netstandard/services/VSS.TRex.Server.Application/VSS.TRex.Server.Application.csproj --output /trex/ApplicationServer ; fi ; \
 if [ "$COMPONENT" = "DesignElevation" -o "$DEBUG" != "" ] ; then dotnet publish ./$SERVICE_PATH/src/netstandard/services/VSS.TRex.Server.DesignElevation/VSS.TRex.Server.DesignElevation.csproj --output /trex/DesignElevation ; fi ; \
 if [ "$COMPONENT" = "MutableData" -o "$DEBUG" != "" ] ; then dotnet publish ./$SERVICE_PATH/src/netstandard/services/VSS.TRex.Server.MutableData/VSS.TRex.Server.MutableData.csproj --output /trex/MutableData ; fi ; \
 if [ "$COMPONENT" = "PSNode" -o "$DEBUG" != "" ] ; then dotnet publish ./$SERVICE_PATH/src/netstandard/services/VSS.TRex.Server.PSNode/VSS.TRex.Server.PSNode.csproj --output /trex/PSNode ; fi ; \
 if [ "$COMPONENT" = "QMesh" -o "$DEBUG" != "" ] ; then dotnet publish ./$SERVICE_PATH/src/netstandard/services/VSS.TRex.Server.QuantizedMesh/VSS.TRex.Server.QuantizedMesh.csproj --output /trex/QMesh ; fi ; \
 if [ "$COMPONENT" = "TileRendering" -o "$DEBUG" != "" ] ; then dotnet publish ./$SERVICE_PATH/src/netstandard/services/VSS.TRex.Server.TileRendering/VSS.TRex.Server.TileRendering.csproj --output /trex/TileRendering ; fi ; \
 if [ "$COMPONENT" = "TINSurfaceExport" -o "$DEBUG" != "" ] ; then dotnet publish ./$SERVICE_PATH/src/netstandard/services/VSS.TRex.Server.TINSurfaceExport/VSS.TRex.Server.TINSurfaceExport.csproj --output /trex/TINSurfaceExport ; fi ; \
 if [ "$COMPONENT" = "Reports" -o "$DEBUG" != "" ] ; then dotnet publish ./$SERVICE_PATH/src/netstandard/services/VSS.TRex.Server.Reports --output /trex/Reports ; fi ; \
 if [ "$COMPONENT" = "ConnectedSiteGateway" -o "$DEBUG" != "" ] ; then dotnet publish ./$SERVICE_PATH/src/gateway/VSS.TRex.ConnectedSite.Gateway.WebApi/VSS.TRex.ConnectedSite.Gateway.WebApi.csproj --output /trex/ConnectedSiteGateway ; fi ; \
 if [ "$COMPONENT" = "Gateway" -o "$DEBUG" != "" ] ; then dotnet publish ./$SERVICE_PATH/src/gateway/VSS.TRex.Gateway.WebApi/VSS.TRex.Gateway.WebApi.csproj --output /trex/Gateway ; fi ; \
 if [ "$COMPONENT" = "MutableGateway"-o "$DEBUG" != "" ] ; then dotnet publish ./$SERVICE_PATH/src/gateway/VSS.TRex.Mutable.Gateway.WebApi/VSS.TRex.Mutable.Gateway.WebApi.csproj --output /trex/MutableGateway ; fi ; \
## if [ "$COMPONENT" = "Webtools" -o "$DEBUG" != "" ] ; then dotnet publish ./$SERVICE_PATH/src/tools/VSS.TRex.Webtools/VSS.TRex.Webtools.csproj --output /trex/Webtools ; fi ; \
 if [ "$COMPONENT" = "Activator" -o "$DEBUG" != "" ] ; then dotnet publish ./$SERVICE_PATH/src/tools/VSS.TRex.GridActivator/VSS.TRex.GridActivator.csproj --output /trex/Utils/Activator ; fi ; \
 if [ "$COMPONENT" = "Deployer" -o "$DEBUG" != "" ] ; then dotnet publish ./$SERVICE_PATH/src/tools/VSS.TRex.Service.Deployer/VSS.TRex.Service.Deployer.csproj --output /trex/Utils/Deployer ; fi


#Gather dependencies - kubernetes support (and its dependencies) have to be injected here as its not part of the nuget package
RUN cp -r /root/.nuget/packages/apache.ignite/2.8.0/build/output/libs/ /trex/libs/
COPY ./$SERVICE_PATH/build/ignite-kubernetes-2.8.0.jar /trex/libs/

WORKDIR /trex/libs/
RUN wget https://repo1.maven.org/maven2/com/fasterxml/jackson/core/jackson-core/2.9.8/jackson-core-2.9.8.jar ; \
    wget https://repo1.maven.org/maven2/com/fasterxml/jackson/core/jackson-annotations/2.9.8/jackson-annotations-2.9.8.jar ; \
    wget https://repo1.maven.org/maven2/com/fasterxml/jackson/core/jackson-databind/2.9.8/jackson-databind-2.9.8.jar ; \
    wget https://repo1.maven.org/maven2/io/prometheus/jmx/jmx_prometheus_javaagent/0.12.0/jmx_prometheus_javaagent-0.12.0.jar

# wget http://repo1.maven.org/maven2/log4j/log4j/1.2.17/log4j-1.2.17.jar
# wget http://repo1.maven.org/maven2/org/apache/ignite/ignite-log4j/2.7.0/ignite-log4j-2.7.0.jar
# wget http://repo1.maven.org/maven2/org/springframework/spring-aop/5.0.8.RELEASE/spring-aop-5.0.8.RELEASE.jar
# wget http://repo1.maven.org/maven2/org/springframework/spring-expression/5.0.8.RELEASE/spring-expression-5.0.8.RELEASE.jar
# wget https://repo1.maven.org/maven2/org/springframework/spring-jdbc/5.0.8.RELEASE/spring-jdbc-5.0.8.RELEASE.jar

# RUN rm spring*4.3.18*
#TRex unit tests will be tested by the jenkins file as host directory mounts cannot be specified in the dockerfile

WORKDIR /trex

#We are now constructing the run time container

# Now create runtime container
# Why bash? it seems to be needed for ignite to run (maybe a dependency?)
FROM 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:trex-3.1-runtime as runtime_container
ARG COMPONENT

ENV TPAAS_AUTH_URL=https://identity-stg.trimble.com/i/oauth2
ENV TPAAS_APP_TOKEN=MGh1X25tYXlEQWFkMFdpY1hDekVHVTE3U2ZVYTppVWN3eEZ1cFRDRWFsaFVFOTRwWGhkVVNEa0Vh
ENV COORDINATE_SERVICE_URL=https://api-stg.trimble.com/t/trimble.com/coordinates/1.0

#Bring libs accross TODO: make the common part of this a new image to speed build times
COPY --from=build_container /trex/libs/ /trex/libs/

WORKDIR /trex

# Make sure this is done in the final container
RUN touch prometheusConfig.yaml

# Copy built artifacts from last stage into runtime container
COPY --from=build_container /trex/${COMPONENT}/ /trex/

