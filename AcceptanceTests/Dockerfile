FROM microsoft/dotnet:1.1-runtime
COPY . /app
WORKDIR /app

# Create the mount point to hold volume from host for test results
VOLUME testresults

ENTRYPOINT ["cmd", "runtests.bat"]
