using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Teste.Model;
using Teste.Repository;

namespace CestaApp.Views
{
    public partial class CarrinhoView : UserControl
    {
        // Variável que o XAML vai ler
        public ObservableCollection<ItemCarrinho> ItensNoCarrinho { get; set; }

        // Calcula a soma de tudo
        public decimal ValorTotal => ItensNoCarrinho.Sum(item => item.Subtotal);

        public CarrinhoView()
        {
            InitializeComponent();
            CarregarCarrinho();
        }

        private void CarregarCarrinho()
        {
            // Pega o que está salvo na memória e joga pra tela
            ItensNoCarrinho = new ObservableCollection<ItemCarrinho>(MemoriaCarrinho.Itens);
            this.DataContext = this;
        }

        private void RemoverItem_Click(object sender, RoutedEventArgs e)
        {
            // Descobre qual botão remover foi clicado
            Button botaoRemover = sender as Button;
            ItemCarrinho itemParaRemover = botaoRemover.DataContext as ItemCarrinho;

            if (itemParaRemover != null)
            {
                // Remove da memória e da tela
                MemoriaCarrinho.Itens.Remove(itemParaRemover);
                ItensNoCarrinho.Remove(itemParaRemover);

                // Atualiza a tela e força a recalcular o Valor Total
                this.DataContext = null;
                this.DataContext = this;
            }
        }

        // 🔥 APENAS UM FINALIZAR PEDIDO AGORA!
        private void FinalizarPedido_Click(object sender, RoutedEventArgs e)
        {
            if (ItensNoCarrinho.Count == 0)
            {
                MessageBox.Show("Seu carrinho está vazio!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 1. Transforma os itens do carrinho no seu ItemPedido
            List<ItemPedido> itensDoPedido = new List<ItemPedido>();
            foreach (var item in ItensNoCarrinho)
            {
                itensDoPedido.Add(new ItemPedido
                {
                    Nome = item.CestaSelecionada.Nome,
                    Quantidade = item.Quantidade
                });
            }

            // 2. Junta todas as observações
            string obsGeral = string.Join(" | ", ItensNoCarrinho
                .Where(i => !string.IsNullOrWhiteSpace(i.Observacoes))
                .Select(i => $"{i.CestaSelecionada.Nome}: {i.Observacoes}"));

            // 3. Monta o objeto do Pedido
            Pedido novoPedido = new Pedido
            {
                NomePedido = $"PED-{DateTime.Now:yyyyMMddHHmmss}",
                Recebedor = Sessao.UsuarioLogado.Nome,
                Endereco = "A combinar",
                FormaPagamento = "A combinar",
                Status = "Pendente",
                Total = this.ValorTotal,
                Itens = itensDoPedido,
                Observacoes = string.IsNullOrEmpty(obsGeral) ? "Sem observações" : obsGeral
            };

            // 4. SALVAMENTO CORRIGIDO ✅
            PedidoRepository repoPedido = new PedidoRepository();

            // 🔥 TROCADO: Em vez de AtualizarArquivoTxt, usamos AdicionarNovoPedidoNoTxt
            // Isso garante que o pedido seja ANEXADO ao arquivo, sem apagar os anteriores.
            repoPedido.AdicionarNovoPedidoNoTxt(novoPedido);

            MessageBox.Show("Pedido finalizado com sucesso! O administrador já foi notificado.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

            // 5. Limpa o carrinho
            MemoriaCarrinho.Itens.Clear();
            ItensNoCarrinho.Clear();

            CarrinhoRepository repoCarrinho = new CarrinhoRepository();
            repoCarrinho.AtualizarArquivoTxt();

            this.DataContext = null;
            this.DataContext = this;
        }
    }
}