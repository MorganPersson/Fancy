module Test.Fancy.RouteTests
open System
open Fancy
open Nancy
open Nancy.Testing
open Xunit
open Swensen.Unquote

type RouteTestType1 = { x: string; y:string }

GET "/" (fun _ _ -> "Hello from /")

GET "/RouteTests/{x}/{y}" (fun param http ->
    let param_x = param ?> "x"
    let param_y = param ?> "y"
    let result = { x = param_x; y = param_y}
    http.Response
        .AsJson result
    )

let browserGet path = 
    let browser = Browser(new DefaultNancyBootstrapper())
    let response = browser.Get path
    response.StatusCode, response.Body.AsString()

[<Fact>]
let ``should call route for /`` () =
    let statusCode, body = browserGet "/"
    test <@ statusCode = HttpStatusCode.OK @>
    test <@ body = "Hello from /" @>

[<Fact>]
let ``should get parameters in call`` () =
    let statusCode, body = browserGet "/RouteTests/foo/bar"
    test <@ statusCode = HttpStatusCode.OK @>
    test <@ body = "{\"x\":\"foo\",\"y\":\"bar\"}" @>
