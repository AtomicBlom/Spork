using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using Spork;

public class SporkKhronosSurfaceExtension : ISporkInstanceExtension<KhrSurface>
{
    private readonly KhrSurface _nativeExtension;
    private readonly Instance _instance;

    KhrSurface ISporkInstanceExtension<KhrSurface>.NativeExtension
    {
        init => _nativeExtension = value;
    }

    IInternalSporkInstance ISporkInstanceExtension<KhrSurface>.Instance
    {
        init => _instance = value.NativeInstance;
    }

    public unsafe SporkSurface CreateSurface(IWindow window)
    {
        
        var surface = window.VkSurface!.Create<AllocationCallbacks>(_instance.ToHandle(), null).ToSurface();
        return new SporkSurface(_instance, _nativeExtension, surface);
    }
}