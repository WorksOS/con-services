
FROM dhoer/flyway:4.1.2

RUN mkdir -p /scriptsMs/sql
COPY *.jar /flyway/jars
COPY *.sql /scriptsMs/sql/
COPY *.sh /scriptsMs/
RUN chmod 555 /scriptsMs/*
WORKDIR /scriptsMs

ENTRYPOINT ["/bin/bash","/scriptsMs/runme.sh"]