using System.Windows.Controls;
using Teste.ViewModel;

namespace Teste.View
{
    public partial class PedidosView : UserControl
    {
        public PedidosView()
        {
            DataContext = new PedidosViewModel();
        }
    }
}