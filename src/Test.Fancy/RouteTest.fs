module Test.Fancy.RouteTests
open System
open Fancy
open Nancy
open Nancy.Testing
open Xunit
open Swensen.Unquote

type RouteTestType1 = { x: string; y:string }

get "/" (fun _ -> "Hello from /")

GET "/RouteTests/{x}/{y}" (fun param http ->
    let param_x = param ?> "x"
    let param_y = param ?> "y"
    let result = { x = param_x; y = param_y}
    http.Response
        .AsJson result
    )

get "/StrongTypeTest/{x}/{y}" (fun http x y -> sprintf "%i - %s" x y)

get "/StrongTypeTest/{x}/{y}/{z}" (fun http x y z ->
    let s = (2 * x, y, 3. * z)
    http.Response.AsJson s
)

let browserGet (path:string) = 
    let browser = Browser(new DefaultNancyBootstrapper())
    let response = browser.Get path
    response.StatusCode, response.Body.AsString()

[<Fact>]
let ``should call route with strong typed params`` () =
    let statusCode, body = browserGet "/StrongTypeTest/99/Foo"
    test <@ statusCode = HttpStatusCode.OK @>
    test <@ body = "99 - Foo" @>

[<Fact>]
let ``should call route with even more strong typed params`` () =
    let statusCode, body = browserGet "/StrongTypeTest/2/Foo/3"
    test <@ statusCode = HttpStatusCode.OK @>
    test <@ body = "{\"item1\":4,\"item2\":\"Foo\",\"item3\":9}" @>

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
