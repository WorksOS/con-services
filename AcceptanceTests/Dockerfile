#FROM microsoft/dotnet:nanoserver
#FROM microsoft/nanoserver
FROM microsoft/windowsservercore
COPY . /app
WORKDIR /app

# Create the mount point to hold volume from host for test results
#VOLUME testresults

ENTRYPOINT ["cmd", "runtests.bat"]
