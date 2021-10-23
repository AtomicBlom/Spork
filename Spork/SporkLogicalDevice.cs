using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Spork.Extensions;
using Spork.LowLevel;

namespace Spork;

public class SporkLogicalDevice : ISporkLogicalDevice, IDisposable
{
    private readonly Vk _vk;
    private readonly ISporkInstance _instance;
    private readonly ISporkPhysicalDevice _physicalDevice;
    private readonly Device _device;

    public SporkLogicalDevice(Vk vk, ISporkPhysicalDevice physicalDevice, Device device)
    {
        _vk = vk;
        _instance = physicalDevice.Instance;
        _physicalDevice = physicalDevice;
        _device = device;
    }

    Device ISporkLogicalDevice.NativeDevice => _device;

    ISporkPhysicalDevice ISporkLogicalDevice.PhysicalDevice => _physicalDevice;

    //FIXME: This should probably be fluent
    public unsafe SporkImageView CreateImageView(ImageViewCreateInfo imageViewCreateInfo)
    {
        if (_vk.CreateImageView(_device, imageViewCreateInfo, null, out var imageView) != Result.Success)
        {
            throw new Exception("Failed to create image views!");
        }

        return new SporkImageView(_vk, this, imageView);
    }

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

    public bool TryGetExtension<TExtension, TNativeExtension>(out TExtension extension) where TExtension : ISporkDeviceExtension<TNativeExtension>, new() where TNativeExtension : NativeExtension<Vk>
    {
        if (!_vk.TryGetDeviceExtension(_instance.NativeInstance, _device, out TNativeExtension nativeExtension))
        {
            extension = default!;
            return false;
        }

        extension = new TExtension
        {
            NativeExtension = nativeExtension,
            Device = this
        };
        return true;
    }
}