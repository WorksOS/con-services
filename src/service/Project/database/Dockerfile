
FROM dhoer/flyway:4.1.2

RUN mkdir -p /scripts/sql
#COPY *.sql /scripts/sql/
COPY *.sh /scripts/
RUN chmod 555 /scripts/*
WORKDIR /scripts

# Remove CR (Carrage Return '\r') in sh files
RUN sed -i 's/\r$//' rm_cr.sh
RUN chmod 775 rm_cr.sh
RUN ["/bin/bash", "rm_cr.sh"]

ENTRYPOINT ["/bin/bash","/scripts/runme.sh"]