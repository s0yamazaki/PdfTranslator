using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PdfTranslator;
using System.ComponentModel;
using MahApps.Metro.Controls;
namespace WpfPdfTranslator
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        string pdfName;
        

        /// <summary>
        /// 処理中イメージの表示／非表示切り替え
        /// 
        /// </summary>
        private void ToggleProgressRing()
        {
            if (this.loading_image.IsActive)
            {
                this.IsEnabled = true;
                this.loading_image.IsActive = false;
                this.rec_overlay.Visibility = Visibility.Collapsed;
            }
            else
            {
                this.IsEnabled = false;
                this.loading_image.IsActive = true;
                this.rec_overlay.Visibility = Visibility.Visible;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void OutputSelect_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.Title = "出力先を選択してください";
            dialog.Filter = "PDFファイル(*.pdf)|*.pdf";
            if (dialog.ShowDialog() == true)
            {
                ToggleProgressRing();
                await Util.Convert(pdfName, dialog.FileName);
                ToggleProgressRing();
            }
        }

        private void PdfSelect_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "出力先を選択してください";
            dialog.Filter = "PDFファイル(*.pdf)|*.pdf";

            if (dialog.ShowDialog() == true)
            {
                pdfName = dialog.FileName;
                outputSelect.IsEnabled = true;
            }
            else
            {
                outputSelect.IsEnabled = false;
            }
        }
    }
}
