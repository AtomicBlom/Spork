using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Spork.Extensions.Khronos.Surface;
using Spork.LowLevel;

namespace Spork.Extensions.Khronos.Swapchain;

public class SwapchainDefinition : ISwapchainDefinition
{
    private readonly ISporkLogicalDevice _device;
    private readonly ISporkSurface _surface;
    private readonly KhrSwapchain _nativeExtension;
    private SwapchainCreateInfoKHR _swapchainCreateInfo;
    private readonly List<uint> _queueFamilyIndices = new();
    private uint? _requestedImageCount = null;

    public unsafe SwapchainDefinition(ISporkLogicalDevice device, ISporkSurface surface, KhrSwapchain nativeExtension)
    {
        _device = device;
        _surface = surface;
        _nativeExtension = nativeExtension;
        _swapchainCreateInfo = new SwapchainCreateInfoKHR(StructureType.SwapchainCreateInfoKhr)
        {
            Surface = surface.NativeSurface,
            ImageArrayLayers = 1,
            CompositeAlpha = CompositeAlphaFlagsKHR.CompositeAlphaOpaqueBitKhr,
            Clipped = Vk.True,
            OldSwapchain = default
        };
    }

    public unsafe SporkSwapchain Create(DisposableSet disposableSet)
    {
        var capabilities = _surface.GetPhysicalDeviceSurfaceCapabilities(_device.PhysicalDevice);
        var imageCount = _requestedImageCount ?? (capabilities.MinImageCount + 1);
        if (capabilities.MaxImageCount > 0 && imageCount > capabilities.MaxImageCount)
        {
            imageCount = capabilities.MaxImageCount;
        }

        _swapchainCreateInfo.MinImageCount = imageCount;
        _swapchainCreateInfo.PreTransform = capabilities.CurrentTransform;

        var queueFamilyIndices = _queueFamilyIndices.ToArray();
        SwapchainKHR nativeSwapchain;
        fixed (uint* queueFamilyIndicesPointer = queueFamilyIndices)
        {
            if (queueFamilyIndices.Length > 1)
            {
                _swapchainCreateInfo.ImageSharingMode = SharingMode.Concurrent;
                _swapchainCreateInfo.QueueFamilyIndexCount = 2;
                _swapchainCreateInfo.PQueueFamilyIndices = queueFamilyIndicesPointer;
            }
            else
            {
                _swapchainCreateInfo.ImageSharingMode = SharingMode.Exclusive;
            }
            
            if (_nativeExtension.CreateSwapchain(_device.NativeDevice, _swapchainCreateInfo, null, out nativeSwapchain) != Result.Success)
            {
                throw new Exception("Failed to create the swapchain");
            }
        }
        var swapchain = new SporkSwapchain(_swapchainCreateInfo, nativeSwapchain);
        var swapchainImages = GetSwapChainImages(swapchain);
        
        var imageViewCreateInfo = new ImageViewCreateInfo(StructureType.ImageViewCreateInfo)
        {
            ViewType = ImageViewType.ImageViewType2D,
            Format = _swapchainCreateInfo.ImageFormat,
            Components = new ComponentMapping
            {
                R = ComponentSwizzle.Identity,
                G = ComponentSwizzle.Identity,
                B = ComponentSwizzle.Identity,
                A = ComponentSwizzle.Identity,
            },
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = ImageAspectFlags.ImageAspectColorBit,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1
            }
        };

        SporkSwapchainImageEntry[] swapchainEntries = new SporkSwapchainImageEntry[swapchainImages.Length];
        for (var i = 0; i < swapchainImages.Length; i++)
        {
            imageViewCreateInfo.Image = ((ISporkImage)swapchainImages[i]).NativeImage;
            var imageView = _device.CreateImageView(imageViewCreateInfo);

            swapchainEntries[i] = new SporkSwapchainImageEntry(_device, imageView, swapchainImages[i]);
        }

        ((IInternalSporkSwapchain) swapchain).ImageEntries = swapchainEntries;
        
        return swapchain;
    }

    private unsafe SporkImage[] GetSwapChainImages(ISporkSwapchain swapchain)
    {
        uint imageCount = 0;
        _nativeExtension.GetSwapchainImages(_device.NativeDevice, swapchain.NativeSwapchain, &imageCount, null);
        var swapChainEntries = new SporkImage[imageCount];
        var swapchainImages = new Image[imageCount];
        fixed (Image* swapchainImagesPointer = swapchainImages)
        {
            //FIXME: This should be a method on SporkLogicalDevice

            _nativeExtension.GetSwapchainImages(_device.NativeDevice, swapchain.NativeSwapchain, &imageCount, swapchainImagesPointer);
        }

        for (var i = 0; i < imageCount; i++)
        {
            swapChainEntries[i] = new SporkImage(swapchainImages[i]);
        }

        return swapChainEntries;
    }

    public ISwapchainDefinition WithQueueFamily(uint queueFamilyIndex)
    {
        if (_queueFamilyIndices.Any(qfi => qfi == queueFamilyIndex)) return this;
        _queueFamilyIndices.Add(queueFamilyIndex);
        return this;
    }

    public ISwapchainDefinition WithSurfaceFormat(SurfaceFormatKHR surfaceFormat)
    {
        _swapchainCreateInfo.ImageFormat = surfaceFormat.Format;
        _swapchainCreateInfo.ImageColorSpace = surfaceFormat.ColorSpace;
        return this;
    }

    public ISwapchainDefinition WithImageExtent(Vector2D<int> extent)
    {
        var capabilities = _surface.GetPhysicalDeviceSurfaceCapabilities(_device.PhysicalDevice);
        _swapchainCreateInfo.ImageExtent = capabilities.CurrentExtent.Width != uint.MaxValue
            ? capabilities.CurrentExtent
            : new Extent2D(
                (uint)Math.Clamp(extent.X, capabilities.MinImageExtent.Width, capabilities.MaxImageExtent.Width),
                (uint)Math.Clamp(extent.Y, capabilities.MinImageExtent.Height, capabilities.MaxImageExtent.Height)
            );
        return this;
    }

    public ISwapchainDefinition WithPresentMode(PresentModeKHR presentMode)
    {
        _swapchainCreateInfo.PresentMode = presentMode;
        return this;
    }

    public ISwapchainDefinition WithImageCount(uint imageCount)
    {
        _requestedImageCount = imageCount;
        return this;
    }
}