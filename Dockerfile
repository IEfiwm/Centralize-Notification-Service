ARG APP_PROJECT=CNS.Api
ARG FINAL_IMAGE=reg.daricgold.com/dotnet/aspnet:10.0

FROM reg.daricgold.com/dotnet/sdk:10.0 AS build
ARG APP_PROJECT=CNS.Api
WORKDIR /src

COPY Directory.Build.props NuGet.Config ./
COPY src/ ./src/

RUN dotnet restore "src/${APP_PROJECT}/${APP_PROJECT}.csproj"
RUN dotnet publish "src/${APP_PROJECT}/${APP_PROJECT}.csproj" \
    -c Release \
    --no-restore \
    -o /app/publish \
    /p:UseAppHost=false

FROM ${FINAL_IMAGE} AS final
ARG APP_PROJECT=CNS.Api
WORKDIR /app

ENV APP_PROJECT=${APP_PROJECT}
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT exec dotnet "${APP_PROJECT}.dll"
