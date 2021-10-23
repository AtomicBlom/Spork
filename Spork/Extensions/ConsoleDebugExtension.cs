using System.Runtime.InteropServices;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Spork.LowLevel;

namespace Spork.Extensions;

public sealed class ConsoleDebugExtension : ISporkInstanceExtension<ExtDebugUtils>, IDisposable
{
    private readonly ExtDebugUtils _nativeExtension = null!;
    private readonly Instance _instance;
    private bool _active = false;
    private DebugUtilsMessengerEXT _debugMessenger;

    ExtDebugUtils ISporkInstanceExtension<ExtDebugUtils>.NativeExtension
    {
        init => _nativeExtension = value;
    }
    ISporkInstance ISporkInstanceExtension<ExtDebugUtils>.Instance
    {
        init => _instance = value.NativeInstance;
    }

    public unsafe void SetupMessenger()
    {
        var createInfo = new DebugUtilsMessengerCreateInfoEXT(StructureType.DebugUtilsMessengerCreateInfoExt)
        {
            MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityVerboseBitExt |
                              DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityWarningBitExt |
                              DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityErrorBitExt,
            MessageType = DebugUtilsMessageTypeFlagsEXT.DebugUtilsMessageTypeGeneralBitExt |
                          DebugUtilsMessageTypeFlagsEXT.DebugUtilsMessageTypePerformanceBitExt |
                          DebugUtilsMessageTypeFlagsEXT.DebugUtilsMessageTypeValidationBitExt,
            PfnUserCallback = (DebugUtilsMessengerCallbackFunctionEXT)DebugCallback
        };
        
        if (_nativeExtension.CreateDebugUtilsMessenger(_instance, &createInfo, null, out var debugMessenger) != Result.Success)
        {
            throw new Exception("Failed to create debug messenger");
        }

        _debugMessenger = debugMessenger;
        _active = true;
    }

    private unsafe uint DebugCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity,
        DebugUtilsMessageTypeFlagsEXT messageTypes,
        DebugUtilsMessengerCallbackDataEXT* pCallbackData,
        void* pUserData)
    {
        if (messageSeverity > DebugUtilsMessageSeverityFlagsEXT.DebugUtilsMessageSeverityVerboseBitExt)
        {
            Console.WriteLine
                ($"{messageSeverity} {messageTypes}" + Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage));

        }

        return Vk.False;
    }

    private unsafe void ReleaseUnmanagedResources()
    {
        if (_active)
        {
            _nativeExtension.DestroyDebugUtilsMessenger(_instance, _debugMessenger, null);
        }
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~ConsoleDebugExtension()
    {
        ReleaseUnmanagedResources();
    }
}