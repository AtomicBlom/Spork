using Silk.NET.Vulkan.Extensions.KHR;
using Spork.Extensions.Khronos.Surface;
using Spork.LowLevel;

namespace Spork.Extensions.Khronos.Swapchain;

public class SporkKhronosSwapchainExtension : ISporkDeviceExtension<KhrSwapchain>
{
    private readonly ISporkLogicalDevice _device = null!;
    private readonly KhrSwapchain _nativeExtension = null!;

    KhrSwapchain ISporkDeviceExtension<KhrSwapchain>.NativeExtension
    {
        init => _nativeExtension = value;
    }

    ISporkLogicalDevice ISporkDeviceExtension<KhrSwapchain>.Device
    {
        init => _device = value;
    }

    public ISwapchainDefinition DefineSwapchain(SporkSurface surface)
    {
        return new SwapchainDefinition(_device, surface, _nativeExtension);
    }
}