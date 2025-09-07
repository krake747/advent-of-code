namespace Aoc.Cli

open Aoc.Lib
open System.IO
open System.Net.Http
open Microsoft.Extensions.Configuration

module Runner =

    let private inputPath (year: int) (day: int) =
        Path.Combine(".inputs", $"year{year}day{day:D2}.txt")

    let private fetchInput (client: HttpClient) (year: int) (day: int) =
        let url = $"https://adventofcode.com/{year}/day/{day}/input"
        let result = client.GetStringAsync url |> Async.AwaitTask |> Async.RunSynchronously
        result

    let private saveInput (path: string) (text: string) = File.WriteAllText(path, text)

    let private readInput (path: string) =
        let text = File.ReadAllText path
        let lines = File.ReadAllLines path |> List.ofArray
        { Text = text; Lines = lines }

    let private getInput (client: HttpClient) (path: string) (year: int) (day: int) =
        if File.Exists path then
            readInput path
        else
            let inputText = fetchInput client year day
            saveInput path inputText
            readInput path

    let ensureInput (year: int) (day: int) : AocInput =
        Directory.CreateDirectory ".inputs" |> ignore

        let config =
            ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional = false, reloadOnChange = true)
                .Build()

        let session =
            match config["AdventOfCode:Session"] with
            | null
            | "" -> failwith "Session key missing in appsettings.json"
            | s -> s

        let path = inputPath year day

        if File.Exists path then
            readInput path
        else
            use client = new HttpClient()
            client.DefaultRequestHeaders.Add("Cookie", $"session={session}")
            getInput client path year day

