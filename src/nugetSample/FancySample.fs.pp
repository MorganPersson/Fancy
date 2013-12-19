// Make sure that the project containing this code is a console program

open System
open Fancy
open Nancy
open Nancy.Hosting.Self

get "/" (fun _ -> "Hello Nancy! </p><a href='fancy/42'>Fancy</a>")

get "/fancy/{id}" (fun http id ->
    let str = sprintf "You sent id=%i at %A" id (DateTime.Now)
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
