#!/bin/bash

# Publishes Unity release to dist/Assemblies using only netstandard2.0 and netstandard2.1
#########################################################################################

dotnet restore

mkdir -p dist/Assemblies

dotnet msbuild /t:Unity -p:PublishDir=`pwd`/dist/Assemblies
