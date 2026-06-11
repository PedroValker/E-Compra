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

                // 🚀 GATILHO VISUAL NA REMOÇÃO: Atualiza o indicador
                var janelaMae = Window.GetWindow(this) as Teste.View.TelaPrincipalCliente;
                if (janelaMae != null) janelaMae.AtualizarBadgeCarrinho();

                VerificarSeCarrinhoEstaVazio();
            }
        }

        // 🔥 MÉTODO ATUALIZADO: Executa toda a lógica em memória e persiste em arquivo TXT apenas no fim
        // 🔥 MÉTODO CORRIGIDO: Vincula o IdUsuario na memória antes de persistir no arquivo TXT
        private void FinalizarPedido_Click(object sender, RoutedEventArgs e)
        {
            var itemCarrinho = MemoriaCarrinho.Itens.FirstOrDefault();
            if (itemCarrinho == null) return;

            // 1. Definição do endereço de entrega (prioriza o do carrinho, fallback para o do usuário logado)
            string enderecoFinal = "A combinar";
            if (!string.IsNullOrWhiteSpace(itemCarrinho.EnderecoEntrega))
            {
                enderecoFinal = itemCarrinho.EnderecoEntrega;
            }
            else if (Sessao.UsuarioLogado != null && Sessao.UsuarioLogado.Endereco != null)
            {
                var end = Sessao.UsuarioLogado.Endereco;
                enderecoFinal = $"{end.Rua}, nº {end.Numero} - {end.Bairro}";
            }

            // 2. Criação do objeto de Pedido com todas as propriedades necessárias
            Pedido novoPedido = new Pedido
            {

                NomePedido = "PED-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                Recebedor = Sessao.UsuarioLogado?.Nome ?? "Cliente",
                Endereco = enderecoFinal,
                FormaPagamento = "A combinar",
                Status = "Pendente",
                Total = itemCarrinho.CestaSelecionada.Preco,
                DataDoPedido = DateTime.Now.ToString("dd/MM/yyyy"),
                CestaComprada = itemCarrinho.CestaSelecionada,
                Itens = new List<ItemPedido>(),
                Observacoes = !string.IsNullOrWhiteSpace(itemCarrinho.Observacoes) ? itemCarrinho.Observacoes.Trim() : "",

                // 🚀 CORREÇÃO CRÍTICA: Associa o ID do Usuário Logado ao pedido na memória
                IdUsuario = Sessao.UsuarioLogado != null ? Sessao.UsuarioLogado.Id : 0
            };

            // Gera o ID incremental baseado nos dados atuais de memória
            novoPedido.IdPedido = MemoriaPedidos.Lista.Any()
                ? MemoriaPedidos.Lista.Max(p => p.IdPedido) + 1
                : 1;

            // 3. Adiciona a linha de identificação da Cesta nos itens do pedido
            novoPedido.Itens.Add(new ItemPedido
            {
                Nome = itemCarrinho.CestaSelecionada.Nome,
                Quantidade = 1
            });

            // 4. Mapeia a receita original de fábrica para comparação
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

            // 5. Mapeia a composição customizada trazida pelo cliente no carrinho
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

            // 6. Analisa se houveram modificações na estrutura da cesta
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

            // Se modificado, acopla a listagem detalhada de alterações ao pedido em memória
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

            MemoriaPedidos.Lista.Add(novoPedido);
            MemoriaCarrinho.Itens.Clear();
            ItensNoCarrinho.Clear();

            // 🚀 GATILHO VISUAL NO FECHAMENTO: Limpa a bolinha vermelha da tela principal
            var janelaMae = Window.GetWindow(this) as Teste.View.TelaPrincipalCliente;
            if (janelaMae != null) janelaMae.AtualizarBadgeCarrinho();

            MessageBox.Show("Pedido finalizado e salvo com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            VerificarSeCarrinhoEstaVazio();
        }
    }
}