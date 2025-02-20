namespace FileMutator.Tools.Interfaces
{
    public interface IMutatorService
    {
        string MutateText(string text);
        byte[] MutateText(byte[] data);
    }
}
