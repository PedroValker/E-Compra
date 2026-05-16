using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq; // Garante que o FirstOrDefault() funcione aqui
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

            // 🔥 CORREÇÃO: Passamos apenas a String que a ViewModel espera (ex: Nome)
            // Se a sua ViewModel buscar os pedidos pelo Email, mude para: Sessao.UsuarioLogado.Email
            if (Sessao.UsuarioLogado != null)
            {
                DataContext = new PedidosViewModel(Sessao.UsuarioLogado.Nome);
            }
        }

        private void AbaHistorico_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is PedidosViewModel vm)
            {
                TabelaDePedidos.ItemsSource = vm.ListaPedidosEntregues;

                // Seleciona automaticamente o primeiro item da lista nova
                vm.PedidoSelecionado = vm.ListaPedidosEntregues.FirstOrDefault();
            }

            AbaHistoricoTexto.FontWeight = System.Windows.FontWeights.Bold;
            AbaHistoricoTexto.Foreground = new SolidColorBrush(Colors.Black);

            AbaPendentesTexto.FontWeight = System.Windows.FontWeights.Normal;
            AbaPendentesTexto.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void AbaPendentes_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is PedidosViewModel vm)
            {
                TabelaDePedidos.ItemsSource = vm.ListaPedidosPendentes;

                // Seleciona automaticamente o primeiro item da lista nova
                vm.PedidoSelecionado = vm.ListaPedidosPendentes.FirstOrDefault();
            }

            AbaPendentesTexto.FontWeight = System.Windows.FontWeights.Bold;
            AbaPendentesTexto.Foreground = new SolidColorBrush(Colors.Black);

            AbaHistoricoTexto.FontWeight = System.Windows.FontWeights.Normal;
            AbaHistoricoTexto.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void VerDetalhes_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Pegamos o botão que foi clicado e extraímos o Pedido que está associado a ele
            if (sender is Button botao && botao.DataContext is Pedido pedidoClicado)
            {
                // Instancia a sua janela de detalhes passando o pedido específico
                var janelaDetalhes = new DetalhesPedidoCliente(pedidoClicado);

                // Abre a janela como um "Pop-up"
                janelaDetalhes.ShowDialog();
            }
        }
    }
}