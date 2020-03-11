FROM atlassian/bamboo-agent-base
USER root
RUN apt-get update && \
    apt-get install git wget vim -y && \
    apt-get install python3.6 python3-pip -y 

ENV DOWNLOAD_URL=https://bamboo.trimble.tools/agentServer/agentInstaller/atlassian-bamboo-agent-installer-6.7.2.jar
ENV AGENT_JAR=${BAMBOO_USER_HOME}/atlassian-bamboo-agent-installer.jar

RUN echo "Download ${DOWNLOAD_URL}"
RUN set -x && \
    rm -f ${AGENT_JAR} && \
    curl -L --silent --output ${AGENT_JAR} ${DOWNLOAD_URL} && \
    mkdir -p ${BAMBOO_USER_HOME}/bamboo-agent-home/bin

RUN pip3 install python-jenkins

USER ${BAMBOO_USER}
RUN ${BAMBOO_USER_HOME}/bamboo-update-capability.sh "system.git.executable" /usr/bin/git
RUN ${BAMBOO_USER_HOME}/bamboo-update-capability.sh "team" "merino"
RUN ${BAMBOO_USER_HOME}/bamboo-update-capability.sh "cluster" "fsm-dev"