# 1. szakasz: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Másoljuk a solution fájlt
COPY *.sln .

# MÁSOLD ÁT MINDKÉT PROJEKTFÁJLT (Ez hiányzott!)
COPY ERP/*.csproj ./ERP/
COPY ERP.Tests/*.csproj ./ERP.Tests/

# Most már lefut a restore, mert megtalálja a tesztprojektet is
RUN dotnet restore

# Másoljuk az összes többi fájlt
COPY . .

# Itt már csak a fő appot építjük
WORKDIR /app/ERP
RUN dotnet publish -c Release -o /out

# 2. szakasz: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .

ENTRYPOINT ["dotnet", "ERP.dll"]