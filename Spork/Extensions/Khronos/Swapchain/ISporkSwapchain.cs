using Silk.NET.Vulkan;

namespace Spork.Extensions.Khronos.Swapchain;

public interface ISporkSwapchain
{
    SwapchainKHR NativeSwapchain { get; }
}