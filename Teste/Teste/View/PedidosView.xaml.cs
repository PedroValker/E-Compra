using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using Teste;
using Teste.Model;
using Teste.ViewModel;

namespace Teste.View
{
    public partial class PedidosView : UserControl
    {
        public PedidosView()
        {
            InitializeComponent();

            // 🚀 VÍNCULO POR ID: Envia o ID numérico único do usuário logado para a ViewModel de forma isolada
            if (Sessao.UsuarioLogado != null)
            {
                DataContext = new PedidosViewModel(Sessao.UsuarioLogado.Id);
            }
        }

        private void AbaHistorico_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is PedidosViewModel vm)
            {
                // Alimenta a tabela com a lista que a ViewModel já buscou e filtrou por ID
                TabelaDePedidos.ItemsSource = vm.ListaPedidosEntregues;

                // Seleciona automaticamente o primeiro item do histórico se houver algum
                vm.PedidoSelecionado = vm.ListaPedidosEntregues?.FirstOrDefault();
            }

            // Feedback visual da aba selecionada (Histórico ativa)
            AbaHistoricoTexto.FontWeight = System.Windows.FontWeights.Bold;
            AbaHistoricoTexto.Foreground = new SolidColorBrush(Colors.Black);

            AbaPendentesTexto.FontWeight = System.Windows.FontWeights.Normal;
            AbaPendentesTexto.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void AbaPendentes_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is PedidosViewModel vm)
            {
                // Alimenta a tabela com a lista de pendentes filtrada por ID na ViewModel
                TabelaDePedidos.ItemsSource = vm.ListaPedidosPendentes;

                // Seleciona automaticamente o primeiro item dos pendentes se houver algum
                vm.PedidoSelecionado = vm.ListaPedidosPendentes?.FirstOrDefault();
            }

            // Feedback visual da aba selecionada (Pendentes ativa)
            AbaPendentesTexto.FontWeight = System.Windows.FontWeights.Bold;
            AbaPendentesTexto.Foreground = new SolidColorBrush(Colors.Black);

            AbaHistoricoTexto.FontWeight = System.Windows.FontWeights.Normal;
            AbaHistoricoTexto.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void VerDetalhes_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Extrai de forma segura o pedido amarrado à linha clicada na tabela
            if (sender is Button botao && botao.DataContext is Pedido pedidoClicado)
            {
                var janelaDetalhes = new DetalhesPedidoCliente(pedidoClicado);
                janelaDetalhes.ShowDialog();
            }
        }
    }
}