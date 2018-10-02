FROM microsoft/dotnet:2.1-runtime
WORKDIR /app

COPY out .
ENTRYPOINT ["dotnet", "NsbInterfaceTest.Producer.dll"]