// Make sure that the project containing this code is a console program

open System
open Fancy
open Nancy
open Nancy.Hosting.Self

GET "/" (fun _ _ -> "Hello Nancy! </p><a href='fancy/42'>Fancy</a>")

GET "/fancy/{id}" (fun p http ->
    let id = p ?> "id"
    let str = sprintf "You sent id=%s at %A" id (DateTime.Now)
    http.Response
        .AsJson(str)
)

[<EntryPoint>]
let main argv =
    let uri = Uri("http://localhost:12345")
    printfn "Running on %A" uri
    let config = HostConfiguration()
    config.UrlReservations.CreateAutomatically <- true
    use host = new NancyHost(config, uri)
    host.Start()
    Diagnostics.Process.Start("http://localhost:12345/") |> ignore;
    printfn "Press [ENTER] to quit."
    Console.ReadLine() |> ignore
    0
