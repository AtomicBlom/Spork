using System.Numerics;
using Silk.NET.Maths;
using Silk.NET.Vulkan.Extensions.EXT;
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

        UniformBufferObject ubo = new UniformBufferObject
        {
            Model = Matrix4x4.CreateFromAxisAngle(Vector3.UnitZ, (float)runTime * (MathF.PI / 180f) * 5),
            View = Matrix4x4.CreateLookAt(new Vector3(2f), Vector3.Zero, Vector3.UnitZ),
            Perspective = Matrix4x4.CreatePerspectiveFieldOfView(45 * (MathF.PI / 180f), _window.FramebufferSize.X / (float)_window.FramebufferSize.Y, 0.1f, 10.0f)
        };
        _vulkanServices.ViewMatrices = ubo;


    }

    private void WindowOnRender(double obj)
    {
        _vulkanServices.Render();
    }

    private void WindowOnClosing()
    {
        _instance.Dispose();
    }

    private void WindowOnFramebufferResize(Vector2D<int> obj)
    {
        //_vulkanServices.WaitForIdle();
        _vulkanServices.FramebufferResized = true;


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

        var surfaceKhronos = _instance.CreateSurface(_window);
        var physicalDevices = _instance.GetPhysicalDevices();

    }
}
