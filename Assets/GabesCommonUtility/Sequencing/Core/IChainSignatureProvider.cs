namespace Sequencing.Core
{
    /*
     * Optional. A step can implement this to contribute a richer identity token to
     * the chain signature the server validates against, e.g. folding in scene names
     * so a mismatch is detected even when two chains share the same step order and
     * types but differ in configuration. Steps that do not implement it fall back to
     * their type's full name.
     */
    public interface IChainSignatureProvider
    {
        string SignatureToken { get; }
    }
}
