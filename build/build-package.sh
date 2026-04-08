#!/bin/bash
set -e

if [ "$#" -ne 1 ]; then
  echo "Usage: build/build-package.sh <version>"
  exit 1
fi

SOLUTION_PATH="Swallow.NodeForMSBuild.slnx"
PACKAGE_PATH="packages"

echo "--> Running tests before packing packages..."
dotnet restore $SOLUTION_PATH
dotnet test --no-restore --verbosity quiet --disable-logo --solution $SOLUTION_PATH

echo "--> Tests passed; building packages..."
dotnet build --no-restore --no-incremental --configuration Release -p:AssemblyVersion="$1" $SOLUTION_PATH

for PROJECT in $(find "src/" -name '*.csproj' -d 2)
do
  dotnet pack --no-restore --no-build --configuration Release -p:Version="$1" --output $PACKAGE_PATH $PROJECT
  echo "--> Built package $PACKAGE_PATH/$PROJECT.$1.nupkg"
done
