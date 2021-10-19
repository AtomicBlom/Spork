using Silk.NET.Vulkan;

namespace Spork;

public class SporkLogicalDevice : ISporkLogicalDevice, IDisposable
{
    private readonly Vk _vk;
    private readonly PhysicalDevice _physicalDevice;
    private readonly Device _device;

    public SporkLogicalDevice(Vk vk, PhysicalDevice physicalDevice, Device device)
    {
        _vk = vk;
        _physicalDevice = physicalDevice;
        _device = device;
    }

    Device ISporkLogicalDevice.Device => _device;

    PhysicalDevice ISporkLogicalDevice.PhysicalDevice => _physicalDevice;

    private unsafe void ReleaseUnmanagedResources()
    {
        _vk.DestroyDevice(_device, null);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~SporkLogicalDevice()
    {
        ReleaseUnmanagedResources();
    }
}