
open System
open Akka.Actor
open Akka.FSharp
open Proj1.MyActors

let mutable N = 0L
let mutable k = 0L
let workLoad = 100L

[<EntryPoint>]
let main argv =
    N <- argv.[0] |> Convert.ToInt64
    k <- argv.[1] |> Convert.ToInt64
    //let stopWatch = System.Diagnostics.Stopwatch.StartNew()

    let actorSystem = System.create "actorSystem" <| Configuration.defaultConfig()
    let bossActor = spawn actorSystem "boss" (boss N k workLoad)
    bossActor <! Start
    
    actorSystem.WhenTerminated.Wait()    
    //stopWatch.Stop()
    //printfn "%f" stopWatch.Elapsed.TotalMilliseconds
    0
    