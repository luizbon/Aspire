using CommunityToolkit.Aspire.Shipping;
using Npgsql;
using NpgsqlTypes;

var builder = Host.CreateApplicationBuilder(args);
var endpointConfiguration = new EndpointConfiguration("Shipping");
endpointConfiguration.EnableOpenTelemetry();

builder.AddServiceDefaults();


var connectionString = builder.Configuration.GetConnectionString("transport");
var transport = new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), connectionString);

var routing = endpointConfiguration.UseTransport(transport);

var persistenceConnection = builder.Configuration.GetConnectionString("shipping-db");
var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
persistence.ConnectionBuilder(
    connectionBuilder: () =>
    {
        return new NpgsqlConnection(persistenceConnection);
    });

var dialect = persistence.SqlDialect<SqlDialect.PostgreSql>();
dialect.JsonBParameterModifier(
    modifier: parameter =>
    {
        var npgsqlParameter = (NpgsqlParameter)parameter;
        npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
    });

endpointConfiguration.UseSerialization<SystemJsonSerializer>();
endpointConfiguration.SendHeartbeatTo("Particular.ServiceControl");

var metrics = endpointConfiguration.EnableMetrics();
metrics.SendMetricDataToServiceControl("Particular.Monitoring", TimeSpan.FromSeconds(1));

endpointConfiguration.EnableInstallers();

builder.UseNServiceBus(endpointConfiguration);

var host = builder.Build();
host.Run();
