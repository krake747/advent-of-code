namespace Aoc.Lib

open System
open System.Text.RegularExpressions

[<AttributeUsage(AttributeTargets.Class, AllowMultiple = false)>]
type AocPuzzleAttribute(year : int, day : int, title : string) =
    inherit Attribute()
    member _.Year = year
    member _.Day = day
    member _.Title = title

[<Struct>]
type AocPuzzle = { Year : int ; Day : int }

type AocInput = { Text : string ; Lines : string list }


[<AocPuzzle(2024, 1, "Historian Hysteria")>]
module Day01 =

    let instructions (lines : string list) (col : int) : int list =
        let split (sep : string) (s : string) = s.Split sep

        lines
        |> List.map (split "   " >> Array.map int)
        |> List.sortBy (fun nums -> nums[col])
        |> List.map (fun nums -> nums[col])

    let part1 (input : AocInput) : int =
        let col = instructions input.Lines
        let left = col 0
        let right = col 1

        List.zip left right |> List.map (fun x -> abs (fst x - snd x)) |> List.sum

    let part2 (input : AocInput) : int =
        let col = instructions input.Lines
        let left = col 0
        let counts = col 1 |> List.countBy id |> Map.ofList

        let findCount id =
            Map.tryFind id counts |> Option.defaultValue 0

        left |> List.sumBy (fun id -> findCount id * id)


[<AocPuzzle(2024, 2, "Red-Nosed Reports")>]
module Day02 =

    let instructions (lines : string list) : int list list =
        let split (sep : char) (s : string) = s.Split sep
        lines |> List.map (split ' ' >> Array.map int >> List.ofArray)

    let monotonicIncreasing (pairs : (int * int) list) : bool =
        pairs |> List.forall (fun (lhs, rhs) -> 1 <= rhs - lhs && rhs - lhs <= 3)

    let monotonicDecreasing (pairs : (int * int) list) : bool =
        pairs |> List.forall (fun (lhs, rhs) -> 1 <= lhs - rhs && lhs - rhs <= 3)

    let monotonic (instructions : int list) : bool =
        let lhs = instructions |> List.take (List.length instructions - 1)
        let rhs = instructions |> List.skip 1
        let pairs = List.zip lhs rhs

        monotonicIncreasing pairs || monotonicDecreasing pairs

    let problemDampener (instructions : int list) : int list list =
        List.init (List.length instructions) (fun i -> List.take i instructions @ List.skip (i + 1) instructions)

    let part1 (input : AocInput) : int =
        input.Lines |> instructions |> List.filter monotonic |> List.length

    let part2 (input : AocInput) : int =
        input.Lines
        |> instructions
        |> List.filter (problemDampener >> Seq.exists monotonic)
        |> List.length

[<AocPuzzle(2024, 3, "Mull It Over")>]
module Day03 =

    [<Struct>]
    type State = { Enabled : bool ; Total : int64 }

    let instructions (m : Match) : int64 =
        int64 (m.Groups[1].Value) * int64 (m.Groups[2].Value)

    let conditionalInstructions (state : State) (m : Match) =
        match m.Value with
        | "do()" -> { state with Enabled = true }
        | "don't()" -> { state with Enabled = false }
        | _ when state.Enabled -> { state with Total = state.Total + instructions m }
        | _ -> state

    let part1 (input : AocInput) : int64 =
        input.Text |> Regex(@"mul\((\d+),(\d+)\)").Matches |> Seq.sumBy instructions

    let part2 (input : AocInput) : int64 =
        input.Text
        |> Regex(@"do\(\)|don't\(\)|mul\((\d+),(\d+)\)").Matches
        |> Seq.fold conditionalInstructions { Enabled = true ; Total = 0 }
        |> _.Total

[<AocPuzzle(2024, 4, "Ceres Search")>]
module Day04 =

    open Point

    type Map = Map<Point, char>

    let parseMap (lines : string list) : Map =
        lines
        |> List.indexed
        |> List.collect (fun (y, line) -> [ for x, c in line |> Seq.toList |> List.indexed -> { X = x ; Y = y }, c ])
        |> Map.ofList

    let searchWord (map : Map) (pattern : string) (p : Point) (dir : Point) : bool =
        let chars = [
            for i in 0 .. pattern.Length - 1 -> map.TryFind(p + dir * i) |> Option.defaultValue ' '
        ]

        chars = Seq.toList pattern || chars = Seq.toList (Seq.rev pattern)

    let searchXmas (map : Map) : bool list =
        let directions = [ east ; southEast ; south ; southWest ]
        let searchMapForXmas = searchWord map "XMAS"

        map
        |> (Map.keys >> Seq.toList >> List.allPairs directions)
        |> List.map (fun (dir, pos) -> searchMapForXmas pos dir)
        |> List.filter id

    let searchXmasShape (map : Map) : Point list =
        let searchMapForXmasShape = searchWord map "MAS"

        let isXmasShape p =
            searchMapForXmasShape (p + northWest) southEast
            && searchMapForXmasShape (p + southWest) northEast

        map |> (Map.keys >> Seq.toList) |> List.filter isXmasShape

    let part1 (input : AocInput) : int =
        input.Lines |> parseMap |> searchXmas |> List.length

    let part2 (input : AocInput) : int =
        input.Lines |> parseMap |> searchXmasShape |> List.length
