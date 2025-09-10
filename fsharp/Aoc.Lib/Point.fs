module Aoc.Lib.Point

[<Struct>]
type Point = {
    X : int
    Y : int
} with

    static member (+)(p1 : Point, p2 : Point) = { X = p1.X + p2.X ; Y = p1.Y + p2.Y }

    static member (*)(p1 : Point, p2 : Point) = {
        X = p1.X * p2.X - p1.Y * p2.Y
        Y = p1.X * p2.Y + p1.Y * p2.X
    }

    static member (*)(p : Point, factor : int) = { X = p.X * factor ; Y = p.Y * factor }

// Directions
let north = { X = 0 ; Y = -1 }
let northEast = { X = 1 ; Y = -1 }
let east = { X = 1 ; Y = 0 }
let southEast = { X = 1 ; Y = 1 }
let south = { X = 0 ; Y = 1 }
let southWest = { X = -1 ; Y = 1 }
let west = { X = -1 ; Y = 0 }
let northWest = { X = -1 ; Y = -1 }

let private rotationClockwise90 = { X = 0 ; Y = 1 }

// Functions
let rotateRight (p : Point) = p * rotationClockwise90
