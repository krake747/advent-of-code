namespace Aoc.Lib

type AocInput = { Text: string; Lines: string list }

module Day01 =

    let instructions (lines: string list) (col: int) =
        lines
        |> List.map (fun l -> l.Split "   " |> Array.map int)
        |> List.sortBy (fun nums -> nums[col])
        |> List.map (fun nums -> nums[col])

    let part1 (input: AocInput) =
        let col = instructions input.Lines
        let left = col 0
        let right = col 1

        List.zip left right |> List.map (fun x -> abs (fst x - snd x)) |> List.sum

    let part2 (input: AocInput) =
        let col = instructions input.Lines
        let left = col 0
        let counts = col 1 |> List.countBy id |> Map.ofList

        let findCount id =
            Map.tryFind id counts |> Option.defaultValue 0

        left |> List.sumBy (fun id -> findCount id * id)
