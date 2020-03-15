#!/usr/bin/env bash

cd ../

dotnet restore

dotnet test -c Release

cd Rebus.SignalR

dotnet clean -o deploy

dotnet pack -c Release -o deploy

cd deploy

dotnet nuget push *.nupkg -s https://api.nuget.org/v3/index.json 
