FROM microsoft/dotnet:2.1.300-rc1-sdk-alpine3.7

#Copy files from scm into build container and build
COPY . /build/

####### TODO run tests

# Build 
RUN dotnet build /build/TRex.netstandard.sln --output /trex

#Now create runtime container
FROM microsoft/dotnet:2.1.0-rc1-runtime-alpine3.7
RUN \
  apk update && \
  apk upgrade && \
  apk add openjdk8 && \
  apk add bash && \
  rm -rf /var/cache/apk/*

#Need these for ignite to work
ENV JAVA_HOME=/usr/lib/jvm/java-1.8-openjdk
ENV LD_LIBRARY_PATH=$JAVA_HOME/jre/lib/amd64/server

# Copy built artifacts from last stage into runtime container
COPY --from=0 /trex/ /trex/
COPY --from=0 /root/.nuget /root/.nuget



