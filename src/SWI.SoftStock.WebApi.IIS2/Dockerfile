#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["SWI.SoftStock.WebApi.IIS2/SWI.SoftStock.WebApi.IIS2.csproj", "SWI.SoftStock.WebApi.IIS2/"]
COPY ["SWI.SoftStock.WebApi/SWI.SoftStock.WebApi.csproj", "SWI.SoftStock.WebApi/"]
COPY ["SWI.SoftStock.ServerApps.WebApplicatinServices/SWI.SoftStock.ServerApps.WebApplicationServices.csproj", "SWI.SoftStock.ServerApps.WebApplicatinServices/"]
COPY ["SWI.SoftStock.ServerApps.WebApplicationContracts/SWI.SoftStock.ServerApps.WebApplicationContracts.csproj", "SWI.SoftStock.ServerApps.WebApplicationContracts/"]
COPY ["SWI.SoftStock.ServerApps.WebApplicationModel/SWI.SoftStock.ServerApps.WebApplicationModel.csproj", "SWI.SoftStock.ServerApps.WebApplicationModel/"]
COPY ["SWI.SoftStock.ServerApps.DataModel2/SWI.SoftStock.ServerApps.DataModel2.csproj", "SWI.SoftStock.ServerApps.DataModel2/"]
COPY ["SWI.SoftStock.ServerApps.DataAccess2/SWI.SoftStock.ServerApps.DataAccess2.csproj", "SWI.SoftStock.ServerApps.DataAccess2/"]
COPY ["SWI.SoftStock.ServerApps.DataAccess.Common2/SWI.SoftStock.ServerApps.DataAccess.Common2.csproj", "SWI.SoftStock.ServerApps.DataAccess.Common2/"]
RUN dotnet restore "SWI.SoftStock.WebApi.IIS2/SWI.SoftStock.WebApi.IIS2.csproj"
COPY . .
WORKDIR "/src/SWI.SoftStock.WebApi.IIS2"
RUN dotnet build "SWI.SoftStock.WebApi.IIS2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SWI.SoftStock.WebApi.IIS2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SWI.SoftStock.WebApi.IIS2.dll"]
