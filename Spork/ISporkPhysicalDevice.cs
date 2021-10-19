using Silk.NET.Vulkan;

namespace Spork;

public interface ISporkPhysicalDevice
{
    PhysicalDevice VulkanPhysicalDevice { get; }
}