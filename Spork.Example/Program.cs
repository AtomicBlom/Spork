using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using Spork;

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
        _applicationScopeDisposables = new DisposableSet();
        _instance = _applicationScopeDisposables.Add(_spork.CreateInstance());
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
        
        var validPhysicalDevices = GetValidPhysicalDevices(surface, requiredExtensions);

        var physicalDevice = validPhysicalDevices.FirstOrDefault() ?? throw new Exception("Unable to find a valid device that can be used to render graphics and present");
        Queue graphicsQueue = default;
        Queue presentQueue = default;
        var device = physicalDevice.PhysicalDevice.DefineLogicalDevice()
            .WithQueue(physicalDevice.GraphicsIndex.Index, createdQueue => graphicsQueue = createdQueue)
            .WithQueue(physicalDevice.PresentIndex.Index, createdQueue => presentQueue = createdQueue)
            .WithValidationLayers(_spork.ActiveValidationLayers)
            .WithDeviceExtensions(requiredExtensions)
            .Create();
        _applicationScopeDisposables.Add(device);
    }

    private IEnumerable<SelectedPhysicalDevice> GetValidPhysicalDevices(SporkSurface surface, string[] requiredExtensions)
    {
        return from physicalDevice in _instance.GetPhysicalDevices(requiredExtensions)
            let queueFamilies = physicalDevice.GetPhysicalDeviceQueueFamilyProperties()
            let graphicsQueueFamily = queueFamilies.FirstOrDefault(p => p.Properties.QueueFlags.HasFlag(QueueFlags.QueueGraphicsBit))
            let presentQueueFamily = queueFamilies.FirstOrDefault(queueFamilyProperties => surface.DoesQueueSupportPresentation(physicalDevice, queueFamilyProperties.Index))
            where presentQueueFamily != null && graphicsQueueFamily != null
            let surfaceCapabilities = surface.GetPhysicalDeviceSurfaceCapabilities(physicalDevice)
            let surfaceFormats = surface.GetPhysicalDeviceSurfaceFormats(physicalDevice)
            let surfacePresentModes = surface.GetPhysicalDeviceSurfacePresentModes(physicalDevice)
            where surfaceFormats.Any() && surfacePresentModes.Any()
            select new SelectedPhysicalDevice(physicalDevice, graphicsQueueFamily, presentQueueFamily);
    }
}

public record SelectedPhysicalDevice(
    SporkPhysicalDevice PhysicalDevice, 
    PhysicalDeviceQueueFamilyProperties GraphicsIndex, 
    PhysicalDeviceQueueFamilyProperties PresentIndex)
{
    

    public uint[] UniqueQueueIndices => GraphicsIndex.Index == PresentIndex.Index 
        ? new[] { GraphicsIndex.Index }
        : new[] { GraphicsIndex.Index, PresentIndex.Index };
}
