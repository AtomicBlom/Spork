using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Spork;

public class SporkSurface : IDisposable
{
    private readonly Instance _instance;
    private readonly KhrSurface _nativeExtension;
    private readonly SurfaceKHR _surface;

    public SporkSurface(Instance instance, KhrSurface nativeExtension, SurfaceKHR surface)
    {
        _instance = instance;
        _nativeExtension = nativeExtension;
        _surface = surface;
    }

    public bool DoesQueueSupportPresentation(ISporkPhysicalDevice physicalDevice, uint index)
    {
        _nativeExtension.GetPhysicalDeviceSurfaceSupport(physicalDevice.VulkanPhysicalDevice, index, _surface, out var presentSupport);
        return presentSupport == Vk.True;
    }

    public SurfaceCapabilitiesKHR GetPhysicalDeviceSurfaceCapabilities(ISporkPhysicalDevice physicalDevice)
    {
        _nativeExtension.GetPhysicalDeviceSurfaceCapabilities(physicalDevice.VulkanPhysicalDevice, _surface, out var capabilities);
        return capabilities;
    }

    public unsafe SurfaceFormatKHR[] GetPhysicalDeviceSurfaceFormats(ISporkPhysicalDevice physicalDevice)
    {
        uint formatCount;
        _nativeExtension.GetPhysicalDeviceSurfaceFormats(physicalDevice.VulkanPhysicalDevice, _surface, &formatCount, null);
        if (formatCount == 0) return Array.Empty<SurfaceFormatKHR>();
        var formats = new SurfaceFormatKHR[formatCount];
        using var mem = GlobalMemory.Allocate((int)formatCount * sizeof(SurfaceFormatKHR));
        var formatsPointer = (SurfaceFormatKHR*)Unsafe.AsPointer(ref mem.GetPinnableReference());

        _nativeExtension.GetPhysicalDeviceSurfaceFormats(physicalDevice.VulkanPhysicalDevice, _surface, &formatCount, formatsPointer);

        for (var i = 0; i < formatCount; i++)
        {
            formats[i] = formatsPointer[i];
        }

        return formats;

    }

    public unsafe PresentModeKHR[] GetPhysicalDeviceSurfacePresentModes(ISporkPhysicalDevice physicalDevice)
    {
        uint presentModeCount;
        _nativeExtension.GetPhysicalDeviceSurfacePresentModes(physicalDevice.VulkanPhysicalDevice, _surface, &presentModeCount, null);
        if (presentModeCount == 0) return Array.Empty<PresentModeKHR>();
        var presentModes = new PresentModeKHR[presentModeCount];
        using var mem = GlobalMemory.Allocate((int)presentModeCount * sizeof(PresentModeKHR));
        var presentModesPointer = (PresentModeKHR*)Unsafe.AsPointer(ref mem.GetPinnableReference());

        _nativeExtension.GetPhysicalDeviceSurfacePresentModes(physicalDevice.VulkanPhysicalDevice, _surface, &presentModeCount, presentModesPointer);
        for (var i = 0; i < presentModeCount; i++)
        {
            presentModes[i] = presentModesPointer[i];
        }

        return presentModes;
    }

    private unsafe void ReleaseUnmanagedResources()
    {
        _nativeExtension.DestroySurface(_instance, _surface, null);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~SporkSurface()
    {
        ReleaseUnmanagedResources();
    }
}