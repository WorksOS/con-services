FROM microsoft/dotnet:2.0-sdk
COPY . /app
WORKDIR /app

#RUN \
#  apk update && \
#  apk upgrade && \
#  apk add bash && \
#  rm -rf /var/cache/apk/*

# Create the mount point to hold volume from host for test results
VOLUME testresults

ENTRYPOINT ["/bin/bash", "runtests.sh"]
