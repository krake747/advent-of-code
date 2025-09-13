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

[<AocPuzzle(2024, 5, "Print Queue")>]
module Day05 =

    type Pages = string list
    type PageUpdates = string list list
    type PagePrecedenceRules = Collections.Generic.Comparer<string>

    type SafetyManual = {
        Updates : PageUpdates
        PrecedenceRules : PagePrecedenceRules
    }

    let split (sep : string) (s : string) : string list = s.Split sep |> Array.toList
    let splitByChar (sep : char) (s : string) : string list = s.Split sep |> Array.toList

    let splitLines = splitByChar '\n'
    let splitCommas = splitByChar ','

    let sleighLaunchSafetyManual (text : string) : SafetyManual =
        let parts = split "\n\n" text
        let ordering = parts[0] |> (splitLines >> Set.ofList)
        let updates = parts[1] |> (splitLines >> List.map splitCommas)

        let precedenceRules =
            PagePrecedenceRules.Create(fun p1 p2 -> if ordering.Contains $"%s{p1}|%s{p2}" then -1 else 1)

        { Updates = updates ; PrecedenceRules = precedenceRules }

    let elfPageSorting (precedenceRules : PagePrecedenceRules) (pages : Pages) : bool =
        let compare p1 p2 = precedenceRules.Compare(p1, p2)
        pages = List.sortWith compare pages

    let extractMiddlePage (pages : Pages) : int = int pages[List.length pages / 2]

    let part1 (input : AocInput) : int =
        input.Text
        |> sleighLaunchSafetyManual
        |> fun manual ->
            manual.Updates
            |> List.filter (elfPageSorting manual.PrecedenceRules)
            |> List.sumBy extractMiddlePage

    let part2 (input : AocInput) =
        input.Text
        |> sleighLaunchSafetyManual
        |> fun manual ->
            manual.Updates
            |> List.filter (fun pages -> not (elfPageSorting manual.PrecedenceRules pages))
            |> List.map (List.sortWith (fun p1 p2 -> manual.PrecedenceRules.Compare(p1, p2)))
            |> List.sumBy extractMiddlePage

[<AocPuzzle(2024, 6, "Guard Gallivant")>]
module Day06 =

    open Point
    open System.Collections.Generic

    type PatrolMap = Dictionary<Point, char>
    type Positions = HashSet<Point>
    type PatrolState = { Position : Point ; Direction : Point }
    type GuardRoute = { Positions : Positions ; Loop : bool }

    let parsePatrolMap (lines : string list) : PatrolMap =
        let dict = Dictionary<Point, char>()

        lines
        |> List.iteri (fun y line -> line |> Seq.iteri (fun x c -> dict.[{ X = x ; Y = y }] <- c))

        dict

    let locateGuardStart (map : PatrolMap) (c : char) : Point =
        map |> Seq.find (fun kvp -> kvp.Value = c) |> _.Key

    let trackGuardRoute (map : PatrolMap) (start : Point) : GuardRoute =
        let visited = HashSet<PatrolState>()
        let mutable patrol = { Position = start ; Direction = north }

        while map.ContainsKey patrol.Position && visited.Add(patrol) do
            let nextChar =
                match map.TryGetValue(patrol.Position + patrol.Direction) with
                | true, ch -> ch
                | false, _ -> ' '

            if nextChar = '#' then
                patrol <- { patrol with Direction = rotateRight patrol.Direction }
            else
                patrol <- {
                    patrol with
                        Position = patrol.Position + patrol.Direction
                }

        {
            Positions = visited |> Seq.map (fun s -> s.Position) |> HashSet
            Loop = visited.Contains patrol
        }

    let updateMap (map : PatrolMap) (obstacle : char) (position : Point) =
        let newMap = Dictionary map
        newMap[position] <- obstacle
        newMap

    let part1 (input : AocInput) : int =
        input.Lines
        |> parsePatrolMap
        |> (fun map -> map, locateGuardStart map '^')
        |> (fun (map, start) -> trackGuardRoute map start)
        |> _.Positions
        |> _.Count

    let part2 (input : AocInput) : int =
        let map = parsePatrolMap input.Lines
        let start = locateGuardStart map '^'
        let route = trackGuardRoute map start

        route
        |> _.Positions
        |> Seq.filter (fun p -> map[p] = '.')
        |> Seq.sumBy (fun obstacle ->
            let updatedMap = updateMap map '#' obstacle
            let route = trackGuardRoute updatedMap start
            route |> _.Loop |> (fun loop -> if loop then 1 else 0)
        )
