using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using Spork.Extensions;
using Spork.LowLevel;

namespace Spork;

public class SporkInstance : IDisposable, ISporkInstance
{
    private readonly Vk _vk;
    private readonly Instance _nativeInstance;

    Vk ISporkInstance.Vulkan => _vk;
    Instance ISporkInstance.NativeInstance => _nativeInstance;
    
    public SporkInstance(ISpork spork, Instance instance)
    {
        _vk = spork.Vulkan;
        _nativeInstance = instance;
    }

    public unsafe SporkExtensionProperties[] GetExtensions()
    {
        uint pPropertyCount;
        _vk.EnumerateInstanceExtensionProperties(string.Empty, &pPropertyCount, null);
        var extensionNames = new ExtensionProperties[pPropertyCount];
        fixed(ExtensionProperties* pProperties = extensionNames)
            _vk.EnumerateInstanceExtensionProperties(string.Empty, &pPropertyCount, pProperties);

        var availableExtensions = new SporkExtensionProperties[pPropertyCount];
        for (var i = 0; i < pPropertyCount; i++)
        {
            fixed (byte* namePointer = extensionNames[i].ExtensionName)
            {
                var name = Marshal.PtrToStringUTF8((nint) namePointer) ?? throw new Exception("Unexpected extension with no name");
                availableExtensions[i] = new SporkExtensionProperties(name, extensionNames[i].SpecVersion);
            }
        }

        return availableExtensions;
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

    public IReadOnlyList<SporkPhysicalDevice> GetPhysicalDevices(IList<string> requiredExtensions)
    {
        return _vk.GetPhysicalDevices(_nativeInstance)
            .Select(physicalDevice => new SporkPhysicalDevice(_vk, this, physicalDevice))
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

public record SporkExtensionProperties(string Name, uint SpecVersion);