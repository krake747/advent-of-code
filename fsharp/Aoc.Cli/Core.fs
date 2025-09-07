namespace Aoc.Cli

open System.Net.Http
open Microsoft.Extensions.Configuration
open System.IO

module Core =
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

    let httpClient =
        let client = new HttpClient()
        client.DefaultRequestHeaders.Remove("Cookie") |> ignore
        client.DefaultRequestHeaders.Add("Cookie", $"session={session}")
        client
