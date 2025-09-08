open Aoc.Cli
open Spectre.Console.Cli
open System.ComponentModel
open Aoc.Lib
open System

// ---------------------------
// Solver
// ---------------------------
let solve (year: int) (day: int) (part: int) (input: AocInput) =
    match year, day, part with
    | 2024, 1, 1 -> Day01.part1 input |> Some
    | 2024, 1, 2 -> Day01.part2 input |> Some
    | _ -> None

// ---------------------------
// Command settings
// ---------------------------
type AocSettings() =
    inherit CommandSettings()

    [<CommandOption("-y|--year")>]
    [<Description("Year of the puzzle")>]
    member val Year = 0 with get, set

    [<CommandOption("-d|--day")>]
    [<Description("Day of the puzzle")>]
    member val Day = 0 with get, set

    [<CommandOption("-t|--test")>]
    [<Description("Use test input instead of real puzzle input")>]
    member val UseTest = false with get, set

    [<CommandOption("-s|--session")>]
    [<Description("Override Advent of Code session token")>]
    member val Session: string = null with get, set

// ---------------------------
// Fetch command
// ---------------------------
type FetchCommand() =
    inherit Command<AocSettings>()

    override _.Execute(_, settings) =
        let year, day = settings.Year, settings.Day

        if year = 0 || day = 0 then
            failwith "Please provide both year and day."

        let session = Core.resolveSession settings.Session
        let client = Core.httpClient session
        Runner.ensureInput client year day |> ignore
        printfn "Input ready for Year %d Day %d" settings.Year day
        0

// ---------------------------
// Test File command
// ---------------------------
type TestFileCommand() =
    inherit Command<AocSettings>()

    let readManualInput () =
        printfn "Please paste your input, then press Enter twice to finish:"

        let rec loop acc =
            let line = Console.ReadLine()
            if String.IsNullOrWhiteSpace line then List.rev acc else loop (line :: acc)

        let lines = loop []
        String.Join("\n", lines)

    override _.Execute(_, settings) =
        let year, day = settings.Year, settings.Day

        if year = 0 || day = 0 then
            failwith "Please provide both year and day."

        Runner.saveTestInput year day (readManualInput ()) |> ignore
        printfn "Set test input for Year %d Day %d" year day
        0


// ---------------------------
// Solve command
// ---------------------------
type SolveCommand() =
    inherit Command<AocSettings>()

    override _.Execute(_, settings) =
        let year, day, test = settings.Year, settings.Day, settings.UseTest

        if year = 0 || day = 0 then
            failwith "Please provide both year and day."

        let session = Core.resolveSession settings.Session
        let client = Core.httpClient session

        let input =
            match test with
            | true -> Runner.ensureTest year day
            | _ -> Runner.ensureInput client year day

        [ 1; 2 ]
        |> List.iter (fun part ->
            match solve year day part input with
            | Some result -> printfn "Year %d Day %d Part %d: %A" year day part result
            | None -> printfn "Solver for Year %d Day %d Part %d not implemented yet" year day part)

        0

// ---------------------------
// Program entry point
// ---------------------------
module Program =

    [<EntryPoint>]
    let main argv =
        let app = CommandApp()

        app.Configure(fun cfg ->
            cfg.AddCommand<FetchCommand> "fetch" |> ignore
            cfg.AddCommand<SolveCommand> "solve" |> ignore
            cfg.AddCommand<TestFileCommand> "test-file" |> ignore)

        app.Run argv
