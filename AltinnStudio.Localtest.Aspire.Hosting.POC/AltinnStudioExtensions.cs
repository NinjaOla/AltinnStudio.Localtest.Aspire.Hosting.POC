using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;

namespace AltinnStudio.Localtest.Aspire.Hosting.POC;

/// <summary>
/// Extensions for adding Altinn Studio resources to an Aspire distributed application.
/// </summary>
public static class AltinnStudioExtensions
{
    /// <summary>
    /// Adds Altinn Studio resources to the distributed application, optionally including a LocalTest container.
    /// </summary>
    /// <param name="builder">The distributed application builder.</param>
    /// <param name="configure">Optional callback to override <see cref="AltinnStudioOptions"/> defaults.</param>
    /// <returns>An <see cref="AltinnStudioResources"/> containing references to the added resources.</returns>
    public static AltinnStudioResources AddAltinnStudio(
        this IDistributedApplicationBuilder builder,
        Action<AltinnStudioOptions>? configure = null)
    {
        var options = new AltinnStudioOptions();
        configure?.Invoke(options);

        IResourceBuilder<ContainerResource>? localTestContainer = null;

        var containerRuntime = builder.Configuration["DOTNET_ASPIRE_CONTAINER_RUNTIME"]
            ?? (File.Exists("/run/.containerenv") ? "podman" : null);
        var isPodman = string.Equals(containerRuntime, "podman", StringComparison.OrdinalIgnoreCase);

        var testDomain = builder.Configuration["TEST_DOMAIN"] ?? options.TestDomain;
        var runtimeDefaultPort = isPodman ? 8000 : options.DefaultPort;
        var outgoingPort = options.OutgoingPort
            ?? (int.TryParse(builder.Configuration["ALTINN3LOCAL_PORT"], out var p) ? p : runtimeDefaultPort);
        var dotnetEnv = isPodman ? "Podman" : "Docker";

        localTestContainer = builder
            .AddContainer("localtest", "altinn/altinn-studio/runtime-localtest", options.Tag)
            .WithImageRegistry(options.ImageRegistry)
            .WithLifetime(options.Lifetime)
            .WithHttpEndpoint(port: outgoingPort, targetPort: 5101, name: "http", isProxied: false)
            .WithEndpoint("internal", e =>
            {
                e.Port = 5101;
                e.TargetPort = 5101;
                e.Transport = "tcp";
                e.IsProxied = false;
            })
            .WithEnvironment("DOTNET_ENVIRONMENT", dotnetEnv)
            .WithEnvironment("GeneralSettings__BaseUrl", $"http://{testDomain}:{outgoingPort.ToString()}")
            .WithEnvironment("GeneralSettings__HostName", testDomain);


        return new AltinnStudioResources(builder, localTestContainer);
    }
}

/// <summary>
/// Configuration options for the Altinn Studio LocalTest container.
/// </summary>
public sealed record AltinnStudioOptions
{
    /// <summary>Image tag for the LocalTest container.</summary>
    public string Tag { get; init; } = "c562f53";

    /// <summary>
    /// Host port exposed for the LocalTest container.
    /// When <see langword="null"/>, auto-detected from <c>ALTINN3LOCAL_PORT</c> configuration
    /// or the container runtime default.
    /// </summary>
    public int? OutgoingPort { get; init; }

    /// <summary>Default host port used when no explicit port is configured (Docker runtime).</summary>
    public int DefaultPort { get; init; } = 80;

    /// <summary>Container lifecycle. Defaults to <see cref="ContainerLifetime.Persistent"/>.</summary>
    public ContainerLifetime Lifetime { get; init; } = ContainerLifetime.Persistent;

    /// <summary>Container image registry.</summary>
    public string ImageRegistry { get; init; } = "ghcr.io";

    /// <summary>Test domain used for URL and environment configuration.</summary>
    public string TestDomain { get; init; } = "local.altinn.cloud";
}

/// <summary>
/// Holds references to the Altinn Studio resources added by <see cref="AltinnStudioExtensions.AddAltinnStudio"/>.
/// </summary>
/// <param name="Builder">The distributed application builder.</param>
/// <param name="LocalTest">The optional LocalTest container resource builder.</param>
public sealed record AltinnStudioResources(
    IDistributedApplicationBuilder Builder,
    IResourceBuilder<ContainerResource> LocalTest);