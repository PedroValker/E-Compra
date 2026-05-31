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
                MemoriaCarrinho.Itens.Remove(itemParaRemover);
                ItensNoCarrinho.Remove(itemParaRemover);

                this.DataContext = null;
                this.DataContext = this;

                VerificarSeCarrinhoEstaVazio();
            }
        }

        // 🔥 MÉTODO CORRIGIDO: Protege o banco de dados contra duplicidade e falsas modificações
        private void FinalizarPedido_Click(object sender, RoutedEventArgs e)
        {
            var itemCarrinho = MemoriaCarrinho.Itens.FirstOrDefault();
            if (itemCarrinho == null) return;

            string enderecoCliente = "A combinar";
            if (Sessao.UsuarioLogado != null && Sessao.UsuarioLogado.Endereco != null)
            {
                var end = Sessao.UsuarioLogado.Endereco;
                enderecoCliente = $"{end.Rua}, nº {end.Numero} - {end.Bairro}";
            }

            Pedido novoPedido = new Pedido
            {
                NomePedido = "PED-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                Recebedor = Sessao.UsuarioLogado?.Nome ?? "Cliente",
                Endereco = enderecoCliente,
                FormaPagamento = "A combinar",
                Status = "Pendente",
                Total = itemCarrinho.CestaSelecionada.Preco,
                DataDoPedido = DateTime.Now.ToString("dd/MM/yyyy"),
                CestaComprada = itemCarrinho.CestaSelecionada,
                Itens = new List<ItemPedido>()
            };

            // 1. Registra a linha obrigatória identificando a Cesta no arquivo de texto
            novoPedido.Itens.Add(new ItemPedido
            {
                Nome = itemCarrinho.CestaSelecionada.Nome,
                Quantidade = 1
            });

            // 2. Mapeia a receita fixa de fábrica cadastrada no sistema
            var cestaOriginalDoBanco = MemoriaCestas.Lista.FirstOrDefault(c =>
                c.Nome.Trim().ToUpper() == itemCarrinho.CestaSelecionada.Nome.Trim().ToUpper());

            var mapaOriginalFabrica = new Dictionary<string, int>();
            if (cestaOriginalDoBanco != null && cestaOriginalDoBanco.Itens != null)
            {
                var agrupadoFabrica = cestaOriginalDoBanco.Itens
                    .Where(p => p != null && !string.IsNullOrEmpty(p.Nome))
                    .GroupBy(p => p.Nome.Trim().ToUpper());

                foreach (var g in agrupadoFabrica)
                {
                    mapaOriginalFabrica[g.Key] = g.Sum(p => p.QuantidadeSelecionada > 0 ? p.QuantidadeSelecionada : 1);
                }
            }

            // 3. Mapeia a sacola de compras modificada pelo cliente (Agrupamento sem repetição de palavras)
            var mapaCarrinhoCliente = new Dictionary<string, int>();
            if (itemCarrinho.CestaSelecionada.Itens != null)
            {
                var agrupadoCliente = itemCarrinho.CestaSelecionada.Itens
                    .Where(p => p != null && !string.IsNullOrEmpty(p.Nome))
                    .GroupBy(p => p.Nome.Trim().ToUpper());

                foreach (var g in agrupadoCliente)
                {
                    mapaCarrinhoCliente[g.Key] = g.Sum(p => p.QuantidadeSelecionada > 0 ? p.QuantidadeSelecionada : 1);
                }
            }

            // 4. ANALISADOR CRÍTICO DE MODIFICAÇÕES:
            bool temModificacao = false;
            var todosOsProdutos = mapaOriginalFabrica.Keys.Union(mapaCarrinhoCliente.Keys).Distinct();

            foreach (var produtoChave in todosOsProdutos)
            {
                mapaOriginalFabrica.TryGetValue(produtoChave, out int qtdF);
                mapaCarrinhoCliente.TryGetValue(produtoChave, out int qtdC);

                if (qtdF != qtdC)
                {
                    temModificacao = true;
                    break;
                }
            }

            // Se o carrinho foi modificado, grava as alterações detalhadas
            // Se NÃO foi modificado, deixa os itens limpos para o painel admin ler como "Completa"
            if (temModificacao && itemCarrinho.CestaSelecionada.Itens != null)
            {
                var produtosParaGravar = itemCarrinho.CestaSelecionada.Itens
                    .Where(p => p != null && !string.IsNullOrEmpty(p.Nome))
                    .GroupBy(p => p.Nome.Trim())
                    .Select(g => new ItemPedido
                    {
                        Nome = g.Key,
                        Quantidade = g.Sum(p => p.QuantidadeSelecionada > 0 ? p.QuantidadeSelecionada : 1)
                    }).ToList();

                foreach (var prod in produtosParaGravar)
                {
                    if (prod.Nome.ToUpper() != itemCarrinho.CestaSelecionada.Nome.ToUpper())
                    {
                        novoPedido.Itens.Add(prod);
                    }
                }
            }

            // 5. Salva de forma permanente e atualiza a interface limpa
            PedidoRepository repo = new PedidoRepository();
            repo.AdicionarNovoPedidoNoTxt(novoPedido);

            MemoriaCarrinho.Itens.Clear();
            ItensNoCarrinho.Clear();

            MessageBox.Show("Pedido finalizado e salvo com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

            VerificarSeCarrinhoEstaVazio();
        }
    }
}