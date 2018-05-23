FROM microsoft/dotnet:2.1-sdk-alpine

#Copy files from scm into build container and build
COPY . /build/

####### TODO run tests

# Build 
RUN dotnet test /build/VSS.Visionlink.Project.sln 
RUN dotnet publish /build/VSS.Visionlink.Project.sln --output /artifacts
RUN /build/AcceptanceTests/scripts/deploy_linux.sh

