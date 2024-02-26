namespace ThreadPool.Encrypt_Decrypt;

public class ProgressParams(string originalText, string encryptedText)
{
    public string OriginalText { get; } = originalText;
    public string EncryptedText { get; } = encryptedText;
}