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

        // 🚀 NOVA LÓGICA: Calcula baseado no preço cadastrado da Cesta + ou - as alterações
        public decimal ValorTotalCesta
        {
            get
            {
                if (CestaAtual == null) return 0;

                // Começa estritamente com o valor definido no cadastro (Ex: R$ 76.50)
                decimal valorBase = CestaAtual.Preco;
                decimal variacaoPreco = 0;

                if (ProdutosDaCesta != null)
                {
                    foreach (var produto in ProdutosDaCesta)
                    {
                        string chave = produto.Nome.Trim().ToUpper();
                        if (_quantidadesOriginaisFabrica.ContainsKey(chave))
                        {
                            int qtdOriginal = _quantidadesOriginaisFabrica[chave];
                            // Se der positivo: adicionou itens. Se der negativo: retirou itens.
                            int diferencaQtd = produto.QuantidadeSelecionada - qtdOriginal;

                            // Multiplica a diferença unitária pelo preço do produto
                            variacaoPreco += (diferencaQtd * produto.Preco);
                        }
                    }
                }

                // Retorna o valor fixo de tabela somado algébricamente com a variação (que pode ser negativa)
                decimal totalCalculado = valorBase + variacaoPreco;

                // Garante que a cesta nunca fique com valor negativo caso removam tudo
                return totalCalculado > 0 ? totalCalculado : 0;
            }
        }

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

                        // Salva rigorosamente a estrutura inicial vinda do banco
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
                // Notifica a interface para atualizar o textblock do valor final total
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

            var cestaOriginalDoBanco = MemoriaCestas.Lista.FirstOrDefault(c => c.Id == CestaAtual.Id);
            string nomeVerdadeiroDaCesta = cestaOriginalDoBanco?.Nome ?? CestaAtual.Nome;

            // 🚀 ATUALIZADO: O preço final leva em conta a nossa nova propriedade inteligente
            decimal precoFinalCesta = this.ValorTotalCesta;

            Cesta cestaClonadaParaCarrinho = new Cesta(CestaAtual.Id)
            {
                Nome = nomeVerdadeiroDaCesta,
                Preco = precoFinalCesta, // Passa o valor correto (Base + Alterações) para o carrinho
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