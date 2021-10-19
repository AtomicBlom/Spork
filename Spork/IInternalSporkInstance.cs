using Silk.NET.Vulkan;

namespace Spork;

public interface IInternalSporkInstance
{
    Vk Vulkan { get; }
    Instance NativeInstance { get; }
}