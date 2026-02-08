FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app

ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
    DOTNET_NOLOGO=1

ARG EF_MIGRATIONS_PROJECT \
	MS_PROJECT_SOURCE_PATH

COPY Directory.Build.props Directory.Packages.props ./
COPY database-build/scripts/ef-migrations.sh ./
COPY "$MS_PROJECT_SOURCE_PATH" ./"$MS_PROJECT_SOURCE_PATH"
COPY sdk/ ./sdk

RUN chmod +x ef-migrations.sh

RUN groupadd -r docker && \
    useradd -r -g docker -m -d /home/docker docker && \
    chown -R docker:docker /app

USER docker

ENV PATH="${PATH}:/home/docker/.dotnet/tools"

RUN dotnet tool install -g dotnet-ef

RUN dotnet restore "$EF_MIGRATIONS_PROJECT" --nologo

RUN dotnet build "$EF_MIGRATIONS_PROJECT" \
    --no-restore \
    --nologo \
    -c Debug \
    /p:TreatWarningsAsErrors=false \
    /p:CodeAnalysisTreatWarningsAsErrors=false \
    /p:EnforceCodeStyleInBuild=false \
    /p:GenerateDocumentationFile=false \
    /p:NoWarn="CS1591"

WORKDIR /app