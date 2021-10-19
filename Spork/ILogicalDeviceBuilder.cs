namespace Spork;

public interface ILogicalDeviceBuilder
{
    ILogicalDeviceBuilder WithQueue(uint queueIndex, Action<global::Silk.NET.Vulkan.Queue> createdQueue, float priority = 1f);
    ILogicalDeviceBuilder WithValidationLayers(params string[]? validationLayers);
    ILogicalDeviceBuilder WithDeviceExtensions(params string[]? requiredExtensions);
    SporkLogicalDevice Create();
    
}