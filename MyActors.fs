namespace Proj1

open System
open Akka
open Akka.Actor
open Akka.FSharp

module MyActors =

    type BossMsg =
        | Start
        | Report of List<(int64*int64)>
    
    type WorkerMsg = 
        | Assign of interval: (int64*int64) * k:int64

    let SS (n:int64) =
        n * (n + 1L) * (2L * n + 1L) / 6L

    let sumOfSquare (start:int64) (k:int64) = 
        let n = start + k - 1L
        let s = start - 1L
        (SS n) - (SS s)

    let check (num:int64) (k:int64)=
        let ss = sumOfSquare num k
        let sqrt = ss |> double |> sqrt |> int64
        
        if sqrt*sqrt = ss then (num,sqrt) else (0L,0L)
        // if sqrt*sqrt = ss then                   
        //     printfn "found: %A^2 + ... + %A^2 = %A^2" num (num+k-1L) sqrt
        //     num
        // else 
        //     0L

    let worker (mailbox: Actor<WorkerMsg>) =
        let rec loop () = actor{
            let! message = mailbox.Receive()
            let boss = mailbox.Sender()
            match message with 
            | Assign (interval, k) ->                 
                let (start, last) = interval                
                let report = 
                    [start .. last] 
                    |> List.map(fun num -> check num k) 
                    |> Report                 
                mailbox.Sender() <! report
                return! loop()            
        }
        loop()
          
    let boss (N:int64) (k:int64) (workload:int64) (mailbox: Actor<BossMsg>) =
        let tasks = [|1L .. workload .. N|]
        let t = tasks.Length |> int64
        let workers = [| for i in 1L .. t do yield (spawn mailbox.Context.System ("worker" + (string (i+1L))) worker) |]
        let mutable unfinished = tasks.Length                
        let rec loop() = actor {
            let! message = mailbox.Receive()            
            match message with 
            | Start-> 
                tasks |> Array.iteri (fun i x ->
                    //let workerName = "worker" + (string (i+1))
                    //let myWorker = spawn mailbox.Context.System workerName worker
                    if (x+workload) < N then
                        workers.[i] <! Assign((x, x+workload-(int64 1)), k)
                    else
                        workers.[i] <! Assign((x, N), k)                            
                )

            | Report (res) ->
                res
                |> List.iter(fun (num,sqrt) -> 
                    if num > 0L then 
                        printfn "%s^2 + ... + %s^2 = %s^2" (num.ToString()) ((num+k-1L).ToString()) (sqrt.ToString())
                    )         
                unfinished <- unfinished - 1
                if unfinished = 0 then
                    mailbox.Context.System.Terminate() |> ignore
            
            return! loop()
        }
        loop()