using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace SP03HomeW0rk
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CancellationTokenSource Cts = new();
        CancellationTokenSource CancelCts = new();
        public OpenFileDialog FileDialog { get; set; } = new();
        public string FileTextBackUp { get; set; }
        public string FileText { get; set; }
        public string FilePath { get; set; }
        public int Password { get; set; }

        public int CancelledIndex { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Open_Filedialog_Button(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (FileDialog.ShowDialog() == true)
                {
                    File_Name_TB.Text = FileDialog.FileName;
                }
            });

        }

        private void EncryptText(CancellationToken ct)
        {

            string FilesText = FileTextBackUp;
            int key = Password;

            char[] charText = FilesText.ToCharArray();
            StringBuilder sb = new StringBuilder(FilesText);
            for (int i = CancelledIndex; i < FilesText.Length; i++)
            {
                if (!ct.IsCancellationRequested)
                {

                    charText[i] ^= (char)key;
                    sb[i] = charText[i];
                    FileText = sb.ToString();
                    File.WriteAllText(FilePath, FileText);
                    CancelledIndex = i;
                    Dispatcher.Invoke(() => { Encrypt_Progress_Bar.Value = i; });
                    Thread.Sleep(1000);
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        Cancel_Button.IsEnabled = false;
                        Start_Button.IsEnabled = true;
                        
                    });
                    return;
                }
            }
            Dispatcher.Invoke(() =>
            {
                Cancel_Button.IsEnabled = false;
                Start_Button.IsEnabled = true;
                CancelledIndex = 0;
            });
        }

        private void CancelledEncryptText(CancellationToken ct2)
        {
            Dispatcher.Invoke(() =>
            {
                Cts.Cancel();
                Cts = new();
                Cancel_Button.IsEnabled = false;
                Start_Button.IsEnabled = true;

            });
            string FilesText = FileText;
            int key = Password;

            char[] charText = FilesText.ToCharArray();
            StringBuilder sb = new StringBuilder(FilesText);
            for (int i = CancelledIndex; i >= 0; i--)
            {
                if (!ct2.IsCancellationRequested)
                {

                    charText[i] ^= (char)key;
                    sb[i] = charText[i];
                    FileText = sb.ToString();
                    Dispatcher.Invoke(() => { Encrypt_Progress_Bar.Value = i; });
                    File.WriteAllText(FilePath, FileText);
                    CancelledIndex = i;
                    Thread.Sleep(1000);
                }
                else return;
            }
        }

        private void Start_Encrypt(object sender, RoutedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(Configuring);
                CancelCts.Cancel();
                Start_Button.IsEnabled = false;
                Cancel_Button.IsEnabled = true;
                CancelCts = new();
                ThreadPool.QueueUserWorkItem(_ => { EncryptText(Cts.Token); });

            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }
        private void Configuring()
        {
            Dispatcher.Invoke(() =>
            {
               
                

                    if (File_Name_TB.Text == "") throw new Exception("File Path did not found");
                    FilePath = File_Name_TB.Text;
                    FileText = File.ReadAllText(File_Name_TB.Text);

                    if (FileText == null || FileText == "") throw new Exception("Text did not found in this file");
                    FileTextBackUp = FileText;
                    Encrypt_Progress_Bar.Maximum = FileText.Length - 1;

                    if (Password_TB.Text == null || Password_TB.Text == "") throw new Exception("Password did not found");
                    Password = int.Parse(Password_TB.Text);
               
            });
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {

            ThreadPool.QueueUserWorkItem(_ => CancelledEncryptText(CancelCts.Token));
        }
    }

}

