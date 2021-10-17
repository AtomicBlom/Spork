using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;

namespace Spork;

public class SporkInstance : IDisposable
{
    private readonly Vk _vk;
    public Instance NativeInstance { get; }
    public IReadOnlyList<string> MandatoryDeviceExtensions { get; init; }
    public IReadOnlyList<string> DesiredDeviceExtensions { get; init; }

    public SporkInstance(Vk vk, Instance instance)
    {
        _vk = vk;
        NativeInstance = instance;
    }

    public bool TryGetExtension<TExtension, TNativeExtension>(out TExtension extension) where TExtension : ISporkInstanceExtension<TNativeExtension>, new() where TNativeExtension : NativeExtension<Vk>
    {
        if (!_vk.TryGetInstanceExtension(NativeInstance, out TNativeExtension nativeExtension))
        {
            extension = default!;
            return false;
        }

        extension = new TExtension
        {
            NativeExtension = nativeExtension,
            Instance = this
        };
        return true;
    }

    public unsafe SurfaceKHR CreateSurface(IWindow window)
    {
        return window.VkSurface!.Create<AllocationCallbacks>(NativeInstance.ToHandle(), null).ToSurface();
    }

    public IReadOnlyList<SporkPhysicalDevice> GetPhysicalDevices(KhrSurface khronosSurfaceExtension, SurfaceKHR khronosSurface)
    {
        var devices = _vk.GetPhysicalDevices(NativeInstance)
            .Select(device => ResolvePhysicalDevice(device, khronosSurfaceExtension, khronosSurface))
            .Where(device => MandatoryDeviceExtensions.All(device.HasExtension) &&
                             device.IsComplete && device.IsSwapChainAdequate
            {
                
            })
            .ToList();
    }

    private unsafe SporkPhysicalDevice ResolvePhysicalDevice(PhysicalDevice device, KhrSurface khronosSurfaceExtension, SurfaceKHR khronosSurface)
    {
        var indices = FindQueueFamilies(device, khronosSurfaceExtension, khronosSurface);
        var swapchainSupport = QuerySwapChainSupport(device, khronosSurfaceExtension, khronosSurface);

        return new SporkPhysicalDevice(_vk, device, indices, swapchainSupport);
    }

    private unsafe (uint? GraphicsFamily, uint? PresentFamily) FindQueueFamilies(PhysicalDevice device, KhrSurface khronosSurfaceExtension, SurfaceKHR khronosSurface)
    {
        //Query the QueueFamilyProperties for the device and populate queueFamilies with it
        uint queryFamilyCount = 0;
        _vk.GetPhysicalDeviceQueueFamilyProperties(device, &queryFamilyCount, null);
        using var mem = GlobalMemory.Allocate((int)queryFamilyCount * sizeof(QueueFamilyProperties));
        var queueFamilies = (QueueFamilyProperties*)Unsafe.AsPointer(ref mem.GetPinnableReference());
        _vk.GetPhysicalDeviceQueueFamilyProperties(device, &queryFamilyCount, queueFamilies);

        //Find a suitable indices.
        uint? graphicsFamily = null;
        uint? presentFamily = null;
        for (uint i = 0; i < queryFamilyCount; i++)
        {
            var queueFamily = queueFamilies[i];
            if (queueFamily.QueueFlags.HasFlag(QueueFlags.QueueGraphicsBit))
            {
                graphicsFamily = i;
            }

            khronosSurfaceExtension.GetPhysicalDeviceSurfaceSupport(device, i, khronosSurface, out var presentSupport);
            if (presentSupport == Vk.True)
            {
                presentFamily = i;
            }

            if (graphicsFamily.HasValue && presentFamily.HasValue) break;
        }

        return (graphicsFamily, presentFamily);
    }

    private unsafe SwapChainSupportDetails QuerySwapChainSupport(PhysicalDevice device, KhrSurface khronosSurfaceExtension, SurfaceKHR khronosSurface)
    {
        var details = new SwapChainSupportDetails();

        khronosSurfaceExtension.GetPhysicalDeviceSurfaceCapabilities(device, khronosSurface, out var capabilities);
        details.Capabilities = capabilities;

        uint formatCount;
        khronosSurfaceExtension.GetPhysicalDeviceSurfaceFormats(device, khronosSurface, &formatCount, null);
        if (formatCount != 0)
        {
            details.Formats = new SurfaceFormatKHR[formatCount];
            using var mem = GlobalMemory.Allocate((int)formatCount * sizeof(SurfaceFormatKHR));
            var formats = (SurfaceFormatKHR*)Unsafe.AsPointer(ref mem.GetPinnableReference());

            khronosSurfaceExtension.GetPhysicalDeviceSurfaceFormats(device, khronosSurface, &formatCount, formats);

            for (var i = 0; i < formatCount; i++)
            {
                details.Formats[i] = formats[i];
            }
        }

        uint presentModeCount;
        khronosSurfaceExtension.GetPhysicalDeviceSurfacePresentModes(device, khronosSurface, &presentModeCount, null);
        if (presentModeCount != 0)
        {
            details.PresentModes = new PresentModeKHR[presentModeCount];
            using var mem = GlobalMemory.Allocate((int)presentModeCount * sizeof(PresentModeKHR));
            var presentModes = (PresentModeKHR*)Unsafe.AsPointer(ref mem.GetPinnableReference());

            khronosSurfaceExtension.GetPhysicalDeviceSurfacePresentModes(device, khronosSurface, &presentModeCount, presentModes);
            for (var i = 0; i < presentModeCount; i++)
            {
                details.PresentModes[i] = presentModes[i];
            }
        }

        return details;
    }

    private unsafe void ReleaseUnmanagedResources()
    {
        _vk.DestroyInstance(NativeInstance, null);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~SporkInstance()
    {
        ReleaseUnmanagedResources();
    }
}