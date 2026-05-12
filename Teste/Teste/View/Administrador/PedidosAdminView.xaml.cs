using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Teste.Model;
using Teste.Repository;

namespace Teste.View
{
    public partial class PedidosAdminView : UserControl
    {
        // 1. Criamos as duas listas separadas para alimentar as abas
        public ObservableCollection<Pedido> ListaPedidosPendentes { get; set; } = new ObservableCollection<Pedido>();
        public ObservableCollection<Pedido> ListaPedidosEntregues { get; set; } = new ObservableCollection<Pedido>();

        public PedidosAdminView()
        {
            InitializeComponent();
            CarregarPedidos();

            // 2. Definimos que a própria tela fornece os dados para o XAML
            this.DataContext = this;
        }

        private void CarregarPedidos()
        {
            // Puxa os dados do arquivo TXT
            PedidoRepository repo = new PedidoRepository();
            repo.CarregarDoArquivo();

            ListaPedidosPendentes.Clear();
            ListaPedidosEntregues.Clear();

            // 3. Separa os pedidos em suas respectivas listas
            foreach (var pedido in MemoriaPedidos.Lista)
            {
                if (pedido.Status != null && pedido.Status.Trim().Equals("Entregue", System.StringComparison.OrdinalIgnoreCase))
                {
                    ListaPedidosEntregues.Add(pedido);
                }
                else
                {
                    ListaPedidosPendentes.Add(pedido);
                }
            }

            // Define a tabela inicial como pendentes
            GridPedidos.ItemsSource = ListaPedidosPendentes;
        }

        private void VerItens_Click(object sender, RoutedEventArgs e)
        {
            // Descobre qual pedido o admin clicou
            if (sender is Button botao && botao.DataContext is Pedido pedidoClicado)
            {
                // Abre a janela de detalhes (Certifique-se de que o nome da sua janela modal é esse mesmo)
                DetalhesPedidoCliente modal = new DetalhesPedidoCliente(pedidoClicado);
                modal.ShowDialog();
            }
        }

        private void AbaPendentes_Click(object sender, MouseButtonEventArgs e)
        {
            // Troca os dados da tabela
            GridPedidos.ItemsSource = ListaPedidosPendentes;

            // Altera visual: Aba Pendentes ativa (Preto Negrito)
            AbaPendentesTexto.FontWeight = FontWeights.Bold;
            AbaPendentesTexto.Foreground = new SolidColorBrush(Colors.Black);

            // Altera visual: Aba Histórico inativa (Cinza Normal)
            AbaHistoricoTexto.FontWeight = FontWeights.Normal;
            AbaHistoricoTexto.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void AbaHistorico_Click(object sender, MouseButtonEventArgs e)
        {
            // Troca os dados da tabela
            GridPedidos.ItemsSource = ListaPedidosEntregues;

            // Altera visual: Aba Histórico ativa (Preto Negrito)
            AbaHistoricoTexto.FontWeight = FontWeights.Bold;
            AbaHistoricoTexto.Foreground = new SolidColorBrush(Colors.Black);

            // Altera visual: Aba Pendentes inativa (Cinza Normal)
            AbaPendentesTexto.FontWeight = FontWeights.Normal;
            AbaPendentesTexto.Foreground = new SolidColorBrush(Colors.Gray);
        }

        private void MarcarEntregue_Click(object sender, RoutedEventArgs e)
        {
            // Descobre qual botão foi clicado e de qual pedido ele é
            if (sender is Button botao && botao.DataContext is Pedido pedidoClicado)
            {
                if (pedidoClicado.Status == "Entregue")
                {
                    MessageBox.Show("Este pedido já foi entregue!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var resp = MessageBox.Show($"Deseja marcar o pedido de {pedidoClicado.Recebedor} como ENTREGUE?",
                                           "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (resp == MessageBoxResult.Yes)
                {
                    // 1. Muda o status
                    pedidoClicado.Status = "Entregue";

                    // 2. Salva a alteração no TXT
                    PedidoRepository repo = new PedidoRepository();
                    repo.AtualizarArquivoTxt();

                    // 3. MÁGICA DE UI: Tira o pedido da lista de pendentes e joga pra lista de entregues!
                    // A tabela se atualiza sozinha sem precisar do "Refresh()".
                    ListaPedidosPendentes.Remove(pedidoClicado);
                    ListaPedidosEntregues.Add(pedidoClicado);

                    MessageBox.Show("Pedido atualizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}