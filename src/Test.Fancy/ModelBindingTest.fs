module Test.Fancy.ModelBindingTest
open System
open Fancy
open Nancy
open Nancy.ModelBinding
open Nancy.Testing
open Nancy.Testing.Fakes
open Xunit
open Swensen.Unquote
open Newtonsoft
open Newtonsoft.Json

[<CLIMutable>]
type MyFancyModel = { FirstName:string; LastName:string; Age:int }

post "/ModelBinding/ShouldBind" (fun http (model:MyFancyModel) ->
    //let model = http.Bind<MyFancyModel>()
    sprintf "%A" model
)
post "/ModelBinding/ShouldNotBind" (fun http str -> if str = "" then "empty" else "null")

let browserPost (path:string) obj = 
    let b = new DefaultNancyBootstrapper()
    let browser = Browser(b)
    let json = JsonConvert.SerializeObject obj
    let bc (ctx:BrowserContext) =
        ctx.Header("Content-Type", "application/json")
        ctx.Body(json)
        ()
    let response = browser.Post(path, fun c -> bc c)
    response.StatusCode, response.Body.AsString()

[<Fact>]
let ``should bind to model`` () =
    use ctx = new NancyContext()
    let obj =  { FirstName="Foo"; LastName="Bar"; Age=42 } 
    let statusCode, body = browserPost "/ModelBinding/ShouldBind" obj
    let expected = "{FirstName = \"Foo\";\n LastName = \"Bar\";\n Age = 42;}"
    test <@ statusCode = HttpStatusCode.OK @>
    test <@ body = expected @>


[<Fact>]
let ``should not bind to any model if no route value matched`` () =
    use ctx = new NancyContext()
    let obj =  { FirstName="Foo"; LastName="Bar"; Age=42 } 
    let statusCode, body = browserPost "/ModelBinding/ShouldNotBind" obj
    test <@ statusCode = HttpStatusCode.OK @>
    test <@ body = "null" @>

