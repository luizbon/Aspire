// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

/// <summary>
/// Represents a container resource for Service Control with a specified name.
/// Inherits from <see cref="ContainerResource"/>.
/// </summary>
/// <remarks>
/// This class provides methods to configure transport mechanisms, transport types, and connection strings for Service Control.
/// </remarks>
public class ServiceControlResource : ContainerResource, IResourceWithConnectionString
{
    private const string TransportConnectionStringEnvironmentVariable = "CONNECTIONSTRING";
    private const string TransportTypeEnvironmentVariable = "TRANSPORTTYPE";
    private const string RavenDbConnectionStringEnvironmentVariable = "RAVENDB_CONNECTIONSTRING";
    private const string ServiceControlUrlEnvironmentVariable = "SERVICECONTROL_URL";
    private const string MonitoringUrlEnvironmentVariable = "MONITORING_URL";

    private IResourceBuilder<IResourceWithConnectionString>? _transport;
    private string? _transportType;
    private readonly IResourceBuilder<ContainerResource> _ravenDb;
    private IResourceBuilder<ContainerResource>? _audit;
    private IResourceBuilder<ContainerResource>? _monitoring;
    private IResourceBuilder<ContainerResource>? _servicePulse;

    /// <summary>
    /// Gets the transport resource associated with this service control.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the transport has not been configured.
    /// </exception>
    public IResourceBuilder<IResourceWithConnectionString> Transport => _transport ?? throw new InvalidOperationException("Transport not configured");

    /// <summary>
    /// Represents a container resource for Service Control with a specified name.
    /// Inherits from <see cref="ContainerResource"/>.
    /// </summary>
    /// <param name="name">The name of the service control resource.</param>
    /// <param name="ravenDb">The RavenDB resource builder associated with this Service Control resource.</param>
    /// <remarks>
    /// This class provides methods to configure transport mechanisms, transport types, and connection strings for Service Control.
    /// </remarks>
    public ServiceControlResource(string name, IResourceBuilder<ContainerResource> ravenDb) : base(name)
    {
        _ravenDb = ravenDb;
        _ravenDb.WithParentRelationship(this);
    }

    /// <summary>
    /// Sets the transport mechanism to be used by ServiceControl for message processing.
    /// This configures the transport connection string for both the main ServiceControl instance
    /// and any associated audit or monitoring instances.
    /// </summary>
    /// <param name="transport">The resource builder for the transport mechanism.</param>
    public void SetTransport(IResourceBuilder<IResourceWithConnectionString> transport)
    {
        _transport = transport;
        _audit?.WithEnvironment(TransportConnectionStringEnvironmentVariable, transport);
        _monitoring?.WithEnvironment(TransportConnectionStringEnvironmentVariable, transport);
    }

    /// <summary>
    /// Sets the transport type to be used by ServiceControl for message processing.
    /// This configures the transport type for both the main ServiceControl instance
    /// and any associated audit or monitoring instances.
    /// </summary>
    /// <param name="transportType">The type of transport to use, specified as a string identifier.</param>
    public void SetTransportType(string transportType)
    {
        _transportType = transportType;
        _audit?.WithEnvironment(TransportTypeEnvironmentVariable, transportType);
        _monitoring?.WithEnvironment(TransportTypeEnvironmentVariable, transportType);
    }

    /// <summary>
    /// Configures a ServiceControl audit instance and associates it with the main ServiceControl resource.
    /// This method sets up the necessary environment variables for RavenDB connection, transport type,
    /// and transport connection string.
    /// </summary>
    /// <param name="audit">The resource builder for the ServiceControl audit container.</param>
    public void SetAudit(IResourceBuilder<ContainerResource> audit)
    {
        _audit = audit;
        _audit.WithEnvironment(RavenDbConnectionStringEnvironmentVariable, _ravenDb.GetEndpoint("http"));
        if (_transportType is not null)
        {
            _audit.WithEnvironment(TransportTypeEnvironmentVariable, _transportType);
        }

        if (_transport is not null)
        {
            _audit.WithEnvironment(TransportConnectionStringEnvironmentVariable, _transport);
        }
    }

    /// <summary>
    /// Configures a ServiceControl monitoring instance and associates it with the main ServiceControl resource.
    /// This method sets up the necessary environment variables for RavenDB connection, transport type,
    /// and transport connection string.
    /// </summary>
    /// <param name="monitoring">The resource builder for the ServiceControl monitoring container.</param>
    public void SetMonitoring(IResourceBuilder<ContainerResource> monitoring)
    {
        _monitoring = monitoring;
        _monitoring.WithEnvironment(RavenDbConnectionStringEnvironmentVariable, _ravenDb.GetEndpoint("http"));
        if (_transportType is not null)
        {
            _monitoring.WithEnvironment(TransportTypeEnvironmentVariable, _transportType);
        }

        if (_transport is not null)
        {
            _monitoring.WithEnvironment(TransportConnectionStringEnvironmentVariable, _transport);
        }

        if (_servicePulse is not null)
        {
            _servicePulse.WithEnvironment(MonitoringUrlEnvironmentVariable, _monitoring.GetEndpoint("http"));
        }
    }

    /// <summary>
    /// Configures a ServicePulse instance and associates it with the main ServiceControl resource.
    /// This method sets up the necessary environment variables for ServiceControl connection
    /// and optionally connects to the monitoring instance if available.
    /// </summary>
    /// <param name="servicePulse">The resource builder for the ServicePulse container.</param>
    public void SetServicePulse(IResourceBuilder<ContainerResource> servicePulse)
    {
        _servicePulse = servicePulse;
        _servicePulse.WithEnvironment(ServiceControlUrlEnvironmentVariable, this.GetEndpoint("http"));
        if (_monitoring is not null)
        {
            _servicePulse.WithEnvironment(MonitoringUrlEnvironmentVariable, _monitoring.GetEndpoint("http"));
        }
    }

    /// <summary>
    /// Gets the connection string expression for the Service Control resource.
    /// </summary>
    /// <value>
    /// The connection string expression derived from the transport resource.
    /// </value>
    /// <exception cref="InvalidOperationException">Thrown if the transport is not configured.</exception>
    public ReferenceExpression ConnectionStringExpression => _transport?.Resource.ConnectionStringExpression ?? throw new InvalidOperationException("Transport not configured");
}