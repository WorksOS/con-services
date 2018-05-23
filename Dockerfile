FROM microsoft/dotnet:2.1-sdk-alpine

#Copy files from scm into build container and build
COPY . /build/
WORKDIR /build

####### TODO run tests

# Build 

RUN chmod 777 *.sh

RUN ["/bin/sh", "build.sh"]
RUN ["/bin/sh", "unittests.sh"]

WORKDIR /build/AcceptanceTests/scripts
RUN chmod 777 *.sh
RUN ["/bin/sh", "deploy_linux.sh"]

