using Spork.LowLevel;

namespace Spork.Extensions.Khronos.Swapchain;

public class SporkSwapchainImageEntry
{
    private readonly ISporkLogicalDevice _device;
    private readonly SporkImageView _sporkImageView;
    private readonly SporkImage _swapchainImage;

    public SporkSwapchainImageEntry(ISporkLogicalDevice device, SporkImageView sporkImageView, SporkImage swapchainImage)
    {
        _device = device;
        _sporkImageView = sporkImageView;
        _swapchainImage = swapchainImage;
    }
}