namespace Spork;

public interface ISporkInstanceExtension<in TNativeExtension>
{
    TNativeExtension NativeExtension { init; }
    SporkInstance Instance { init; }
}