// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace CommunityToolkit.Aspire.Hosting.ServiceControl;

/// <summary>
/// Provides constants for container image tags used by ServicePulse resources.
/// These constants define the registry, image name, and version tag for the
/// ServicePulse container image.
/// </summary>
internal class ServicePulseContainerImageTags
{
    /// <summary>
    /// The registry URL for the ServicePulse container image.
    /// </summary>
    public const string Registry = "docker.io";

    /// <summary>
    /// The image name for the ServicePulse container.
    /// </summary>
    public const string ServicePulseImage = "particular/servicepulse";

    /// <summary>
    /// The default tag for the ServicePulse container image.
    /// </summary>
    public const string Tag = "2.0";
}