module Aoc.Lib.Solver

open Aoc.Lib
open System.Diagnostics

type Solvers = Map<(int * int), AocSolver list>

let solve (solvers : Solvers) (puzzle : AocPuzzle) (input : AocInput) : (obj * int64) list option =
    solvers
    |> Map.tryFind (puzzle.Year, puzzle.Day)
    |> Option.map (fun solverList ->
        solverList
        |> List.map (fun solver ->
            let sw = Stopwatch.StartNew()
            let result = solver input
            sw.Stop()
            result, sw.ElapsedMilliseconds
        )
    )

let puzzleTitle (year : int) (day : int) : string option =
    typeof<AocPuzzleAttribute>.Assembly.GetTypes()
    |> Array.tryPick (fun t ->
        t.GetCustomAttributes(typeof<AocPuzzleAttribute>, false)
        |> Seq.cast<AocPuzzleAttribute>
        |> Seq.tryFind (fun attr -> attr.Year = year && attr.Day = day)
        |> Option.map (fun attr -> attr.Title)
    )
