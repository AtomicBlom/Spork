using Silk.NET.Vulkan;

namespace Spork.Extensions.Khronos.Swapchain;

public class SporkSwapchain : ISporkSwapchain, IInternalSporkSwapchain
{
    private readonly SwapchainCreateInfoKHR _creationInfo;
    private readonly SwapchainKHR _swapchain;
    private SporkSwapchainImageEntry[] _imageEntries;


    public SporkSwapchain(
        SwapchainCreateInfoKHR creationInfo, 
        SwapchainKHR swapchain)
    {
        _creationInfo = creationInfo;
        _swapchain = swapchain;
    }

    SporkSwapchainImageEntry[] IInternalSporkSwapchain.ImageEntries
    {
        set => _imageEntries = value;
    }

    SwapchainKHR ISporkSwapchain.NativeSwapchain => _swapchain;
}