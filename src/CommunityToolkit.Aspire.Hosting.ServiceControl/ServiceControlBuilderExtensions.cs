// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Hosting.ApplicationModel;
using CommunityToolkit.Aspire.Hosting.ServiceControl;

namespace Aspire.Hosting;

/// <summary>
/// Provides extension methods for adding Service Control resources to the application model.
/// </summary>
public static class ServiceControlBuilderExtensions
{
    /// <summary>
    /// Adds a ServiceControl resource to the distributed application builder, configuring it with the specified name,
    /// transport resource, and optional port. This method also sets up a RavenDB container as a dependency,
    /// configures environment variables, health checks, and endpoint URLs for ServiceControl.
    /// </summary>
    /// <param name="builder">The distributed application builder to which the ServiceControl resource will be added.</param>
    /// <param name="name">The name of the ServiceControl resource.</param>
    /// <param name="port">The port on which the ServiceControl HTTP endpoint will be exposed. Defaults to 33333.</param>
    /// <returns>
    /// An <see cref="IResourceBuilder{ServiceControlResource}"/> for further configuration of the ServiceControl resource.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="name"/> is <c>null</c>.</exception>
    public static IResourceBuilder<ServiceControlResource> AddServiceControl(
        this IDistributedApplicationBuilder builder,
        [ResourceName] string name,
        int port = 33333)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        var ravendb = builder.AddContainer($"{name}-ravendb", ServiceControlContainerImageTags.RavenDbImage,
                ServiceControlContainerImageTags.Tag)
            .WithImageRegistry(ServiceControlContainerImageTags.Registry)
            .WithHttpEndpoint(targetPort: 8080, port: 8080)
            .WithUrlForEndpoint("http", url => url.DisplayText = "Management Studio");

        var serviceControlResource = new ServiceControlResource(name, ravendb);

