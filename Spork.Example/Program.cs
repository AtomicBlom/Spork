using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using Spork;
using Spork.Extensions;
using Spork.Extensions.Khronos.Surface;
using Spork.Extensions.Khronos.Swapchain;

try
{
    var game = new Game();
    game.InitializeVulkan();
    game.MainLoop();
}
catch (Exception e)
{
    Console.WriteLine(e);
    Console.ReadKey();
}

internal class Game
{
    private readonly IWindow _window;
    private readonly Spork.Spork _spork;

    public Game()
    {
        var options = WindowOptions.DefaultVulkan;
        options.Size = new(800, 600);
        options.Title = "Vulkan Test";
        options.IsEventDriven = false;

        _window = Window.Create(options);
        _window.Initialize();

        if (_window.VkSurface is null)
        {
            throw new NotSupportedException("Windowing Platform doesn't support Vulkan");
        }

        _window.FramebufferResize += WindowOnFramebufferResize;
        _window.Closing += WindowOnClosing;

        _spork = new Spork.Spork(_window) { EnableValidationLayers = true };
    }

    public void MainLoop()
    {
        _window.Render += WindowOnRender;
        _window.Update += WindowOnUpdate;
        _window.Run();
    }

    private double runTime = 0;
    private SporkInstance _instance;
    private DisposableSet _applicationScopeDisposables;
    private SwapchainDefinition _swapchainDefinition;
    private SelectedPhysicalDevice _physicalDevice;

    private void WindowOnUpdate(double time)
    {
        runTime += time;

        //UniformBufferObject ubo = new UniformBufferObject
        //{
        //    Model = Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, (float)runTime * (MathF.PI / 180f) * 5),
        //    View = Matrix4x4.CreateLookAt(new Vector3(2f), Vector3.Zero, Vector3.UnitZ),
        //    Perspective = Matrix4x4.CreatePerspectiveFieldOfView(45 * (MathF.PI / 180f), _window.FramebufferSize.X / (float)_window.FramebufferSize.Y, 0.1f, 10.0f)
        //};
        //_vulkanServices.ViewMatrices = ubo;


    }

    private void WindowOnRender(double obj)
    {
        //_vulkanServices.Render();
    }

    private void WindowOnClosing()
    {
        _instance.Dispose();
    }

    private void WindowOnFramebufferResize(Vector2D<int> obj)
    {
        //_vulkanServices.WaitForIdle();
        //_vulkanServices.FramebufferResized = true;


    }

    public void InitializeVulkan()
    {
        Queue graphicsQueue = default;
        Queue presentQueue = default;

        _applicationScopeDisposables = new DisposableSet();
        _instance = _applicationScopeDisposables.Add(_spork.CreateInstance());
        Console.WriteLine("Available Instance extensions");
        foreach (var extensionProperties in _instance.GetExtensions())
        {
            Console.WriteLine($"{extensionProperties.Name,-40} (v{extensionProperties.SpecVersion})");
        }

        if (_spork.EnableValidationLayers && _instance.TryGetExtension<ConsoleDebugExtension, ExtDebugUtils>(out var consoleDebugUtils))
        {
            _applicationScopeDisposables.Add(consoleDebugUtils);
            consoleDebugUtils.SetupMessenger();
        }

        if (!_instance.TryGetExtension<SporkKhronosSurfaceExtension, KhrSurface>(out var khronosSurfaceExtension))
        {
            throw new NotSupportedException("Unable to find the KHR_surface Extension");
        }

        var requiredExtensions = new[]
        {
            KhrSwapchain.ExtensionName
        };

        var surface = _applicationScopeDisposables.Add(khronosSurfaceExtension.CreateSurface(_window));
        
        _physicalDevice = GetValidPhysicalDevices(surface, requiredExtensions)
            .FirstOrDefault() ?? throw new Exception("Unable to find a valid device that can be used to render graphics and present");
        
        var device = _physicalDevice.PhysicalDevice.DefineLogicalDevice()
            .WithQueue(_physicalDevice.GraphicsIndex.Index, createdQueue => graphicsQueue = createdQueue)
            .WithQueue(_physicalDevice.PresentIndex.Index, createdQueue => presentQueue = createdQueue)
            .WithValidationLayers(_spork.ActiveValidationLayers)
            .WithDeviceExtensions(requiredExtensions)
            .Create();
        _applicationScopeDisposables.Add(device);

        var definition = DefineSwapChain(device, surface);

        if (!TryCreateSwapChain(definition, out var swapchain))
        {
            throw new Exception("Failed to create the first swapchain");
        }

        var colourAttachment = new SporkColorAttachment()
        {
            Format = swapchain.ImageFormat,

        };
        var subpass = new SporkSubpass(PipelineBindPoint.Graphics)
            .WithColorAttachment(colourAttachment)
            .AfterExternal();

        var renderPassDefinition = device.DefineRenderPass()
            .WithSubpass(subpass);

    }

