# CommunityToolkit.Aspire.Hosting.ServiceControl

## Overview

This .NET Aspire Integration provides hosting support for ServiceControl, enabling seamless integration with NServiceBus and Particular ServiceControl resources. It includes configurations for ServicePulse, RabbitMQ transport, monitoring, and auditing.

## Getting Started

### Install the package

In your AppHost project, install the package using the following command:

```dotnetcli
dotnet add package CommunityToolkit.Aspire.Hosting.ServiceControl
```

### Example usage

In the _Program.cs_ file of your AppHost project, define ServiceControl resources and consume the connection using the following methods:

```csharp
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var serviceControl = builder.AddServiceControl("servicecontrol")
    .WithServicePulse()
    .WithRabbitMqTransport()
    .WithMonitoring()
    .WithAudit();

builder.AddProject<Projects.CommunityToolkit_Aspire_NServiceBus_Billing>("Billing")
    .WithReference(serviceControl.Transport(), "transport") // transport is the connection string name to be used
    .WaitFor(serviceControl);

builder.Build().Run();
```

In your project, setup your NServiceBus endpoint as usual.

```csharp
var endpointConfiguration = new EndpointConfiguration("Billing");
endpointConfiguration.EnableOpenTelemetry();

var connectionString = builder.Configuration.GetConnectionString("transport"); // connection string name matching with the transport
var transport = new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), connectionString);
var routing = endpointConfiguration.UseTransport(transport);
```

## Features

-   **ServicePulse Integration**: Provides real-time monitoring and visualization of NServiceBus endpoints.
-   **RabbitMQ Transport**: Configures RabbitMQ as the transport mechanism for ServiceControl.
-   **Monitoring and Auditing**: Enables ServiceControl Monitoring and Audit containers for enhanced diagnostics.

## Additional Information

-   [NServiceBus](https://docs.particular.net/nservicebus/)
-   [ServiceControl](https://docs.particular.net/servicecontrol/)
-   [ServicePulse](https://docs.particular.net/servicepulse/)

