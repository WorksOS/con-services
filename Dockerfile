FROM microsoft/dotnet:2.1-sdk-alpine

#Copy files from scm into build container and build
COPY . /build/

####### TODO run tests

# Build 
RUN /build/build.sh
RUN /build/unittests.sh
RUN /build/AcceptanceTests/scripts/deploy_linux.sh

