# AltinnStudio.Localtest.Aspire.Hosting.POC
Altinnstudio localtest aspire hosting extension
We have been using aspire for our altinn app for quite some time, just for logs etc. 
Its not a standard app, we have our own seperate frontend, but its delivered to the client the same way as original altinn apps in prod. In dev its running a vite dev server.
Both the app and frontend is started through aspire. However a painpoint was to always remember (specially for new devs) to clone and start the Localtest environment.


Since altinn studio released the localtest runtime as docker image, this is a proof of concept of using the altinn studio localtest as an aspire resource for a better dev experience.
https://github.com/Altinn/altinn-studio/pkgs/container/altinn-studio%2Fruntime-localtest
https://github.com/Altinn

```csharp
var altinn = builder.AddAltinnStudio();

var app = builder.AddProject<Projects.App>("altinnAppName", configure: static project =>
{
    project.LaunchProfileName = "AppRef";
    project.ExcludeKestrelEndpoints = true;
    project.ExcludeLaunchProfile = false;
})
    .WaitForStart(altinn.LocalTest)
    .WithHttpEndpoint(port: 5005, name: "localaltinn", isProxied: false)
    .WithUrlForEndpoint("localaltinn", (r =>
     {
         r.DisplayText = "LocalAltinnCloud";
         r.Url = "http://local.altinn.cloud/";
     }));
```

Later versions will be to setup telemtry for the img, and proper references on the app.