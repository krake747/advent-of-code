open Aoc.Cli
open Spectre.Console.Cli
open System.ComponentModel
open Aoc.Lib

let solve (year: int) (day: int) (part: int) (input: AocInput) =
    match year, day, part with
    | 2024, 1, 1 -> Day01.part1 input
    | 2024, 1, 2 -> Day01.part2 input
    | _ -> failwith "Day/part not implemented"

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

// ---------------------------
// Fetch command
// ---------------------------
type FetchCommand() =
    inherit Command<AocSettings>()

    override _.Execute(_, settings) =
        let year, day = settings.Year, settings.Day

        if year = 0 || day = 0 then
            printfn "Please provide both year and day."
            1
        else
            let _ = Runner.ensureInput year day
            printfn "Input ready for Year %d Day %d" settings.Year day
            0

// ---------------------------
// Solve command
// ---------------------------
type SolveCommand() =
    inherit Command<AocSettings>()

    override _.Execute(_, settings) =
        let year, day = settings.Year, settings.Day

        if year = 0 || day = 0 then
            printfn "Please provide both year and day."
            1
        else
            let input = Runner.ensureInput year day

            [ 1; 2 ]
            |> List.iter (fun part ->
                try
                    let result = solve year day part input
                    printfn "Year %d Day %d Part %d: %A" year day part result
                with _ ->
                    printfn "Solver for Year %d Day %d Part %d not implemented yet" year day part)

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
            cfg.AddCommand<SolveCommand> "solve" |> ignore)

        app.Run argv
