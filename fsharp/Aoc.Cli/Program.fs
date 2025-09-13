open Aoc.Cli
open Aoc.Lib
open Spectre.Console
open Spectre.Console.Cli
open System
open System.ComponentModel
open System.Diagnostics

// ---------------------------
// Solver
// ---------------------------
type Solver = AocInput -> obj

let solvers : Map<int * int, Solver list> =
    Map [
        (2024, 1), [ Day01.part1 >> box ; Day01.part2 >> box ]
        (2024, 2), [ Day02.part1 >> box ; Day02.part2 >> box ]
        (2024, 3), [ Day03.part1 >> box ; Day03.part2 >> box ]
        (2024, 4), [ Day04.part1 >> box ; Day04.part2 >> box ]
        (2024, 5), [ Day05.part1 >> box ; Day05.part2 >> box ]
        (2024, 6), [ Day06.part1 >> box ; Day06.part2 >> box ]
    ]

let solve (puzzle : AocPuzzle) (input : AocInput) : (obj * int64) list option =
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
    member val Session : string = null with get, set

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

        let title =
            match puzzleTitle year day with
            | Some t -> $"{t} - Year {year} Day {day}"
            | None -> $"Year {year} Day {day}"

        let table = Table()
        table.Border <- TableBorder.Rounded
        table.Title <- TableTitle $"[bold orchid]{title}[/]"
        table.AddColumn(TableColumn("Part").RightAligned()) |> ignore
        table.AddColumn(TableColumn("Result").RightAligned()) |> ignore
        table.AddColumn(TableColumn("Status").Centered()) |> ignore
        table.AddColumn(TableColumn("Time (ms)").RightAligned()) |> ignore


        match solve { Year = year ; Day = day } input with
        | Some results ->

            results
            |> List.iteri (fun i (result, elapsed) ->
                table.AddRow(
                    Markup((i + 1).ToString()),
                    Markup $"%A{result}",
                    Markup "[green]✓[/]",
                    Markup $"{elapsed}"
                )
                |> ignore
            )
        | None -> table.AddRow(Markup "-", Markup "-", Markup "[red]Not Implemented[/]") |> ignore


        AnsiConsole.Write table
        0


// ---------------------------
// Test File command
// ---------------------------
[<Description("Create or set test input for a puzzle")>]
type FetchTestCommand() =
    inherit Command<AocSettings>()

    let readManualInput () =
        printfn "Please paste your input, then press Enter twice to finish:"

        let rec loop acc emptyCount =
            let line = Console.ReadLine()

            match line with
            | null -> List.rev acc
            | l when String.IsNullOrWhiteSpace l ->
                if emptyCount + 1 >= 2 then List.rev acc else loop ("" :: acc) (emptyCount + 1)
            | l -> loop (l :: acc) 0

        let lines = loop [] 0
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
            cfg.AddCommand<FetchTestCommand> "fetch-test" |> ignore
        )

        app.Run argv
