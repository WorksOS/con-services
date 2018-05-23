FROM microsoft/dotnet:2.1.300-rc1-sdk-alpine3.7

#Copy files from scm into build container and build
COPY . /build/
WORKDIR /build
RUN dotnet build testApp.sln