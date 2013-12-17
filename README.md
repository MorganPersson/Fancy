Fancy
=======
Wrist friendly F# for Nancy.

Install
>Install-Package Fancy

Defining routes
=======
```f#
GET "/" (fun _ _ -> "Hello world!")

GET "/greet/{name}" (fun p http ->
  let name = p ?> "name"
  let t = name, DateTime.Now
  http.Response
      .AsJson t
)
```

Contribute
=======
We would love your help. Send us patches, report/fix bugs, request features etc.

Licence
=======
Fancy is licensed under [BSD 3](http://opensource.org/licenses/BSD-3-Clause)