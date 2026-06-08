using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Teste.View
{
    public partial class ContatoNovo : UserControl
    {
        public ContatoNovo()
        {
            InitializeComponent();
        }

        private void BtnContato_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://wa.me/5512988202526";

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }
}