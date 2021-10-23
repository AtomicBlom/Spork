using Silk.NET.Maths;
using Silk.NET.Vulkan;

namespace Spork.Extensions.Khronos.Swapchain;

public interface ISwapchainDefinition
{
    SporkSwapchain Create(DisposableSet disposableSet);
    ISwapchainDefinition WithQueueFamily(uint queueFamilyIndex);
    ISwapchainDefinition WithSurfaceFormat(SurfaceFormatKHR surfaceFormat);
    ISwapchainDefinition WithImageExtent(Vector2D<int> extent);
    ISwapchainDefinition WithPresentMode(PresentModeKHR presentMode);
    ISwapchainDefinition WithImageCount(uint imageCount);
    bool CanCreate { get; }
}