namespace Aoc.Lib

open System

[<AttributeUsage(AttributeTargets.Class, AllowMultiple = false)>]
type AocPuzzleAttribute(year: int, day: int, title: string) =
    inherit Attribute()
    member _.Year = year
    member _.Day = day
    member _.Title = title

[<Struct>]
type AocPuzzle = { Year: int; Day: int; Part: int }

type AocInput = { Text: string; Lines: string list }


[<AocPuzzle(2024, 1, "Historian Hysteria")>]
module Day01 =

    let instructions (lines: string list) (col: int) =
        let split (sep: string) (s: string) = s.Split sep

        lines
        |> List.map (split "   " >> Array.map int)
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


[<AocPuzzle(2024, 2, "Red-Nosed Reports")>]
module Day02 =

    let instructions (lines: string list) =
        let split (sep: char) (s: string) = s.Split sep
        lines |> List.map (split ' ' >> Array.map int >> List.ofArray)

    let monotonicIncreasing (pairs: (int * int) list) =
        pairs |> List.forall (fun (lhs, rhs) -> 1 <= rhs - lhs && rhs - lhs <= 3)

    let monotonicDecreasing (pairs: (int * int) list) =
        pairs |> List.forall (fun (lhs, rhs) -> 1 <= lhs - rhs && lhs - rhs <= 3)

    let monotonic (instructions: int list) =
        let lhs = instructions |> List.take (List.length instructions - 1)
        let rhs = instructions |> List.skip 1
        let pairs = List.zip lhs rhs

        monotonicIncreasing pairs || monotonicDecreasing pairs

    let problemDampener (instructions: int list) =
        List.init (List.length instructions) (fun i -> List.take i instructions @ List.skip (i + 1) instructions)

    let part1 (input: AocInput) =
        input.Lines |> instructions |> List.filter monotonic |> List.length

    let part2 (input: AocInput) =
        input.Lines
        |> instructions
        |> List.filter (problemDampener >> Seq.exists monotonic)
        |> List.length
