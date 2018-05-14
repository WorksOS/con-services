FROM microsoft/dotnet:2.1.300-rc1-sdk-alpine3.7
RUN \
  apk update && \
  apk upgrade && \
  apk add openjdk8 && \
  apk add bash && \
  rm -rf /var/cache/apk/*

ENV JAVA_HOME=/usr/lib/jvm/java-1.8-openjdk
ENV LD_LIBRARY_PATH=$JAVA_HOME/jre/lib/amd64/server

COPY . /build/
RUN dotnet build /build/TRex.Framework.netstandard.sln --output /trex

#copy log4net to build manually for the moment
RUN cp /build/log4net.xml /trex

#Remove build folder from image
RUN rm -r /build

