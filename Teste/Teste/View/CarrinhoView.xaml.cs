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
        public ObservableCollection<ItemCarrinho> ItensNoCarrinho { get; set; }

        public decimal ValorTotal => ItensNoCarrinho != null ? ItensNoCarrinho.Sum(item => item.Subtotal) : 0;

        public CarrinhoView()
        {
            InitializeComponent();
            CarregarCarrinho();
        }

        private void CarregarCarrinho()
        {
            ItensNoCarrinho = new ObservableCollection<ItemCarrinho>(MemoriaCarrinho.Itens);
            this.DataContext = this;

            VerificarSeCarrinhoEstaVazio();
        }

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

        private void VoltarParaLoja_Click(object sender, RoutedEventArgs e)
        {
            var SkinnerPrincipal = Window.GetWindow(this) as Teste.View.TelaPrincipalCliente;
            if (SkinnerPrincipal != null)
            {
                SkinnerPrincipal.VoltarInicio_Click(sender, e);
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

                // 🔴 REMOVIDO: CarrinhoRepository e AtualizarArquivoTxt() saíram daqui.
                // A alteração fica guardada apenas na lista estática na memória.

                // Força o WPF a recalcular as propriedades e o Valor Total
                this.DataContext = null;
                this.DataContext = this;

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

            List<ItemPedido> itensDoPedido = new List<ItemPedido>();
            foreach (var item in ItensNoCarrinho)
            {
                itensDoPedido.Add(new ItemPedido
                {
                    Nome = item.CestaSelecionada.Nome,
                    Quantidade = item.Quantidade
                });
            }

            string obsGeral = string.Join(" | ", ItensNoCarrinho
                .Where(i => !string.IsNullOrWhiteSpace(i.Observacoes))
                .Select(i => $"{i.CestaSelecionada.Nome}: {i.Observacoes}"));

            Pedido novoPedido = new Pedido
            {
                NomePedido = $"PED-{DateTime.Now:yyyyMMddHHmmss}",
                Recebedor = Sessao.UsuarioLogado.Nome,
                IdUsuario = Sessao.UsuarioLogado.Id,
                Endereco = "A combinar",
                FormaPagamento = "A combinar",
                Status = "Pendente",
                Total = this.ValorTotal,
                Itens = itensDoPedido,
                Observacoes = string.IsNullOrEmpty(obsGeral) ? "Sem observações" : obsGeral
            };

            // 🛠️ MODIFICADO: Adiciona apenas na memória em vez de escrever direto no arquivo TXT.
            // Certifique-se de que a sua classe global armazena a lista de pedidos em algo como MemoriaPedidos.Lista.
            MemoriaPedidos.Lista.Add(novoPedido);

            MessageBox.Show("Pedido finalizado com sucesso! O administrador já foi notificado.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

            // Limpa o carrinho por completo na memória interna e na interface
            MemoriaCarrinho.Itens.Clear();
            ItensNoCarrinho.Clear();

            // 🔴 REMOVIDO: As chamadas ao CarrinhoRepository para atualizar o arquivo sumiram daqui.

            // Reseta o contexto de renderização
            this.DataContext = null;
            this.DataContext = this;

            VerificarSeCarrinhoEstaVazio();
        }
    }
}