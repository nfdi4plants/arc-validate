FROM mcr.microsoft.com/dotnet/sdk:6.0
COPY ./ /opt/arc-validate
WORKDIR /opt/arc-validate
ENV PATH="${PATH}:/opt/arc-validate"
WORKDIR /arc