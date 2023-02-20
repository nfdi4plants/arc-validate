FROM mcr.microsoft.com/dotnet/sdk:6.0

RUN apt-get update \
    && apt-get -y upgrade \
    && apt-get -y install python3 python3-pip python3-dev

RUN pip3 install anybadge --no-cache
RUN pip3 install junitparser --no-cache

COPY ./ /opt/arc-validate
WORKDIR /opt/arc-validate
RUN chmod +x create-badge.py

ENV PATH="${PATH}:/opt/arc-validate"

WORKDIR /arc