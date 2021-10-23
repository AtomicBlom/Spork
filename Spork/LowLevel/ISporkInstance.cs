using Silk.NET.Vulkan;

namespace Spork.LowLevel;

public interface ISporkInstance
{
    Vk Vulkan { get; }
    Instance NativeInstance { get; }
}