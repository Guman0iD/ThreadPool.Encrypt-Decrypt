using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Windows;

namespace ThreadPool.Encrypt_Decrypt;

public partial class FIleContent : Window
{
    public FIleContent(string originalText, string encryptedText)
    {
        InitializeComponent();
        OriginalTextBox.Text = originalText;

        var totalLength = originalText.Length;
        var progress = 0;
        var step = totalLength / 100;

        EncryptedTextBox.Text = encryptedText;

        for (int i = 0; i < totalLength; i++)
        {
            if (i % step == 0)
            {
                progress++;
                ProgressBar.Value = progress;
              //  Thread.Sleep(100); //
            }
        }

        ProgressBar.Value = 100;
    }
}