using CestaApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel; // 🔥 Importante para o INotifyPropertyChanged
using System.Linq;
using System.Runtime.CompilerServices; // 🔥 Importante para o CallerMemberName
using System.Windows;
using System.Windows.Controls;
using Teste.Model;
using Teste.Repository;

namespace CestaApp.Views
{
    // 🔥 Adicionado INotifyPropertyChanged à classe para que a tela mude o preço na hora
    public partial class CestaView : UserControl, INotifyPropertyChanged
    {
        public Cesta CestaAtual { get; set; }
        public ObservableCollection<Produto> ProdutosDaCesta { get; set; }
        public string Observacoes { get; set; }

        // 🔥 Propriedade calculada dinâmica: varre os produtos e soma (Preço * QuantidadeSelecionada)
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
                    // 🔥 AMARRAÇÃO CRÍTICA: Faz a tela escutar se as propriedades deste produto mudaram
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

        // 🔥 O SEGREDO: Captura a alteração de quantidade do Produto e atualiza o total geral na tela
        private void Produto_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Produto.QuantidadeSelecionada) || e.PropertyName == nameof(Produto.SubtotalItem))
            {
                // Notifica o XAML que o preço total mudou e precisa ser redesenhado
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

                if (diferenca > 0) produto.Preco = produto.Preco; // apenas segurança

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
                GridProdutos.Items.Refresh();
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
                    GridProdutos.Items.Refresh();
                }
            }
        }

        private void AdicionarCarrinho_Click(object sender, RoutedEventArgs e)
        {
            if (CestaAtual == null) return;

            List<Produto> listaFinalParaCarrinho = new List<Produto>();

            foreach (var p in ProdutosDaCesta)
            {
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

            Cesta cestaClonadaParaCarrinho = new Cesta(CestaAtual.Id)
            {
                Nome = CestaAtual.Nome,
                // 🔥 Atualiza o preço da cesta clonada com o total dinâmico que o usuário montou
                Preco = this.ValorTotalCesta,
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

        // 🔥 Implementação da interface INotifyPropertyChanged para a View
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}