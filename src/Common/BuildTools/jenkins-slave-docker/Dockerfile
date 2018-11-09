FROM jenkins/jnlp-slave:alpine

USER root

RUN apk update

#install AWS CLI
RUN set -x \
    && apk add git bash curl openssh-server wget unzip python

RUN wget "s3.amazonaws.com/aws-cli/awscli-bundle.zip" -O "awscli-bundle.zip" && \
    unzip awscli-bundle.zip && \
    ./awscli-bundle/install -i /usr/local/aws -b /usr/local/bin/aws && \
    rm awscli-bundle.zip && \
    rm -rf awscli-bundle

#WHY
EXPOSE 22

# Install docker
RUN apk add --no-cache \
	openssh-client \
	docker


# https://github.com/docker/docker/blob/master/project/PACKAGERS.md#runtime-dependencies
RUN set -eux; \
	apk add --no-cache \
		btrfs-progs \
		e2fsprogs \
		e2fsprogs-extra \
		iptables \
		xfsprogs \
		xz \
# pigz: https://github.com/moby/moby/pull/35697 (faster gzip implementation)
		pigz \
	; \
# only install zfs if it's available for the current architecture
# https://git.alpinelinux.org/cgit/aports/tree/main/zfs/APKBUILD?h=3.6-stable#n9 ("all !armhf !ppc64le" as of 2017-11-01)
# "apk info XYZ" exits with a zero exit code but no output when the package exists but not for this arch
	if zfs="$(apk info --no-cache --quiet zfs)" && [ -n "$zfs" ]; then \
		apk add --no-cache zfs; \
	fi

# set up subuid/subgid so that "--userns-remap=default" works out-of-the-box
RUN set -x \
	&& addgroup -S dockremap \
	&& adduser -S -G dockremap dockremap \
	&& echo 'dockremap:165536:65536' >> /etc/subuid \
	&& echo 'dockremap:165536:65536' >> /etc/subgid

# https://github.com/docker/docker/tree/master/hack/dind
ENV DIND_COMMIT 52379fa76dee07ca038624d639d9e14f4fb719ff

RUN set -ex; \
	apk add --no-cache --virtual .fetch-deps libressl; \
	wget -O /usr/local/bin/dind "https://raw.githubusercontent.com/docker/docker/${DIND_COMMIT}/hack/dind"; \
	chmod +x /usr/local/bin/dind; \
	apk del .fetch-deps

COPY dockerd-entrypoint.sh /usr/local/bin/

VOLUME /var/lib/docker
EXPOSE 2375

#Install shadow for usermod tools
RUN apk add --no-cache\
		shadow

#Add jenkins user to docker group
RUN usermod -aG docker jenkins

RUN apk add supervisor
ADD supervisord.conf /etc/supervisor/conf.d/supervisord.conf

RUN mkdir /etc/docker && echo  '{ "insecure-registries":["registry-docker-registry.jenkins:5000"] }' > /etc/docker/daemon.json
RUN tail /etc/docker/daemon.json

#Try this out
COPY *.sh /usr/local/bin/ 
COPY *.tar /usr/local/bin/ 

RUN chmod 755 /usr/local/bin/*.sh

# Supervisor does not support entrypoint must use CMD
# https://stackoverflow.com/questions/22465003/error-positional-arguments-are-not-supported
#CMD ["/usr/bin/supervisord"]
ENTRYPOINT ["super-entrypoint.sh"]
#CMD ["super-entrypoint.sh"]
#ENTRYPOINT ["bash", "-c", "dockerd-entrypoint.sh", "&&", "bash"]
#CMD ["dockerd-entrypoint.sh"]