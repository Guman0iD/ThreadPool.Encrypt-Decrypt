namespace ThreadPool.Encrypt_Decrypt;

public class EncryptDecrypt(string filePath, int shift)
{
    public string FilePath { get; } = filePath;
    public int Shift { get; } = shift;
}