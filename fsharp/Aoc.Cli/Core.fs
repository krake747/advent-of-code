module Aoc.Cli.Core

open System
open System.IO
open System.Net.Http
open System.Text.Json

// ---------------------------
// Local session storage
// ---------------------------
type LocalConfig = { Session: string }

let private configPath =
    Path.Combine(Environment.GetFolderPath Environment.SpecialFolder.UserProfile, ".aoc", "config.json")

let private ensureLocalSession () =
    match File.Exists configPath with
    | true ->
        File.ReadAllText configPath
        |> JsonSerializer.Deserialize<LocalConfig>
        |> Option.ofObj
        |> Option.map _.Session
    | false -> None

let private saveLocalSession (session: string) =
    let cfg = { Session = session }
    let json = JsonSerializer.Serialize cfg
    File.WriteAllText(configPath, json)

let private promptSession () =
    printf "Enter your Advent of Code session: "
    let input = Console.ReadLine()

    if String.IsNullOrWhiteSpace input then
        failwith "Session cannot be empty."

    saveLocalSession input
    input

// ---------------------------
// Resolve session from multiple sources
// ---------------------------
let resolveSession (cliSession: string) =
    let dir = Path.GetDirectoryName configPath

    if not (Directory.Exists dir) then
        Directory.CreateDirectory dir |> ignore

    let candidates = [
        cliSession |> Option.ofObj
        Environment.GetEnvironmentVariable "AOC_SESSION" |> Option.ofObj
        ensureLocalSession ()
    ]

    candidates |> List.choose id |> List.tryHead |> Option.defaultWith promptSession

// ---------------------------
// HTTP client with session cookie
// ---------------------------
let httpClient (session: string) =
    let client = new HttpClient()
    client.DefaultRequestHeaders.Remove "Cookie" |> ignore
    client.DefaultRequestHeaders.Add("Cookie", $"session={session}")
    client
