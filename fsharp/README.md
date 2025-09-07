# Advent of Code in F\#

This repository provides a starting point for solving Advent of Code challenges in F# using a structured CLI approach.

## Project Structure

* `Aoc.Lib` – Contains shared library code and utility functions for solving puzzles.
* `Aoc.Cli` – The command-line interface project that runs solutions and manages input.

## Setup

Create the solution and projects:

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

## Dependencies

* `Spectre.Console.Cli` – Used to execute CLI commands.
* `Microsoft.Extensions.Configuration.Json` – Used to load the Advent of Code session configuration.

Install packages:

```bash
cd Aoc.Cli
dotnet add package Spectre.Console.Cli
dotnet add package Microsoft.Extensions.Configuration.Json
```

## Usage

1. Configure your Advent of Code session in a `appsettings.json` file.
2. Run the CLI commands via `dotnet run` from `Aoc.Cli`.
