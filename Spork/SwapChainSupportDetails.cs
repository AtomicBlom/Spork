using Silk.NET.Vulkan;

namespace Spork;

public record struct SwapChainSupportDetails(
    SurfaceCapabilitiesKHR Capabilities,
    SurfaceFormatKHR[] Formats,
    PresentModeKHR[] PresentModes
);