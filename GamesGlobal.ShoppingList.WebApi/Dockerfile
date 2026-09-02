#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

RUN apt-get update
RUN apt-get install -y curl

ENV ASPNETCORE_ENVIRONMENT=Production

WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["./GamesGlobal.ShoppingList.WebApi/GamesGlobal.ShoppingList.WebApi.csproj", "GamesGlobal.ShoppingList.WebApi/"]
COPY ["./GamesGlobal.ShoppingList.Application/GamesGlobal.ShoppingList.Application.csproj", "GamesGlobal.ShoppingList.Application/"]
COPY ["./GamesGlobal.ShoppingList.BusinessDomain/GamesGlobal.ShoppingList.BusinessDomain.csproj", "GamesGlobal.ShoppingList.BusinessDomain/"]
COPY ["./GamesGlobal.ShoppingList.Infrastructure/GamesGlobal.ShoppingList.Infrastructure.csproj", "GamesGlobal.ShoppingList.Infrastructure/"]
COPY ["./GamesGlobal.ShoppingList.Common/GamesGlobal.ShoppingList.Common.csproj", "GamesGlobal.ShoppingList.Common/"]
COPY ["./GamesGlobal.ShoppingList.xUnitTests/GamesGlobal.ShoppingList.xUnitTests.csproj", "GamesGlobal.ShoppingList.xUnitTests/"]

COPY . .

WORKDIR "/src/GamesGlobal.ShoppingList.WebApi"
RUN dotnet restore "GamesGlobal.ShoppingList.WebApi.csproj"

WORKDIR "/src/GamesGlobal.ShoppingList.xUnitTests"
RUN dotnet test "GamesGlobal.ShoppingList.xUnitTests.csproj"

WORKDIR "/src/GamesGlobal.ShoppingList.WebApi"
RUN dotnet build "GamesGlobal.ShoppingList.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GamesGlobal.ShoppingList.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GamesGlobal.ShoppingList.WebApi.dll"]