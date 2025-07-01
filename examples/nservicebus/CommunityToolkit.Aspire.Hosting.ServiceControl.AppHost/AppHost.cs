using Aspire.Hosting; // Required for extension methods

var builder = DistributedApplication.CreateBuilder(args);

var postgresServer = builder.AddPostgres("database")
    .WithPgAdmin();

var shippingDb = postgresServer.AddDatabase("shipping-db");

var serviceControl = builder.AddServiceControl("servicecontrol")
    .WithServicePulse()
    .WithRabbitMqTransport()
    .WithMonitoring()
    .WithAudit();

builder.AddProject<Projects.CommunityToolkit_Aspire_NServiceBus_Billing>("Billing")
    .WithReference(serviceControl.Transport(), "transport")
    .WaitFor(serviceControl);

var sales = builder.AddProject<Projects.CommunityToolkit_Aspire_NServiceBus_Sales>("Sales")
    .WithReference(serviceControl.Transport(), "transport")
    .WaitFor(serviceControl);

builder.AddProject<Projects.CommunityToolkit_Aspire_NServiceBus_ClientUI>("ClientUI")
    .WithReference(serviceControl.Transport(), "transport")
    .WaitFor(sales)
    .WaitFor(serviceControl);

builder.AddProject<Projects.CommunityToolkit_Aspire_NServiceBus_Shipping>("Shipping")
    .WithReference(serviceControl.Transport(), "transport")
    .WithReference(shippingDb)
    .WaitFor(shippingDb)
    .WaitFor(serviceControl);

builder.Build().Run();
