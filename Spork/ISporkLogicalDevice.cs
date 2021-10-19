using Silk.NET.Vulkan;

namespace Spork;

public interface ISporkLogicalDevice
{
    Device Device { get; }
    PhysicalDevice PhysicalDevice { get; }
}