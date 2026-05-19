using CestaApp.Views;
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

        // Calcula a soma de tudo (convertendo decimal para double caso seu Pedido exija double, ou mantendo se for decimal)
        public decimal ValorTotal => ItensNoCarrinho != null ? ItensNoCarrinho.Sum(item => item.Subtotal) : 0;

        public CarrinhoView()
        {
            InitializeComponent();
            CarregarCarrinho();
        }

        private void CarregarCarrinho()
        {
            // Pega o que está salvo na memória e joga pra tela
            ItensNoCarrinho = new ObservableCollection<ItemCarrinho>(MemoriaCarrinho.Itens); // Nota: Verifique se na sua classe está grafado 'Iatens' ou 'Itens'
            this.DataContext = this;

            // 🚀 NOVO: Verifica o estado do carrinho assim que a tela abre
            VerificarSeCarrinhoEstaVazio();
        }

        // 🚀 NOVO: Controla a visibilidade dos painéis de carrinho ativo vs vazio de forma dinâmica
        private void VerificarSeCarrinhoEstaVazio()
        {
            if (ItensNoCarrinho == null || ItensNoCarrinho.Count == 0)
            {
                PainelCarrinhoAtivo.Visibility = Visibility.Collapsed;
                PainelCarrinhoVazio.Visibility = Visibility.Visible;
                BtnFinalizar.IsEnabled = false;
            }
            else
            {
                PainelCarrinhoAtivo.Visibility = Visibility.Visible;
                PainelCarrinhoVazio.Visibility = Visibility.Collapsed;
                BtnFinalizar.IsEnabled = true;
            }
        }

        // 🚀 NOVO: Ação do botão do Empty State para retornar à vitrine inicial
        private void VoltarParaLoja_Click(object sender, RoutedEventArgs e)
        {
            var janelaPrincipal = Window.GetWindow(this) as Teste.View.TelaPrincipalCliente;
            if (janelaPrincipal != null)
            {
                janelaPrincipal.VoltarInicio_Click(sender, e);
            }
        }

        private void RemoverItem_Click(object sender, RoutedEventArgs e)
        {
            Button botaoRemover = sender as Button;
            if (botaoRemover == null) return;

            ItemCarrinho itemParaRemover = botaoRemover.DataContext as ItemCarrinho;

            if (itemParaRemover != null)
            {
                // Remove da memória global e da lista observada da tela
                MemoriaCarrinho.Itens.Remove(itemParaRemover);
                ItensNoCarrinho.Remove(itemParaRemover);

                // Atualiza o arquivo físico do carrinho para persistência em disco
                CarrinhoRepository repoCarrinho = new CarrinhoRepository();
                repoCarrinho.AtualizarArquivoTxt();

                // Força o WPF a recalcular as propriedades e o Valor Total
                this.DataContext = null;
                this.DataContext = this;

                // 🚀 NOVO: Reavalia se a remoção zerou o carrinho para exibir a tela de vazio
                VerificarSeCarrinhoEstaVazio();
            }
        }

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

            // 3. Monta o objeto do Pedido amarrado ao ID estável e dinâmico da Sessão
            Pedido novoPedido = new Pedido
            {
                NomePedido = $"PED-{DateTime.Now:yyyyMMddHHmmss}",
                Recebedor = Sessao.UsuarioLogado.Nome,
                IdUsuario = Sessao.UsuarioLogado.Id, // 🚀 VÍNCULO POR ID GARANTIDO AQUI!
                Endereco = "A combinar",
                FormaPagamento = "A combinar",
                Status = "Pendente",
                Total = this.ValorTotal,
                Itens = itensDoPedido,
                Observacoes = string.IsNullOrEmpty(obsGeral) ? "Sem observações" : obsGeral
            };

            // 4. Salva o pedido anexando-o no fim do arquivo txt
            PedidoRepository repoPedido = new PedidoRepository();
            repoPedido.AdicionarNovoPedidoNoTxt(novoPedido);

            MessageBox.Show("Pedido finalizado com sucesso! O administrador já foi notificado.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

            // 5. Limpa o carrinho por completo na memória interna e na interface
            MemoriaCarrinho.Itens.Clear();
            ItensNoCarrinho.Clear();

            // Salva o estado vazio do carrinho em disco
            CarrinhoRepository repoCarrinho = new CarrinhoRepository();
            repoCarrinho.AtualizarArquivoTxt();

            // Reseta o contexto de renderização
            this.DataContext = null;
            this.DataContext = this;

            // 🚀 NOVO: Chuta o usuário para a tela de Estado Vazio pós-compra realizada
            VerificarSeCarrinhoEstaVazio();
        }
    }
}