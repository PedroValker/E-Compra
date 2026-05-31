using CestaApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Teste.Model;
using Teste.Repository;

namespace CestaApp.Views
{
    public partial class CestaView : UserControl, INotifyPropertyChanged
    {
        public Cesta CestaAtual { get; set; }
        public ObservableCollection<Produto> ProdutosDaCesta { get; set; }
        public string Observacoes { get; set; }

        public decimal ValorTotalCesta => ProdutosDaCesta != null ? ProdutosDaCesta.Sum(p => p.Preco * p.QuantidadeSelecionada) : 0;

        private Dictionary<string, int> _quantidadesOriginaisFabrica = new Dictionary<string, int>();

        public CestaView(Cesta cesta)
        {
            InitializeComponent();

            var cestaOriginal = MemoriaCestas.Lista.FirstOrDefault(c =>
                c.Nome != null && c.Nome.Trim().ToUpper() == cesta?.Nome?.Trim()?.ToUpper());

            CestaAtual = cestaOriginal ?? cesta;
            ProdutosDaCesta = new ObservableCollection<Produto>();
            _quantidadesOriginaisFabrica.Clear();

            if (CestaAtual != null && CestaAtual.Itens != null)
            {
                var itensAgrupados = CestaAtual.Itens
                    .Where(p => p != null && !string.IsNullOrEmpty(p.Nome))
                    .GroupBy(p => p.Nome.Trim().ToUpper())
                    .Select(grupo =>
                    {
                        var primeiroItem = grupo.First();
                        int totalDeFabrica = grupo.Count();

                        _quantidadesOriginaisFabrica[primeiroItem.Nome.Trim().ToUpper()] = totalDeFabrica;

                        return new Produto
                        {
                            Nome = primeiroItem.Nome,
                            Preco = primeiroItem.Preco,
                            Peso = totalDeFabrica.ToString(),
                            QuantidadeSelecionada = totalDeFabrica
                        };
                    }).ToList();

                foreach (var item in itensAgrupados)
                {
                    item.PropertyChanged -= Produto_PropertyChanged;
                    item.PropertyChanged += Produto_PropertyChanged;

                    ProdutosDaCesta.Add(item);
                }
            }

            this.DataContext = this;
        }

        public CestaView()
        {
            InitializeComponent();
        }

        private void Produto_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Produto.QuantidadeSelecionada) || e.PropertyName == nameof(Produto.SubtotalItem))
            {
                OnPropertyChanged(nameof(ValorTotalCesta));
            }
        }

        private void AtualizarTextoDaVariacao(Produto produto)
        {
            string chave = produto.Nome.Trim().ToUpper();
            if (_quantidadesOriginaisFabrica.ContainsKey(chave))
            {
                int qtdOriginal = _quantidadesOriginaisFabrica[chave];
                int diferenca = produto.QuantidadeSelecionada - qtdOriginal;

                if (diferenca > 0) produto.Preco = produto.Preco;

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
                AtualizarTextoDaVariacao(produto);
            }
        }

        private void DiminuirQtd_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button botao && botao.DataContext is Produto produto)
            {
                if (produto.QuantidadeSelecionada > 0)
                {
                    produto.QuantidadeSelecionada--;
                    AtualizarTextoDaVariacao(produto);
                }
            }
        }

        private void AdicionarCarrinho_Click(object sender, RoutedEventArgs e)
        {
            if (CestaAtual == null) return;

            List<Produto> listaFinalParaCarrinho = new List<Produto>();
            bool foiModificada = false;

            // 1. Desfaz o agrupamento da interface para salvar os itens individuais no padrão do seu app
            foreach (var p in ProdutosDaCesta)
            {
                string chave = p.Nome.Trim().ToUpper();

                // 🔥 VERIFICAÇÃO DE MUDANÇA: Verifica se a quantidade atual é diferente da receita original de fábrica
                if (_quantidadesOriginaisFabrica.ContainsKey(chave) && _quantidadesOriginaisFabrica[chave] != p.QuantidadeSelecionada)
                {
                    foiModificada = true;
                }

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

            var cestaOriginalDoBanco = MemoriaCestas.Lista.FirstOrDefault(c => c.Id == CestaAtual.Id);
            string nomeVerdadeiroDaCesta = cestaOriginalDoBanco?.Nome ?? CestaAtual.Nome;
            decimal precoOriginalDeTabela = cestaOriginalDoBanco?.Preco ?? CestaAtual.Preco;

            // 🔥 REGRA DE PREÇO DO COMBO: 
            // Se a cesta não foi modificada (continua com a receita de fábrica), usamos o preço fixo de tabela (ex: R$ 76,50).
            // Se o cliente alterou alguma quantidade, ela assume o valor recalculado dinamicamente (ex: R$ 87,60).
            decimal precoFinalCesta = foiModificada ? this.ValorTotalCesta : precoOriginalDeTabela;

            Cesta cestaClonadaParaCarrinho = new Cesta(CestaAtual.Id)
            {
                Nome = nomeVerdadeiroDaCesta,
                Preco = precoFinalCesta,
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

            MessageBox.Show($"'{nomeVerdadeiroDaCesta}' foi adicionada ao seu carrinho com sucesso!",
                            "Carrinho",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

            this.Observacoes = "";
            this.DataContext = null;
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}