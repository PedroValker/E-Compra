using System.Windows.Controls;
using Teste.ViewModel;

namespace Teste.View
{
    public partial class ClientesView : UserControl
    {
        public ClientesView()
        {
            InitializeComponent();

            DataContext = new ClientesViewModel();
        }
    }
}