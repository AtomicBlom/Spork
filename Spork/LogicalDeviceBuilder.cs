using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Spork;

public class LogicalDeviceBuilder : ILogicalDeviceBuilder
{
    private readonly Vk _vk;
    private readonly PhysicalDevice _physicalDevice;
    private readonly List<(uint physicalDeviceGraphicsIndex, Action<Queue> createdQueue)> _queuesDefined = new();
    private readonly List<DeviceQueueCreateInfo> _deviceCreateInfos = new();
    private readonly DisposableSet _disposableMemory = new ();

    private string[] _validationLayers = Array.Empty<string>();
    private string[] _deviceExtensions = Array.Empty<string>();

    

    public LogicalDeviceBuilder(Vk vk, PhysicalDevice physicalDevice)
    {
        _vk = vk;
        _physicalDevice = physicalDevice;
    }

    public unsafe ILogicalDeviceBuilder WithQueue(uint queueIndex, Action<Queue> createdQueue, float priority = 1f)
    {
        _queuesDefined.Add((queueIndex, createdQueue));
        if (_deviceCreateInfos.Any(dci => dci.QueueFamilyIndex == queueIndex)) return this;

        var mem = GlobalMemory.Allocate(sizeof(float));
        _disposableMemory.Add(mem);
        var priorityPointer = (float*)Unsafe.AsPointer(ref mem.GetPinnableReference());
        *priorityPointer = priority;

        DeviceQueueCreateInfo queueCreateInfo = new(StructureType.DeviceQueueCreateInfo)
        {
            QueueFamilyIndex = queueIndex,
            QueueCount = 1,
            PQueuePriorities = priorityPointer
        };
        _deviceCreateInfos.Add(queueCreateInfo);

        return this;
    }

    public ILogicalDeviceBuilder WithValidationLayers(params string[]? validationLayers)
    {
        _validationLayers = validationLayers ?? Array.Empty<string>();
        return this;
    }

    public ILogicalDeviceBuilder WithDeviceExtensions(params string[]? requiredExtensions)
    {
        _deviceExtensions = requiredExtensions ?? Array.Empty<string>();
        return this;
    }

    public unsafe SporkLogicalDevice Create()
    {
        var mem = GlobalMemory.Allocate(_deviceCreateInfos.Count * sizeof(DeviceQueueCreateInfo));
        _disposableMemory.Add(mem);
        var deviceCreateInfos = (DeviceQueueCreateInfo*)Unsafe.AsPointer(ref mem.GetPinnableReference());

        for (var i = 0; i < _deviceCreateInfos.Count; i++)
        {
            deviceCreateInfos[i] = _deviceCreateInfos[i];
        }
        
        PhysicalDeviceFeatures deviceFeatures = new();

        DeviceCreateInfo deviceCreateInfo = new(StructureType.DeviceCreateInfo)
        {
            QueueCreateInfoCount = (uint)_deviceCreateInfos.Count,
            PQueueCreateInfos = deviceCreateInfos,
            PEnabledFeatures = &deviceFeatures,
            EnabledExtensionCount = (uint)_deviceExtensions.Length,
            PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(_deviceExtensions)
        };

        if (_validationLayers.Length > 0)
        {
            deviceCreateInfo.EnabledLayerCount = (uint)_validationLayers.Length;
            deviceCreateInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(_validationLayers);
        }
        else
        {
            deviceCreateInfo.EnabledLayerCount = 0;
        }
        
        if (_vk.CreateDevice(_physicalDevice, &deviceCreateInfo, null, out var device) != Result.Success)
        {
            throw new Exception("Failed to create logical device");
        }

        _vk.CurrentDevice = device;

        foreach (var (index, createdQueueAction) in _queuesDefined)
        {
            _vk.GetDeviceQueue(device, index, 0, out var queue);
            createdQueueAction(queue);
        }

        return new SporkLogicalDevice(_vk, _physicalDevice, device);
    }

    ~LogicalDeviceBuilder()
    {
        _disposableMemory.Dispose();
    }
}