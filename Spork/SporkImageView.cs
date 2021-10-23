using Silk.NET.Vulkan;

namespace Spork.LowLevel;

public class SporkImageView : IDisposable
{
    private readonly Vk _vk;
    private readonly ISporkLogicalDevice _device;
    private readonly ImageView _imageView;

    public SporkImageView(Vk vk, ISporkLogicalDevice device, ImageView imageView)
    {
        _vk = vk;
        _device = device;
        _imageView = imageView;
    }

    private unsafe void ReleaseUnmanagedResources()
    {
        _vk.DestroyImageView(_device.NativeDevice, _imageView, null);
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~SporkImageView()
    {
        ReleaseUnmanagedResources();
    }
}