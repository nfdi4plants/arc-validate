FROM mcr.microsoft.com/dotnet/sdk:10.0

COPY --from=ghcr.io/astral-sh/uv:latest /uv /uvx /bin/

COPY ./ /opt/arc-validate
WORKDIR /opt/arc-validate

RUN chmod +x build.sh
RUN ./build.sh runtests

ENV PATH="${PATH}:/opt/arc-validate"
ENV PATH="${PATH}:/opt/arc-validate/src/arc-validate/bin/Release/net10.0"

RUN apt update && apt install -y yq && apt install jq

WORKDIR /arc