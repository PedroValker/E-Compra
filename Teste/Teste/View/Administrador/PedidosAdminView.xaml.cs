using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Teste.Model;
using Teste.Repository;

// 🔥 CORREÇÃO AQUI: Ajustado para o nome real do seu projeto
namespace Teste.View
{
    public partial class PedidosAdminView : UserControl
    {
        public ObservableCollection<Pedido> ListaPedidos { get; set; }

        public PedidosAdminView()
        {
            InitializeComponent();
            CarregarPedidos();
        }

        private void CarregarPedidos()
        {
            // Puxa os dados atualizados do arquivo TXT
            PedidoRepository repo = new PedidoRepository();
            repo.CarregarDoArquivo();

            // Coloca na tela
            ListaPedidos = new ObservableCollection<Pedido>(MemoriaPedidos.Lista);
            this.DataContext = this;
        }
        private void VerItens_Click(object sender, RoutedEventArgs e)
        {
            // Descobre de qual pedido o admin clicou em "Ver Itens"
            if (sender is Button botao && botao.DataContext is Pedido pedidoClicado)
            {
                // Abre a janela que criamos passando o pedido específico
                DetalhesPedidoWindow modal = new DetalhesPedidoWindow(pedidoClicado);
                modal.ShowDialog(); // ShowDialog congela a tela de trás até o admin fechar o pop-up
            }
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

                // Pergunta de segurança
                var resp = MessageBox.Show($"Deseja marcar o pedido de {pedidoClicado.Recebedor} como ENTREGUE?",
                                           "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (resp == MessageBoxResult.Yes)
                {
                    // 1. Muda o status
                    pedidoClicado.Status = "Entregue";

                    // 2. Salva a alteração no TXT
                    PedidoRepository repo = new PedidoRepository();
                    repo.AtualizarArquivoTxt();

                    // 3. Atualiza a tabela na tela para o Admin ver a mudança
                    GridPedidos.Items.Refresh();

                    MessageBox.Show("Pedido atualizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}