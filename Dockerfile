FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copiar archivos de proyecto y restaurar dependencias
COPY src/PersonalNotes.Infrastructure/*.csproj ./PersonalNotes.Infrastructure/
COPY src/PersonalNotes.Api/*.csproj ./PersonalNotes.Api/
COPY src/PersonalNotes.Application/*.csproj ./PersonalNotes.Application/
COPY src/PersonalNotes.Domain/*.csproj ./PersonalNotes.Domain/

RUN dotnet restore PersonalNotes.Api/PersonalNotes.Api.csproj
COPY src/ ./

RUN dotnet publish PersonalNotes.Api/PersonalNotes.Api.csproj -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime-env
WORKDIR /app
COPY --from=build-env /app/out .

EXPOSE 8080

ENTRYPOINT ["dotnet", "PersonalNotes.Api.dll"]
