using System.Windows.Controls;
using Teste.Model;
using Teste.ViewModel;

namespace Teste.View
{
    public partial class PedidosView : UserControl
    {
        public PedidosView()
        {
            // 1º: Sempre chame o InitializeComponent primeiro!
            InitializeComponent();

            // 2º: Instancie o VIEWMODEL (e passe o nome do cliente logado, 
            // como configuramos no código anterior)
            DataContext = new PedidosViewModel(Sessao.UsuarioLogado);
        }
    }
}