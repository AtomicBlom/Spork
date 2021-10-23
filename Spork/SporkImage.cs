using Silk.NET.Vulkan;

namespace Spork.Extensions.Khronos.Swapchain;

public class SporkImage : ISporkImage
{
    private readonly Image _image;

    public SporkImage(Image image)
    {
        _image = image;
    }

    Image ISporkImage.NativeImage => _image;
}