        return builder.AddResource(serviceControlResource)
            .WithImage(ServiceControlContainerImageTags.ServiceControlImage, ServiceControlContainerImageTags.Tag)
            .WithImageRegistry(ServiceControlContainerImageTags.Registry)
            .WithHttpEndpoint(targetPort: 33333, port: port)
            .WithEnvironment("RAVENDB_CONNECTIONSTRING", ravendb.GetEndpoint("http"))
            .WithArgs("--setup-and-run")
            .WithUrlForEndpoint("http", url => url.DisplayLocation = UrlDisplayLocation.DetailsOnly)
            .WithHttpHealthCheck("/api/configuration")
            .WaitFor(ravendb);
    }

    /// <summary>
    /// Configures the ServiceControl resource to use RabbitMQ as the transport mechanism.
    /// This method adds a RabbitMQ container to the application, sets up the transport configuration,
    /// and establishes the necessary connection between ServiceControl and RabbitMQ.
    /// </summary>
    /// <param name="builder">The resource builder for the ServiceControl resource.</param>
    /// <returns>An <see cref="IResourceBuilder{ServiceControlResource}"/> for further configuration of the ServiceControl resource.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <c>null</c>.</exception>
    public static IResourceBuilder<ServiceControlResource> WithRabbitMqTransport(
        this IResourceBuilder<ServiceControlResource> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        const string transportType = "RabbitMQ.QuorumConventionalRouting";

        var transport = builder.ApplicationBuilder.AddRabbitMQ($"{builder.Resource.Name}-rabbitmq")
            .WithManagementPlugin(15672)
            .WithUrlForEndpoint("management", url => url.DisplayText = "RabbitMQ Management")
            .WithParentRelationship(builder.Resource);

        builder.Resource.SetTransport(transport);
        builder.Resource.SetTransportType(transportType);

        return builder
            .WithEnvironment("CONNECTIONSTRING", transport)
            .WithEnvironment("TRANSPORTTYPE", transportType)
            .WaitFor(transport);
    }

    /// <summary>
    /// Adds a ServiceControl Audit instance as a companion to the main ServiceControl resource.
    /// This method configures the audit container, sets up the necessary connections to the main ServiceControl instance,
    /// and configures health checks and endpoints.
    /// </summary>
    /// <param name="builder">The resource builder for the ServiceControl resource.</param>
    /// <param name="port">The port on which the ServiceControl Audit HTTP endpoint will be exposed. Defaults to 44444.</param>
    /// <returns>An <see cref="IResourceBuilder{ServiceControlResource}"/> for further configuration of the ServiceControl resource.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <c>null</c>.</exception>
    public static IResourceBuilder<ServiceControlResource> WithAudit(
        this IResourceBuilder<ServiceControlResource> builder, int? port = 44444)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var audit = builder.ApplicationBuilder.AddContainer($"{builder.Resource.Name}-audit",
                ServiceControlContainerImageTags.ServiceControlAuditImage, ServiceControlContainerImageTags.Tag)
            .WithImageRegistry(ServiceControlContainerImageTags.Registry)
            .WithHttpEndpoint(targetPort: 44444, port: port)
            .WithArgs("--setup-and-run")
            .WithUrlForEndpoint("http", url => url.DisplayLocation = UrlDisplayLocation.DetailsOnly)
            .WithHttpHealthCheck("/api/configuration")
            .WaitFor(builder)
            .WithParentRelationship(builder.Resource);

        builder.Resource.SetAudit(audit);

        return builder
            .WithEnvironment("REMOTEINSTANCES", $"[{{\"api_uri\":\"{audit.GetEndpoint("http")}\"}}]");
    }

    /// <summary>
    /// Adds a ServiceControl Monitoring instance as a companion to the main ServiceControl resource.
    /// This method configures the monitoring container, sets up the necessary connections to the main ServiceControl instance,
    /// and configures health checks and endpoints.
    /// </summary>
    /// <param name="builder">The resource builder for the ServiceControl resource.</param>
    /// <param name="port">The port on which the ServiceControl Monitoring HTTP endpoint will be exposed. Defaults to 33633.</param>
    /// <returns>An <see cref="IResourceBuilder{ServiceControlResource}"/> for further configuration of the ServiceControl resource.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <c>null</c>.</exception>
    public static IResourceBuilder<ServiceControlResource> WithMonitoring(
        this IResourceBuilder<ServiceControlResource> builder, int? port = 33633)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var monitoring = builder.ApplicationBuilder.AddContainer($"{builder.Resource.Name}-monitoring",
                ServiceControlContainerImageTags.ServiceControlMonitoringImage, ServiceControlContainerImageTags.Tag)
            .WithImageRegistry(ServiceControlContainerImageTags.Registry)
            .WithHttpEndpoint(targetPort: 33633, port: port)
            .WithArgs("--setup-and-run")
            .WithUrlForEndpoint("http", url => url.DisplayLocation = UrlDisplayLocation.DetailsOnly)
            .WithHttpHealthCheck("connection")
            .WaitFor(builder)
            .WithParentRelationship(builder.Resource);

        builder.Resource.SetMonitoring(monitoring);

        return builder;
    }

    /// <summary>
    /// Adds a ServicePulse instance as a companion to the main ServiceControl resource.
    /// ServicePulse provides a web interface for monitoring and managing NServiceBus endpoints.
    /// This method configures the ServicePulse container and connects it to ServiceControl.
    /// </summary>
    /// <param name="builder">The resource builder for the ServiceControl resource.</param>
    /// <param name="port">The port on which the ServicePulse HTTP endpoint will be exposed. Defaults to 9090.</param>
    /// <returns>An <see cref="IResourceBuilder{ServiceControlResource}"/> for further configuration of the ServiceControl resource.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <c>null</c>.</exception>
    public static IResourceBuilder<ServiceControlResource> WithServicePulse(
        this IResourceBuilder<ServiceControlResource> builder, int? port = 9090)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var servicePulse = builder.ApplicationBuilder.AddContainer($"{builder.Resource.Name}-servicepulse",
                ServicePulseContainerImageTags.ServicePulseImage, ServicePulseContainerImageTags.Tag)
            .WithImageRegistry(ServicePulseContainerImageTags.Registry)
            .WithHttpEndpoint(targetPort: 9090, port: port)
            .WithEnvironment("ENABLE_REVERSE_PROXY", "true")
            .WithUrlForEndpoint("http", url => url.DisplayText = "ServicePulse")
            .WaitFor(builder);

        builder.Resource.SetServicePulse(servicePulse);

        return builder;
    }

    /// <summary>
    /// Provides extension methods for configuring ServiceControl and its companion resources (Audit, Monitoring, ServicePulse)
    /// within a distributed application using the Aspire hosting model. These methods allow for the addition and configuration
    /// of ServiceControl, RabbitMQ transport, and related containers, including setting up environment variables, health checks,
    /// endpoints, and inter-resource relationships.
    /// </summary>
    public static IResourceBuilder<IResourceWithConnectionString> Transport(this IResourceBuilder<ServiceControlResource> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.Resource.Transport;
    }
}