using Silk.NET.Vulkan;

namespace Spork;

public class SporkPhysicalDevice
{
    private readonly Vk _vk;
    private readonly PhysicalDevice _physicalDevice;
    private readonly SwapChainSupportDetails _swapchainSupport;

    public SporkPhysicalDevice(Vk vk, PhysicalDevice physicalDevice, (uint? GraphicsFamily, uint? PresentFamily) indices, SwapChainSupportDetails swapchainSupport)
    {
        _vk = vk;
        _physicalDevice = physicalDevice;
        GraphicsFamily = indices.GraphicsFamily;
        PresentFamily = indices.PresentFamily;
        _swapchainSupport = swapchainSupport;
    }

    public uint? PresentFamily { get; init; }

    public uint? GraphicsFamily { get; init; }

    public bool HasExtension(string extension)
    {
        return _vk.IsDeviceExtensionPresent(_physicalDevice, extension);
    }

    internal bool IsComplete => GraphicsFamily.HasValue && PresentFamily.HasValue;
}