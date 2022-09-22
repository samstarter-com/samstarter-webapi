#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0.8-bullseye-slim-amd64 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0.400-1-bullseye-slim-amd64 AS build
WORKDIR /src
COPY ["src/SWI.SoftStock.WebApi.IIS2/SWI.SoftStock.WebApi.IIS2.csproj", "SWI.SoftStock.WebApi.IIS2/"]
COPY ["src/SWI.SoftStock.WebApi/SWI.SoftStock.WebApi.csproj", "SWI.SoftStock.WebApi/"]
COPY ["src/SWI.SoftStock.ServerApps.MailSender/SWI.SoftStock.ServerApps.MailSender.csproj", "SWI.SoftStock.ServerApps.MailSender/"]
COPY ["src/SWI.SoftStock.Common.Dto2/SWI.SoftStock.Common.Dto2.csproj", "SWI.SoftStock.Common.Dto2/"]
COPY ["src/SWI.SoftStock.ServerApps.WebApplicatinServices/SWI.SoftStock.ServerApps.WebApplicationServices.csproj", "SWI.SoftStock.ServerApps.WebApplicatinServices/"]
COPY ["src/SWI.SoftStock.ServerApps.WebApplicationContracts/SWI.SoftStock.ServerApps.WebApplicationContracts.csproj", "SWI.SoftStock.ServerApps.WebApplicationContracts/"]
COPY ["src/SWI.SoftStock.ServerApps.WebApplicationModel/SWI.SoftStock.ServerApps.WebApplicationModel.csproj", "SWI.SoftStock.ServerApps.WebApplicationModel/"]
COPY ["src/SWI.SoftStock.ServerApps.DataModel2/SWI.SoftStock.ServerApps.DataModel2.csproj", "SWI.SoftStock.ServerApps.DataModel2/"]
COPY ["src/SWI.SoftStock.ServerApps.DataAccess2/SWI.SoftStock.ServerApps.DataAccess2.csproj", "SWI.SoftStock.ServerApps.DataAccess2/"]
COPY ["src/SWI.SoftStock.ServerApps.DataAccess.Common2/SWI.SoftStock.ServerApps.DataAccess.Common2.csproj", "SWI.SoftStock.ServerApps.DataAccess.Common2/"]
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
