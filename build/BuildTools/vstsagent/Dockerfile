FROM microsoft/vsts-agent:ubuntu-16.04

# Install basic command-line utilities
RUN apt-get update \
   && apt-get install -y --no-install-recommends \
   curl \
   dnsutils \
   file \
   ftp \
   iproute2 \
   iputils-ping \
   locales \
   openssh-client \
   rsync\
   shellcheck \
   sudo \
   telnet \
   time \
   unzip \
   wget \
   zip \
   tzdata 

# Setup the locale
ENV LANG en_US.UTF-8
ENV LC_ALL $LANG
RUN locale-gen $LANG \
   && update-locale

# Accept EULA - needed for certain Microsoft packages like SQL Server Client Tools
ENV ACCEPT_EULA=Y

# Install Helm
RUN curl https://raw.githubusercontent.com/helm/helm/master/scripts/get-helm-3 | bash

# Install kubectl
RUN curl -LO https://storage.googleapis.com/kubernetes-release/release/$(curl -s https://storage.googleapis.com/kubernetes-release/release/stable.txt)/bin/linux/amd64/kubectl \
   && chmod +x ./kubectl \
   && mv ./kubectl /usr/local/bin/kubectl

# Install Powershell Core
RUN curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
   && curl https://packages.microsoft.com/config/ubuntu/16.04/prod.list | tee /etc/apt/sources.list.d/microsoft.list \
   && apt-get update \
   && apt-get install -y --no-install-recommends \
   powershell \
   && rm -rf /var/lib/apt/lists/* \
   && rm -rf /etc/apt/sources.list.d/*

# Clean system
RUN apt-get clean \
   && rm -rf /var/lib/apt/lists/* \
   && rm -rf /etc/apt/sources.list.d/*

ENV DOCKER_CHANNEL stable
ENV DOCKER_VERSION 18.06.1-ce

RUN set -ex \
   && curl -fL "https://download.docker.com/linux/static/${DOCKER_CHANNEL}/`uname -m`/docker-${DOCKER_VERSION}.tgz" -o docker.tgz \
   && tar --extract --file docker.tgz --strip-components 1 --directory /usr/local/bin \
   && rm docker.tgz \
   && docker -v

ENV DOCKER_COMPOSE_VERSION 1.22.0

RUN set -x \
   && curl -fSL "https://github.com/docker/compose/releases/download/$DOCKER_COMPOSE_VERSION/docker-compose-`uname -s`-`uname -m`" -o /usr/local/bin/docker-compose \
   && chmod +x /usr/local/bin/docker-compose \
   && docker-compose -v

# Setup dotnet
RUN wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN add-apt-repository universe && apt-get update
RUN apt-get install dotnet-sdk-3.1 libgdiplus -y

# RUN helm init --client-only
# RUN helm plugin install https://github.com/chartmuseum/helm-push
# RUN helm repo add chartmuseum http://chartmuseum.k8s.vspengg.com
RUN apt-get update && apt-get install -y apt-transport-https python3 python3-pip python3-setuptools

RUN pip3 install --upgrade pip
RUN pip3 install python-jenkins

# AWS is used to login to ECR repos to push docker iamges
RUN curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip" && \
   unzip awscliv2.zip && \
   sudo ./aws/install

#Install nodejs
RUN curl -sL https://deb.nodesource.com/setup_10.x | bash -
RUN apt-get update && apt-get install -y nodejs

# Clean system
RUN apt-get clean \
   && rm -rf /var/lib/apt/lists/* \
   && rm -rf /etc/apt/sources.list.d/*
