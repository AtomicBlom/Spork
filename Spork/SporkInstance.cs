using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;

namespace Spork;

public class SporkInstance : IDisposable, IInternalSporkInstance
{
    private readonly Vk _vk;
    private readonly Instance _nativeInstance;

    Vk IInternalSporkInstance.Vulkan => _vk;
    Instance IInternalSporkInstance.NativeInstance => _nativeInstance;
    
    public SporkInstance(IInternalSpork spork, Instance instance)
    {
        _vk = spork.Vulkan;
        _nativeInstance = instance;
    }

    public bool TryGetExtension<TExtension, TNativeExtension>(out TExtension extension) where TExtension : ISporkInstanceExtension<TNativeExtension>, new() where TNativeExtension : NativeExtension<Vk>
    {
        if (!_vk.TryGetInstanceExtension(_nativeInstance, out TNativeExtension nativeExtension))
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
        return window.VkSurface!.Create<AllocationCallbacks>(_nativeInstance.ToHandle(), null).ToSurface();
    }

    public IReadOnlyList<SporkPhysicalDevice> GetPhysicalDevices(IList<string> requiredExtensions)
    {
        return _vk.GetPhysicalDevices(_nativeInstance)
            .Select(physicalDevice => new SporkPhysicalDevice(_vk, physicalDevice))
            .Where(physicalDevice => requiredExtensions.All(physicalDevice.HasExtension))
            .ToList();
    }

    private unsafe void ReleaseUnmanagedResources()
    {
        _vk.DestroyInstance(_nativeInstance, null);
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