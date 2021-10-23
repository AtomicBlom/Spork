using Silk.NET.Vulkan;

namespace Spork.LowLevel;

public interface ISporkPhysicalDevice
{
    ISporkInstance Instance { get; }
    PhysicalDevice VulkanPhysicalDevice { get; }
}