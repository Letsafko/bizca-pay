FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src

COPY ["microservices/user/src/Bizca.Users.Api/Bizca.Users.Api.csproj", "microservices/user/src/Bizca.Users.Api/"]
COPY ["microservices/user/src/Bizca.Users.Infrastructure/Bizca.Users.Infrastructure.csproj", "microservices/user/src/Bizca.Users.Infrastructure/"]
COPY ["microservices/user/src/Bizca.Users.Domain/Bizca.Users.Domain.csproj", "microservices/user/src/Bizca.Users.Domain/"]
COPY ["sdk/Bizca.Sdk.SharedKernel/Bizca.Sdk.SharedKernel.csproj", "sdk/Bizca.Sdk.SharedKernel/"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]

RUN pwd

RUN dotnet restore "microservices/user/src/Bizca.Users.Api/Bizca.Users.Api.csproj"
COPY . .
WORKDIR "/src/microservices/user/src/Bizca.Users.Api"
RUN dotnet build "./Bizca.Users.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Debug
RUN dotnet publish "./Bizca.Users.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bizca.Users.Api.dll"]
