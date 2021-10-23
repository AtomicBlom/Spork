using System.Runtime.CompilerServices;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Spork.LowLevel;

namespace Spork;

public class SporkPhysicalDevice : ISporkPhysicalDevice
{
    private readonly Vk _vk;
    private readonly ISporkInstance _instance;
    private readonly PhysicalDevice _physicalDevice;

    PhysicalDevice ISporkPhysicalDevice.VulkanPhysicalDevice => _physicalDevice;

    ISporkInstance ISporkPhysicalDevice.Instance => _instance;

    public SporkPhysicalDevice(Vk vk, ISporkInstance instance, PhysicalDevice physicalDevice)
    {
        _vk = vk;
        _instance = instance;
        _physicalDevice = physicalDevice;
    }

    public bool HasExtension(string extension)
    {
        return _vk.IsDeviceExtensionPresent(_physicalDevice, extension);
    }

    public unsafe IEnumerable<PhysicalDeviceQueueFamilyProperties> GetPhysicalDeviceQueueFamilyProperties()
    {
        uint queryFamilyCount = 0;

        _vk.GetPhysicalDeviceQueueFamilyProperties(_physicalDevice, &queryFamilyCount, null);
        using var mem = GlobalMemory.Allocate((int)queryFamilyCount * sizeof(QueueFamilyProperties));
        var queueFamiliesPointer = (QueueFamilyProperties*)Unsafe.AsPointer(ref mem.GetPinnableReference());
        _vk.GetPhysicalDeviceQueueFamilyProperties(_physicalDevice, &queryFamilyCount, queueFamiliesPointer);

        var queueFamilies = new PhysicalDeviceQueueFamilyProperties[queryFamilyCount];

        for (uint i = 0; i < queryFamilyCount; i++)
        {
            queueFamilies[i] = new(i, queueFamiliesPointer[i]);
        }

        return queueFamilies;
    }

    public ILogicalDeviceBuilder DefineLogicalDevice()
    {
        return new LogicalDeviceBuilder(_vk, this);
    }
}