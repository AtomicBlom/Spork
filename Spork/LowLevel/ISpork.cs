using Silk.NET.Vulkan;

namespace Spork.LowLevel;

public interface ISpork
{
    Vk Vulkan { get; }
}