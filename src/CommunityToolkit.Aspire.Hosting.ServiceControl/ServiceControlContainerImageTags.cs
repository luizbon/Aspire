// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CommunityToolkit.Aspire.Hosting.ServiceControl;

/// <summary>
/// Provides constants for container image tags used by ServiceControl resources.
/// These constants define the registry, image names, and version tags for various
/// ServiceControl-related container images.
/// </summary>
internal class ServiceControlContainerImageTags
{
    /// <summary>
    /// The registry URL for the container images.
    /// </summary>
    public const string Registry = "docker.io";

    /// <summary>
    /// The image name for the ServiceControl RavenDB container.
    /// </summary>
    public const string RavenDbImage = "particular/servicecontrol-ravendb";

    /// <summary>
    /// The image name for the ServiceControl container.
    /// </summary>
    public const string ServiceControlImage = "particular/servicecontrol";

    /// <summary>
    /// The image name for the ServiceControl Monitoring container.
    /// </summary>
    public const string ServiceControlMonitoringImage = "particular/servicecontrol-monitoring";

    /// <summary>
    /// The image name for the ServiceControl Audit container.
    /// </summary>
    public const string ServiceControlAuditImage = "particular/servicecontrol-audit";

    /// <summary>
    /// The default tag for the ServiceControl container images.
    /// </summary>
    public const string Tag = "6.6";
}
