FROM microsoft/dotnet:2.1-sdk-alpine

#Copy files from scm into build container and build
COPY . /build/
WORKDIR /build

####### TODO run tests

# Build 

RUN chmod 777 *.sh

RUN ["/bin/sh", "build.sh"]
RUN ["/bin/sh", "unittests.sh"]

FROM microsoft/dotnet:2.1-runtime-alpine

# Set the Working Directory
WORKDIR /app

RUN rm -rf /build

# Configure the listening port to 80
ENV ASPNETCORE_URLS http://*:80
EXPOSE 80

# Copy the app
COPY ./artifacts/ProjectWebApi /app

# Create the mount point to hold volume from host for logs
VOLUME logs

# Start the app
ENTRYPOINT dotnet VSS.MasterData.Project.WebAPI.dll
