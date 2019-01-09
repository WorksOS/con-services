FROM python:3.6
COPY ./build/requirements.txt /tmp
RUN pip install -r /tmp/requirements.txt

COPY ./src /app/src

WORKDIR /app

CMD ["gunicorn", "--bind", "0.0.0.0:80", "--timeout", "3600", "--workers", "5", "src.webapi.server:app"]