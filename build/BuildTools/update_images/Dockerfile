FROM 300213723870.dkr.ecr.us-west-2.amazonaws.com/base-images:alpine-dind

RUN apk add supervisor
ADD supervisord.conf /etc/supervisor/conf.d/supervisord.conf

COPY *.sh /usr/local/bin/ 
RUN chmod 755 /usr/local/bin/*.sh
CMD ["super-entrypoint.sh"]
