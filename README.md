Fancy
=======
Wrist friendly F# for Nancy.

Install
>Install-Package Fancy

Defining routes
====
there are 6 methods defining the http methods (or verbs) `get`, `post`, `put`, `delete`, `patch` and `options`
They all share the same definition, `string->f:(INancyModule->'a)->unit`

the first parameter is a string, it's a [Nancy route](https://github.com/NancyFx/Nancy/wiki/Defining-routes).
the second parameter is a function which take a parameter of type INancyModule and a generic parameter `'a`.
And finally it returns unit.

Examples
===
```f#
get "/" (fun _ -> "Hello world!")

get "/greet/{name}" (fun http name ->
  let t = sprintf "At %s: Hello %s" (DateTime.Now) name
  http.Response
      .AsJson t
)

get "/many/{x}/{y}/{z}" (fun (x:string) (y:int) (z:string) -> () )
```

Contribute
=======
We would love your help. Send us patches, report/fix bugs, request features etc.

Licence
=======
Fancy is licensed under [BSD 3](http://opensource.org/licenses/BSD-3-Clause)