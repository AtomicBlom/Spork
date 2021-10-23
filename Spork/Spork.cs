using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using Spork.LowLevel;

namespace Spork;

public class Spork : ISpork
{
    private readonly IWindow _window;
    private readonly Vk _vk;

    Vk ISpork.Vulkan => _vk;

    public Spork(IWindow window)
    {
        _window = window;
        _vk = Vk.GetApi();
    }

    public bool EnableValidationLayers { get; init; }
    public string EngineName { get; init; } = "Spork Vulkan Abstraction Layer";
    public string ApplicationName { get; init; } = "Unnamed Application";
    public IReadOnlyList<string[]> ValidationLayerNamesPriorityList { get; init; } = new List<string[]>()
    {
        new[] { "VK_LAYER_KHRONOS_validation" },
        new[] { "VK_LAYER_LUNARG_standard_validation" },
        new[]
        {
            "VK_LAYER_GOOGLE_threading",
            "VK_LAYER_LUNARG_parameter_validation",
            "VK_LAYER_LUNARG_object_tracker",
            "VK_LAYER_LUNARG_core_validation",
            "VK_LAYER_GOOGLE_unique_objects",
        }
    };

    private IReadOnlyList<string> MandatoryInstanceExtensions { get; init; } = new List<string>
    {
        ExtDebugUtils.ExtensionName,
    };

    private IReadOnlyList<string> DesiredInstanceExtensions { get; init; } = new List<string>
    {
        "VK_NV_external_memory_capabilities"
    };

    public string[]? ActiveValidationLayers { get; private set; }

    public unsafe SporkInstance CreateInstance()
    {
        Console.WriteLine("Initializing Vulkan");
        if (EnableValidationLayers)
        {
            ActiveValidationLayers = GetOptimalValidationLayers();
            if (ActiveValidationLayers is null)
            {
                throw new NotSupportedException("Validation Layers requested, but not available, You may need to install the Vulkan SDK");
            }
        }

        var appInfo = new ApplicationInfo(StructureType.ApplicationInfo)
        {
            PApplicationName = (byte*)Marshal.StringToHGlobalAnsi(ApplicationName),
            ApplicationVersion = new Version32(1, 0, 0),
            PEngineName = (byte*)Marshal.StringToHGlobalAnsi(EngineName),
            EngineVersion = new Version32(1, 0, 0),
            ApiVersion = Vk.Version11
        };

        var createInfo = new InstanceCreateInfo(StructureType.InstanceCreateInfo)
        {
            PApplicationInfo = &appInfo
        };

        var extensionList = MandatoryInstanceExtensions.Concat(DesiredInstanceExtensions.Where(di => _vk.IsInstanceExtensionPresent(di))).ToArray();

        var extensions = _window.VkSurface!.GetRequiredExtensions(out var extensionCount);
        //Combine into a new array
        var newExtensions = stackalloc byte*[(int)extensionCount + extensionList.Length];
        for (var i = 0; i < extensionCount; i++) newExtensions[i] = extensions[i];
        for (var i = 0; i < extensionList.Length; i++) newExtensions[extensionCount + i] = (byte*)SilkMarshal.StringToPtr(extensionList[i]);
        extensionCount += (uint)extensionList.Length;
        createInfo.EnabledExtensionCount = extensionCount;
        createInfo.PpEnabledExtensionNames = newExtensions;

        if (EnableValidationLayers && ActiveValidationLayers is not null)
        {
            createInfo.EnabledLayerCount = (uint)ActiveValidationLayers.Length;
            createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(ActiveValidationLayers);
        }

        if (_vk.CreateInstance(&createInfo, null, out var instance) != Result.Success)
        {
            throw new Exception("Failed to create instance of Vulkan");
        }

        _vk.CurrentInstance = instance;
        
        Marshal.FreeHGlobal((nint)appInfo.PApplicationName);
        Marshal.FreeHGlobal((nint)appInfo.PEngineName);

        if (EnableValidationLayers)
        {
            SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
        }

        return new SporkInstance(this, instance);
    }

    private unsafe string[]? GetOptimalValidationLayers()
    {
        var layerCount = 0u;
        _vk.EnumerateInstanceLayerProperties(&layerCount, (LayerProperties*)0);

        var availableLayers = new LayerProperties[layerCount];
        fixed (LayerProperties* availableLayersPointer = availableLayers)
            _vk.EnumerateInstanceLayerProperties(&layerCount, availableLayersPointer);

        var availableNames = availableLayers.Select(layer => Marshal.PtrToStringUTF8((nint)layer.LayerName)).ToArray();
        foreach (var validationLayerNameSet in ValidationLayerNamesPriorityList)
        {
            if (validationLayerNameSet.All(validationLayerName => availableNames.Contains(validationLayerName)))
            {
                return validationLayerNameSet;
            }
        }

        return null;
    }

}