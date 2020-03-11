FROM 940327799086.dkr.ecr.us-west-2.amazonaws.com/rpd-ccss-base-images:alpine-dind

RUN apk add supervisor
ADD supervisord.conf /etc/supervisor/conf.d/supervisord.conf

COPY *.sh /usr/local/bin/
RUN chmod 755 /usr/local/bin/*.sh
CMD ["super-entrypoint.sh"]
