module Fancy
open System
open System.Collections.Generic
open Nancy
open Nancy.Routing

let (?>) (target : obj) targetKey =
    let t = target :?> DynamicDictionary
    t.[targetKey].ToString()
 
type FancyRoute = { HttpMethod:string; Path:string; Action:obj->INancyModule->obj } 

let private registeredRoutes = List<FancyRoute>()

type FancyModule () as this =
    let mutable after = AfterPipeline()
    let mutable before = BeforePipeline()
    let mutable onError = ErrorPipeline()
    let mutable context = null
    let mutable response = null
    let mutable modelBinderLocator = null
    let mutable modelValidationResult = null
    let mutable validatorLocator = null
    let mutable request = null
    let mutable viewFactory = null
    let mutable modulePath = null

    let routes = List<Route>(registeredRoutes.Count)

    let addRoute (route:FancyRoute) nancyMod =
        let ff:obj->obj = fun x -> route.Action x nancyMod
        let route = Route.FromSync(route.HttpMethod, route.Path, null, fun x -> ff x |> box)
        routes.Add route

    do
        let n = (this :> INancyModule)
        registeredRoutes |> Seq.iter(fun r -> addRoute r n)

    interface INancyModule with
        member t.After with get () = after and set(value) = after <- value
        member t.Before with get () = before and set(value) = before <- value
        member t.OnError with get () = onError and set(value) = onError <- value
        member t.Context with get () = context and set(value) = context <- value
        member t.Response with get () = response and set(value) = response <- value
        member t.ModelBinderLocator with get () = modelBinderLocator and set(value) = modelBinderLocator <- value
        member t.ModelValidationResult with get () = modelValidationResult and set(value) = modelValidationResult <- value
        member t.ValidatorLocator with get () = validatorLocator and set(value) = validatorLocator <- value
        member t.Request with get () = request and set(value) = request <- value
        member t.ViewFactory with get () = viewFactory and set(value) = viewFactory <- value
        member t.ModulePath with get () = modulePath 
        member t.Routes with get () = routes :> IEnumerable<Route>


let private createRoute httpMethod p f =
    let ff = fun a b -> (f a b) :> obj
    let r = { HttpMethod = httpMethod; Path = p; Action = ff }
    registeredRoutes.Add r

let private getMethod f =
    let typ = f.GetType()
    let all = typ.GetMethods()
    let theMethod = all |> Array.find(fun m ->
        let p = m.GetParameters() 
        p.Length > 0 && p.[0].ParameterType = typedefof<INancyModule>)
    theMethod

let private getParameters (theMethod:Reflection.MethodInfo) =
    let parameters = 
        theMethod.GetParameters()
        |> Seq.skip 1
        |> Seq.map(fun p -> p.Name, p.ParameterType)
        |> List.ofSeq
    parameters

let private createStronglyTypedRoute httpMethod path (f:INancyModule->'T) =
    let theMethod = getMethod f
    let parameters = getParameters theMethod
    let ff = fun p (h:INancyModule) -> 
        let paramValues = parameters |> List.map(fun pp -> p ?> (fst pp), snd pp)
        let paramVals = 
            (h :> obj) 
            :: (paramValues |> List.map(fun pp -> Convert.ChangeType(fst pp, snd pp)))
            |> Array.ofList
        theMethod.Invoke(f, paramVals)
    createRoute httpMethod path ff

[<Obsolete("use the strongly typed get'")>]
let GET path f = createRoute "GET" path f
[<Obsolete("use the strongly typed post'")>]
let POST path f = createRoute "POST" path f
[<Obsolete("use the strongly typed put'")>]
let PUT path f = createRoute "PUT" path f
[<Obsolete("use the strongly typed delete'")>]
let DELETE path f = createRoute "DELETE" path f
[<Obsolete("use the strongly typed patch'")>]
let PATCH path f = createRoute "PATCH" path f
[<Obsolete("use the strongly typed options'")>]
let OPTIONS path f = createRoute "OPTIONS" path f

let get path f = createStronglyTypedRoute "GET" path f
let post path f = createStronglyTypedRoute "POST" path f
let put path f = createStronglyTypedRoute "PUT" path f
let delete path f = createStronglyTypedRoute "DELETE" path f
let patch path f = createStronglyTypedRoute "PATCH" path f
let options path f = createStronglyTypedRoute "OPTIONS" path f
