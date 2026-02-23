using NinjaOla.ASLocaltest.Aspire.Hosting.POC;


var builder = DistributedApplication.CreateBuilder(args);

var altinn = builder.AddAltinnStudio(c =>
{
    //c.Tag = ""//See readme.md for more information about this tag
    c.OutgoingPort = 8080; // the port used to access http://local.altinn.cloud/:{OutgoingPort}
    c.Lifetime = ContainerLifetime.Session;
});

builder.Build().Run();
