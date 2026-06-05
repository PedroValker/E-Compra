using System.Windows.Controls;
using Teste.ViewModel;

namespace Teste.View
{
    public partial class PendenciasView : UserControl
    {
        public PendenciasView()
        {
            InitializeComponent();
            DataContext = new PendenciasViewModel();
        }
    }
    
}