    private ISwapchainDefinition DefineSwapChain(SporkLogicalDevice device, SporkSurface sporkSurface)
    {
        if (!device.TryGetExtension<SporkKhronosSwapchainExtension, KhrSwapchain>(out var khronosSwapchainExtension))
        {
            throw new NotSupportedException("Unable to find the KHR_swapchain Extension");
        }

        var surfaceFormat = _physicalDevice.SurfaceFormats.FirstOrDefault(format => format.Format == Format.B8G8R8A8Unorm && format.ColorSpace == ColorSpaceKHR.ColorSpaceSrgbNonlinearKhr, _physicalDevice.SurfaceFormats[0]);
        var presentMode = _physicalDevice.SurfacePresentModes.FirstOrDefault(mode => mode == PresentModeKHR.PresentModeMailboxKhr, PresentModeKHR.PresentModeFifoKhr);

        return khronosSwapchainExtension.DefineSwapchain(sporkSurface)
            .WithQueueFamily(_physicalDevice.GraphicsIndex.Index)
            .WithQueueFamily(_physicalDevice.PresentIndex.Index)
            .WithSurfaceFormat(surfaceFormat)
            .WithPresentMode(presentMode);
    }

    private bool TryCreateSwapChain(ISwapchainDefinition definition, out SporkSwapchain swapchain)
    {
        definition.WithImageExtent(_window.FramebufferSize);
        if (!definition.CanCreate)
        {
            swapchain = null!;
            return false;
        }

        var swapchainDisposableSet = _applicationScopeDisposables.Add(new DisposableSet());
        swapchain = _swapchainDefinition.Create(swapchainDisposableSet);
        
        return true;
    }

    private IEnumerable<SelectedPhysicalDevice> GetValidPhysicalDevices(SporkSurface surface, string[] requiredExtensions)
    {
        return from physicalDevice in _instance.GetPhysicalDevices(requiredExtensions)
            let queueFamilies = physicalDevice.GetPhysicalDeviceQueueFamilyProperties()
            let graphicsQueueFamily = queueFamilies.FirstOrDefault(p => p.Properties.QueueFlags.HasFlag(QueueFlags.QueueGraphicsBit))
            let presentQueueFamily = queueFamilies.FirstOrDefault(queueFamilyProperties => surface.DoesQueueSupportPresentation(physicalDevice, queueFamilyProperties.Index))
            where presentQueueFamily != null && graphicsQueueFamily != null
            let surfaceFormats = surface.GetPhysicalDeviceSurfaceFormats(physicalDevice)
            let surfacePresentModes = surface.GetPhysicalDeviceSurfacePresentModes(physicalDevice)
            where surfaceFormats.Any() && surfacePresentModes.Any()
            select new SelectedPhysicalDevice(physicalDevice, graphicsQueueFamily, presentQueueFamily, surfaceFormats, surfacePresentModes);
    }
}

public record SelectedPhysicalDevice(SporkPhysicalDevice PhysicalDevice,
    PhysicalDeviceQueueFamilyProperties GraphicsIndex,
    PhysicalDeviceQueueFamilyProperties PresentIndex,
    SurfaceFormatKHR[] SurfaceFormats,
    PresentModeKHR[] SurfacePresentModes);