#######################################
#
# To run locally
# > docker build -t bamboo-agent .
# > docker run -it --privileged -v <directory-you-want-to-mount>:/home/bamboo/bamboo-agent-home/ --entrypoint "/bin/bash" bamboo-agent
#
#
# Once running bash inside the container, run the following to enable docker
# > sudo /usr/local/bin/wrapdocker.sh
#
########################################

FROM atlassian/bamboo-agent-base

#### Install packages for builds

USER root
RUN apt-get update && \
    apt-get install git wget vim sudo dos2unix unzip -y && \
    apt-get install python3.6 python3-pip nodejs npm -y 

# Python is used to run the Jenkins build script
RUN pip3 install python-jenkins

# AWS is used to login to ECR repos to push docker iamges
RUN curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip" && \
    unzip awscliv2.zip && \
    sudo ./aws/install

#### END OF Install packages

#### SETUP AGENT

RUN curl https://raw.githubusercontent.com/helm/helm/master/scripts/get-helm-3 | bash

ENV DOWNLOAD_URL=https://bamboo.trimble.tools/agentServer/agentInstaller/atlassian-bamboo-agent-installer-6.7.2.jar
ENV AGENT_JAR=${BAMBOO_USER_HOME}/atlassian-bamboo-agent-installer.jar

RUN echo "Download ${DOWNLOAD_URL}"
RUN set -x && \
    rm -f ${AGENT_JAR} && \
    curl -L --silent --output ${AGENT_JAR} ${DOWNLOAD_URL} && \
    mkdir -p ${BAMBOO_USER_HOME}/bamboo-agent-home/bin

#### END OF AGENT

#### INSTALL DOCKER
RUN apt-get install \
    apt-transport-https \
    ca-certificates \
    curl \
    gnupg-agent \
    software-properties-common -y

RUN curl -fsSL https://download.docker.com/linux/ubuntu/gpg | apt-key add -
RUN apt-key fingerprint 0EBFCD88

RUN add-apt-repository \
   "deb [arch=amd64] https://download.docker.com/linux/ubuntu \
   $(lsb_release -cs) \
   stable"
RUN apt-get update
RUN apt-get install docker-ce docker-ce-cli containerd.io -y

# from DIND image
# https://github.com/jpetazzo/dind
# Modified to not create a shell, just setup docker (hence why it's in source)
ADD ./wrapdocker.sh /usr/local/bin/wrapdocker.sh
RUN chmod +x /usr/local/bin/wrapdocker.sh

VOLUME /var/lib/docker
RUN usermod -aG docker ${BAMBOO_USER}
RUN echo "${BAMBOO_USER} ALL=(ALL) NOPASSWD:ALL" >> /etc/sudoers

### END OF DOCKER 

RUN wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN add-apt-repository universe && apt-get update
RUN apt-get install dotnet-sdk-3.1 libgdiplus -y

### Run Agent specific commands as bamboo here

USER ${BAMBOO_USER}
RUN ${BAMBOO_USER_HOME}/bamboo-update-capability.sh "system.git.executable" "/usr/bin/git"
RUN ${BAMBOO_USER_HOME}/bamboo-update-capability.sh "system.docker.executable" "/usr/bin/docker"
RUN ${BAMBOO_USER_HOME}/bamboo-update-capability.sh "system.dotnet.executable" "/usr/bin/dotnet"
RUN ${BAMBOO_USER_HOME}/bamboo-update-capability.sh "team" "merino"
RUN ${BAMBOO_USER_HOME}/bamboo-update-capability.sh "cluster" "fsm-dev"

# Setup the agent script to enable docker by calling 'wrapdocker'
RUN mv ./runAgent.sh runAgent-nodocker.sh

# Find the last line (java command for agent), and insert the wrapdocker before it
# https://unix.stackexchange.com/questions/356312/sed-insert-something-after-the-second-last-line
RUN sed '$isudo wrapdocker.sh' ./runAgent-nodocker.sh > ./runAgent.sh 
# Make sure we ca nexecute our new file
RUN chmod +x ./runAgent.sh
ENTRYPOINT ["./runAgent.sh"]