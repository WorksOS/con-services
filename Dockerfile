FROM microsoft/dotnet:2.1-sdk-alpine

#Copy files from scm into build container and build
COPY . /build/
WORKDIR /build

####### TODO run tests

# Build 

RUN chmod 775 *.sh
RUN ["/bin/bash", "rm_cr.sh"]

RUN ["/bin/bash", "build.sh"]
RUN ["/bin/bash", "unittests.sh"]

WORKDIR /build/AcceptanceTests/scripts
RUN chmod 775 *.sh
RUN ["/bin/bash","deploy_linux.sh"]

