FROM dhoer/flyway:4.1.2

RUN mkdir -p /scripts/sql
COPY *.sql /scripts/sql/
COPY *.sh /scripts/
RUN chmod 555 /scripts/*
WORKDIR /scripts

ENTRYPOINT ["/bin/bash","/scripts/runme.sh"]