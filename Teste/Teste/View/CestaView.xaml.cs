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
    public partial class CestaView : UserControl
    {
        public Cesta CestaAtual { get; set; }
        public ObservableCollection<Produto> ProdutosDaCesta { get; set; }
        public string Observacoes { get; set; }

        // Dicionário privado para armazenar rigidamente a receita padrão de fábrica do TXT
        private Dictionary<string, int> _quantidadesOriginaisFabrica = new Dictionary<string, int>();

        public CestaView(Cesta cesta)
        {
            InitializeComponent();

            // Resgata o registro de fábrica direto da memória global para garantir integridade
            var cestaOriginal = MemoriaCestas.Lista.FirstOrDefault(c =>
                c.Nome != null && c.Nome.Trim().ToUpper() == cesta?.Nome?.Trim()?.ToUpper());

            CestaAtual = cestaOriginal ?? cesta;
            ProdutosDaCesta = new ObservableCollection<Produto>();
            _quantidadesOriginaisFabrica.Clear();

            if (CestaAtual != null && CestaAtual.Itens != null)
            {
                // Agrupa contando quantas ocorrências existem no arquivo txt
                var itensAgrupados = CestaAtual.Itens
                    .Where(p => p != null && !string.IsNullOrEmpty(p.Nome))
                    .GroupBy(p => p.Nome.Trim().ToUpper())
                    .Select(grupo =>
                    {
                        var primeiroItem = grupo.First();
                        int totalDeFabrica = grupo.Count();

                        // Guarda a quantidade original estável no dicionário para cálculo de Diff
                        _quantidadesOriginaisFabrica[primeiroItem.Nome.Trim().ToUpper()] = totalDeFabrica;

                        return new Produto
                        {
                            Nome = primeiroItem.Nome,
                            Preco = primeiroItem.Preco,
                            // 🔥 RESOLVIDO: A primeira coluna inicia exibindo a quantidade padrão de fábrica (ex: 12)
                            Peso = totalDeFabrica.ToString(),
                            // O contador da direita também inicia com o valor total cheio
                            QuantidadeSelecionada = totalDeFabrica
                        };
                    }).ToList();

                foreach (var item in itensAgrupados)
                {
                    ProdutosDaCesta.Add(item);
                }
            }

            this.DataContext = this;
        }

        public CestaView()
        {
            InitializeComponent();
        }

        // Modifica o texto da primeira coluna para mostrar a variação (+1, -2) quando houver ajustes
        private void AtualizarTextoDaVariacao(Produto produto)
        {
            string chave = produto.Nome.Trim().ToUpper();
            if (_quantidadesOriginaisFabrica.ContainsKey(chave))
            {
                int qtdOriginal = _quantidadesOriginaisFabrica[chave];
                int diferenca = produto.QuantidadeSelecionada - qtdOriginal;

                // Se o cliente alterou a quantidade, exibe o saldo com sinal. Se voltou ao padrão, mostra o valor base fixo.
                if (diferenca > 0) produto.Peso = $"+{diferenca}";
                else if (diferenca < 0) produto.Peso = $"{diferenca}";
                else produto.Peso = qtdOriginal.ToString();
            }
        }

        private void AumentarQtd_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button botao && botao.DataContext is Produto produto)
            {
                produto.QuantidadeSelecionada++;
                AtualizarTextoDaVariacao(produto); // Atualiza o saldo dinamicamente na tela
                GridProdutos.Items.Refresh(); // Redesenha a linha no WPF
            }
        }

        private void DiminuirQtd_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button botao && botao.DataContext is Produto produto)
            {
                if (produto.QuantidadeSelecionada > 0)
                {
                    produto.QuantidadeSelecionada--;
                    AtualizarTextoDaVariacao(produto); // Atualiza o saldo dinamicamente na tela
                    GridProdutos.Items.Refresh(); // Redesenha a linha no WPF
                }
            }
        }

        private void AdicionarCarrinho_Click(object sender, RoutedEventArgs e)
        {
            if (CestaAtual == null) return;

            List<Produto> listaFinalParaCarrinho = new List<Produto>();

            foreach (var p in ProdutosDaCesta)
            {
                // Monta a lista final baseado na quantidade acumulada no seletor da direita
                for (int i = 0; i < p.QuantidadeSelecionada; i++)
                {
                    listaFinalParaCarrinho.Add(new Produto
                    {
                        Nome = p.Nome,
                        Preco = p.Preco,
                        Peso = "",
                        QuantidadeSelecionada = 1
                    });
                }
            }

            // Clona a cesta de forma segura para não bagunçar referências na RAM global
            Cesta cestaClonadaParaCarrinho = new Cesta(CestaAtual.Id)
            {
                Nome = CestaAtual.Nome,
                Preco = CestaAtual.Preco,
                ImagemPath = CestaAtual.ImagemPath,
                Itens = listaFinalParaCarrinho
            };

            ItemCarrinho novoItem = new ItemCarrinho
            {
                CestaSelecionada = cestaClonadaParaCarrinho,
                Quantidade = 1,
                Observacoes = this.Observacoes
            };

            MemoriaCarrinho.Itens.Add(novoItem);

            MessageBox.Show($"'{CestaAtual.Nome}' foi adicionada ao seu carrinho com sucesso!",
                            "Carrinho",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

            this.Observacoes = "";
            this.DataContext = null;
            this.DataContext = this;
        }
    }
}