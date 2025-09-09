open Aoc.Cli
open Aoc.Lib
open Spectre.Console
open Spectre.Console.Cli
open System
open System.ComponentModel

// ---------------------------
// Solver
// ---------------------------
type Solver = AocInput -> obj

let solvers: Map<int * int * int, Solver> =
    Map [
        (2024, 1, 1), Day01.part1 >> box
        (2024, 1, 2), Day01.part2 >> box
        (2024, 2, 1), Day02.part1 >> box
        (2024, 2, 2), Day02.part2 >> box
    ]

// Lookup function
let solve (puzzle: AocPuzzle) (input: AocInput) : obj option =
    Map.tryFind (puzzle.Year, puzzle.Day, puzzle.Part) solvers
    |> Option.map (fun solver -> solver input)

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
[<Description("Fetch puzzle input from Advent of Code")>]
type FetchCommand() =
    inherit Command<AocSettings>()

    override _.Execute(_, settings) =
        let year, day = settings.Year, settings.Day

        if year = 0 || day = 0 then
            failwith "Please provide both year and day."

        let session = Core.resolveSession settings.Session
        let client = Core.httpClient session
        Runner.ensureInput client year day |> ignore
        printfn $"Input ready for Year %d{settings.Year} Day %d{day}"
        0

// ---------------------------
// Solve command
// ---------------------------
[<Description("Solve a puzzle for the given year and day")>]
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

        let table = Table()
        table.Border <- TableBorder.Rounded
        table.AddColumn(TableColumn("Part").RightAligned()) |> ignore
        table.AddColumn(TableColumn("Result").RightAligned()) |> ignore
        table.AddColumn(TableColumn("Status").Centered()) |> ignore

        [ 1; 2 ]
        |> List.iter (fun part ->
            match solve { Year = year; Day = day; Part = part } input with
            | Some result ->
                table.AddRow(Markup(part.ToString()), Markup $"%A{result}", Markup "[green]✓[/]")
                |> ignore
            | None ->
                table.AddRow(Markup(part.ToString()), Markup "-", Markup "[red]Not Implemented[/]")
                |> ignore
        )

        AnsiConsole.MarkupLine $"[bold orchid]Year {year} Day {day} Results[/]"
        AnsiConsole.Write table

        0


// ---------------------------
// Test File command
// ---------------------------
[<Description("Create or set test input for a puzzle")>]
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

        Runner.saveTestInput year day (readManualInput ())
        printfn $"Set test input for Year %d{year} Day %d{day}"
        0



// ---------------------------
// Program entry point
// ---------------------------
module Program =

    [<EntryPoint>]
    let main argv =
        let app = CommandApp()

        app.Configure(fun cfg ->
            cfg.SetApplicationName "aoc" |> ignore
            cfg.AddCommand<FetchCommand>("fetch").WithExample "fetch -y 2024 -d 1" |> ignore
            cfg.AddCommand<SolveCommand>("solve").WithExample "solve -y 2024 -d 1" |> ignore
            cfg.AddCommand<TestFileCommand> "test-file" |> ignore
        )

        app.Run argv
