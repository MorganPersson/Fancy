Fancy
=======
Wrist friendly F# for Nancy.

Install
>Install-Package Fancy

Defining routes
=======
```f#
get "/" (fun _ -> "Hello world!")

get "/greet/{name}" (fun http name ->
  let t = sprintf "At %s: Hello %s" (DateTime.Now) name
  http.Response
      .AsJson t
)
```
the first parameter to the `get` function is a [Nancy route](https://github.com/NancyFx/Nancy/wiki/Defining-routes)
The second parameter is an anonymous function. The anonymous function should take at least one parameter where the  first parameter is always a INancyModule. The types of the parameters following the first will be inferred by the F# compiler. In the example above `name` will be inferred to a string.

Contribute
=======
We would love your help. Send us patches, report/fix bugs, request features etc.

Licence
=======
Fancy is licensed under [BSD 3](http://opensource.org/licenses/BSD-3-Clause)