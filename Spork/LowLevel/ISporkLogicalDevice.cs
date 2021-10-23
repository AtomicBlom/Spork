using Silk.NET.Vulkan;

namespace Spork.LowLevel;

public interface ISporkLogicalDevice
{
    Device NativeDevice { get; }
    ISporkPhysicalDevice PhysicalDevice { get; }
    SporkImageView CreateImageView(ImageViewCreateInfo imageViewCreateInfo);
}