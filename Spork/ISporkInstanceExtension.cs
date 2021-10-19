namespace Spork;

public interface ISporkInstanceExtension<in TNativeExtension>
{
    TNativeExtension NativeExtension { init; }
    IInternalSporkInstance Instance { init; }
}