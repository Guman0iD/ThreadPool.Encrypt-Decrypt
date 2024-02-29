using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;

namespace ThreadPool.Encrypt_Decrypt;

public partial class MainWindow : Window
{
    private DispatcherTimer _timer;
    public event EventHandler<int> EncryptionProgressChanged;

    public MainWindow()
    {
        InitializeComponent();
        StartTimer();
        _timer = new DispatcherTimer();
        EncryptionProgressChanged += HandleEncryptionProgressChanged;
    }

    private void HandleEncryptionProgressChanged(object? sender, int progress)
    {
        Dispatcher.Invoke(() => ProgressBar.Value = progress);
    }

    private void StartTimer()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(5)
        };
        _timer.Tick += Timer_Tick!;
        _timer.Start();
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        Dispatcher.Invoke(() => {
            NotificationBox.Text = null;
            _timer.Stop();
        });
    }

    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog fileDialog = new OpenFileDialog
        {
            Multiselect = true
        };

        if (fileDialog.ShowDialog() == true)
        {
            foreach (var fileName in fileDialog.FileNames)
            {
                FileList.Items.Add(fileName);
            }
        }
    }

    private void EncryptButton_Click(object sender, RoutedEventArgs e)
    {
        if (FileList.Items.Count == 0)
        {
            NotificationBox.Text = "Select at least one file to encrypt.";
            StartTimer();
            return;
        }

        if (string.IsNullOrEmpty(KeyTextBox.Text))
        {
            NotificationBox.Text = "Enter the encryption key.";
            StartTimer();
            return;
        }

        int shift;
        if (!int.TryParse(KeyTextBox.Text, out shift))
        {
            NotificationBox.Text = "Enter the correct encryption key!";
            StartTimer();
            return;
        }

        foreach (string filePath in FileList.Items)
        {
            System.Threading.ThreadPool.QueueUserWorkItem(EncryptFile!, new EncryptDecrypt(filePath, shift));
        }

        NotificationBox.Text = "Encryption initiated.";
        StartTimer();
    }

    private void EncryptFile(object state)
    {
        var encryptParams = (EncryptDecrypt)state;
        var filePath = encryptParams.FilePath;
        var shift = encryptParams.Shift;

        try
        {
            var encryptedFilePath = Path.Combine(Path.GetDirectoryName(filePath)!,
                Path.GetFileNameWithoutExtension(filePath) + ".enx");
            
            var fileName = Path.GetFileName(filePath);

            using var streamReader = new StreamReader(filePath);
            using var sw = new StreamWriter(encryptedFilePath);

            var totalBytes = streamReader.BaseStream.Length;
            long bytesRead = 0;

            while (!streamReader.EndOfStream)
            {
                var charValue = streamReader.Read();
                var encryptedChar = CaesarCipher(((char)charValue).ToString(), shift);
                sw.Write(encryptedChar);

                bytesRead++;
                var progressPercentage = (int)((bytesRead * 100) / totalBytes);
                
                Dispatcher.Invoke(() => EncryptionProgressChanged?.Invoke(this, progressPercentage));
            }
            
            Dispatcher.Invoke(() =>
            {
                NotificationBox.Text = $"Encryption finished for {fileName}!";
                StartTimer();
            });
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() =>
            {
                NotificationBox.Text = $"Error during file encryption {filePath}: {ex.Message}";
            });
        }
    }

    private string CaesarCipher(string input, int shift)
    {
        var result = new StringBuilder();

        foreach (var ch in input)
        {
            if (char.IsLetter(ch))
            {
                var offset = char.IsUpper(ch) ? 'A' : 'a';
                result.Append((char)((ch + shift - offset) % 26 + offset));
            }
            else
            {
                result.Append(ch);
            }
        }

        return result.ToString();
    }

    private void Remove(object sender, RoutedEventArgs e)
    {
        var selectedItems = new List<object>();
        foreach (var selectedItem in FileList.SelectedItems)
        {
            selectedItems.Add(selectedItem);
        }

        foreach (var selectedItem in selectedItems)
        {
            FileList.Items.Remove(selectedItem);
        }

        FileList.Items.Refresh();
    }
}