# Advent of Code in F\#

This repository provides a starting point for solving Advent of Code challenges in F# using a structured CLI approach.

## Project Structure

* `Aoc.Lib` – Contains shared library code and utility functions for solving puzzles.
* `Aoc.Cli` – The command-line interface project that runs solutions and manages input.
* `Aoc.Cli.Core` – Contains the shared configuration and HttpClient setup.

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

Install packages:

```bash
cd Aoc.Cli
dotnet add package Spectre.Console.Cli
```

## Usage

Run the CLI from the Aoc.Cli project:

### Fetch puzzle input

```bash
dotnet run fetch -y 2024 --d 1
```

This will download the puzzle input (if not already cached) into `.inputs/year2024day01.txt`.

### Solve puzzles

```bash
dotnet run solve --y 2024 --d 1
```

### Notes

* A shared HttpClient is used internally to fetch inputs efficiently.
* Puzzle inputs are cached locally under the .inputs folder.
* The CLI supports any year/day combination for which you have implemented solvers.
