module Aoc.Cli.Core

open Microsoft.Extensions.Configuration
open System
open System.IO
open System.Net.Http
open System.Text.Json

// ---------------------------
// Load appsettings.json
// ---------------------------
let appsettings =
    ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional = false, reloadOnChange = true)
        .Build()

// ---------------------------
// Local session storage
// ---------------------------
type LocalConfig = { Session: string }

let configPath =
    Path.Combine(Environment.GetFolderPath Environment.SpecialFolder.UserProfile, ".aoc", "config.json")

let ensureLocalSession () =
    let dir = Path.GetDirectoryName configPath

    if not (Directory.Exists dir) then
        Directory.CreateDirectory dir |> ignore

    if File.Exists configPath then
        let text = File.ReadAllText configPath
        let cfg = JsonSerializer.Deserialize<LocalConfig> text
        cfg.Session
    else
        null

let saveLocalSession (session: string) =
    let dir = Path.GetDirectoryName(configPath)

    if not (Directory.Exists dir) then
        Directory.CreateDirectory dir |> ignore

    let cfg = { Session = session }
    let json = JsonSerializer.Serialize cfg
    File.WriteAllText(configPath, json)


// ---------------------------
// Resolve session from multiple sources
// ---------------------------
let resolveSession (cliSession: string) =
    let promptSession () =
        printf "Enter your Advent of Code session: "
        let input = Console.ReadLine()

        if String.IsNullOrWhiteSpace input then
            failwith "Session cannot be empty."

        saveLocalSession input
        input

    let candidates = [
        cliSession
        Environment.GetEnvironmentVariable "AOC_SESSION"
        ensureLocalSession ()
        appsettings.["AdventOfCode:Session"]
    ]

    candidates
    |> List.tryFind (fun s -> not (String.IsNullOrWhiteSpace s))
    |> Option.defaultWith promptSession

// ---------------------------
// HTTP client with session cookie
// ---------------------------
let httpClient (session: string) =
    let client = new HttpClient()
    client.DefaultRequestHeaders.Remove "Cookie" |> ignore
    client.DefaultRequestHeaders.Add("Cookie", $"session={session}")
    client
