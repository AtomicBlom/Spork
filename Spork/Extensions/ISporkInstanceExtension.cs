using Spork.LowLevel;

namespace Spork.Extensions;

public interface ISporkInstanceExtension<in TNativeExtension>
{
    TNativeExtension NativeExtension { init; }
    ISporkInstance Instance { init; }
}

public interface ISporkDeviceExtension<in TNativeExtension>
{
    TNativeExtension NativeExtension { init; }
    ISporkLogicalDevice Device { init; }
}