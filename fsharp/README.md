# Advent of Code in F\#

Create the main entry point:

```bash
cd fsharp
dotnet new sln -n Aoc
dotnet new classlib -lang 'F#' -o Aoc.Lib
dotnet new console -lang 'F#' -o Aoc.Cli
dotnet sln add Aoc.Lib
dotnet sln add Aoc.Cli
cd Aoc.Cli
dotnet add reference ../Aoc.Lib/Aoc.Lib.fsproj
```
