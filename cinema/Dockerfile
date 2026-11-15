FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY cinema/cinema.csproj cinema/
RUN dotnet restore cinema/cinema.csproj

COPY cinema/ cinema/
RUN dotnet publish cinema/cinema.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

USER $APP_UID
EXPOSE 8080 8081
ENTRYPOINT ["dotnet", "cinema.dll"]