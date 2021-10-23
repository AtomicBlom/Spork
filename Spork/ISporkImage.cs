using Silk.NET.Vulkan;

namespace Spork.Extensions.Khronos.Swapchain;

public interface ISporkImage
{
    Image NativeImage { get; }
}