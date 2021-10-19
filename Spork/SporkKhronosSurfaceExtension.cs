using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Spork;

public class SporkKhronosSurfaceExtension : ISporkInstanceExtension<KhrSurface>
{
    private readonly KhrSurface _nativeExtension;

    KhrSurface ISporkInstanceExtension<KhrSurface>.NativeExtension
    {
        init => _nativeExtension = value;
    }

    IInternalSporkInstance ISporkInstanceExtension<KhrSurface>.Instance { init { } }

    public bool DoesQueueSupportPresentation(ISporkPhysicalDevice physicalDevice, uint index, SurfaceKHR surface)
    {
        _nativeExtension.GetPhysicalDeviceSurfaceSupport(physicalDevice.VulkanPhysicalDevice, index, surface, out var presentSupport);
        return presentSupport == Vk.True;
    }

    public SurfaceCapabilitiesKHR GetPhysicalDeviceSurfaceCapabilities(ISporkPhysicalDevice physicalDevice, SurfaceKHR surfaceKhronos)
    {
        _nativeExtension.GetPhysicalDeviceSurfaceCapabilities(physicalDevice.VulkanPhysicalDevice, surfaceKhronos, out var capabilities);
        return capabilities;
    }

    public unsafe SurfaceFormatKHR[] GetPhysicalDeviceSurfaceFormats(ISporkPhysicalDevice physicalDevice, SurfaceKHR surface)
    {
        uint formatCount;
        _nativeExtension.GetPhysicalDeviceSurfaceFormats(physicalDevice.VulkanPhysicalDevice, surface, &formatCount, null);
        if (formatCount == 0) return Array.Empty<SurfaceFormatKHR>();
        var formats = new SurfaceFormatKHR[formatCount];
        using var mem = GlobalMemory.Allocate((int)formatCount * sizeof(SurfaceFormatKHR));
        var formatsPointer = (SurfaceFormatKHR*)Unsafe.AsPointer(ref mem.GetPinnableReference());

        _nativeExtension.GetPhysicalDeviceSurfaceFormats(physicalDevice.VulkanPhysicalDevice, surface, &formatCount, formatsPointer);

        for (var i = 0; i < formatCount; i++)
        {
            formats[i] = formatsPointer[i];
        }

        return formats;

    }

    public unsafe PresentModeKHR[] GetPhysicalDeviceSurfacePresentModes(ISporkPhysicalDevice physicalDevice, SurfaceKHR surfaceKhronos)
    {
        uint presentModeCount;
        _nativeExtension.GetPhysicalDeviceSurfacePresentModes(physicalDevice.VulkanPhysicalDevice, surfaceKhronos, &presentModeCount, null);
        if (presentModeCount == 0) return Array.Empty<PresentModeKHR>();
        var presentModes = new PresentModeKHR[presentModeCount];
        using var mem = GlobalMemory.Allocate((int) presentModeCount * sizeof(PresentModeKHR));
        var presentModesPointer = (PresentModeKHR*) Unsafe.AsPointer(ref mem.GetPinnableReference());

        _nativeExtension.GetPhysicalDeviceSurfacePresentModes(physicalDevice.VulkanPhysicalDevice, surfaceKhronos, &presentModeCount, presentModesPointer);
        for (var i = 0; i < presentModeCount; i++)
        {
            presentModes[i] = presentModesPointer[i];
        }

        return presentModes;
    }
}