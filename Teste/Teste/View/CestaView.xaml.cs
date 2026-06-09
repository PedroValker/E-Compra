using CestaApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
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

        private string _enderecoEntrega;
        public string EnderecoEntrega
        {
            get => _enderecoEntrega;
            set
            {
                _enderecoEntrega = value;
                OnPropertyChanged();
            }
        }

        public decimal ValorTotalCesta
        {
            get
            {
                if (CestaAtual == null) return 0;

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
                            int diferencaQtd = produto.QuantidadeSelecionada - qtdOriginal;
                            variacaoPreco += (diferencaQtd * produto.Preco);
                        }
                    }
                }

                decimal totalCalculado = valorBase + variacaoPreco;
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

            // 🚀 AUTOMÁTICO: Resgata as informações cadastrais de endereço em memória do usuário ativo
            if (Sessao.UsuarioLogado != null && Sessao.UsuarioLogado.Endereco != null)
            {
                var end = Sessao.UsuarioLogado.Endereco;
                if (!string.IsNullOrWhiteSpace(end.Rua))
                {
                    EnderecoEntrega = $"{end.Rua}, nº {end.Numero} - {end.Bairro}";
                }
                else
                {
                    EnderecoEntrega = "A combinar";
                }
            }
            else
            {
                EnderecoEntrega = "A combinar";
            }

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
            decimal precoFinalCesta = this.ValorTotalCesta;

            // 🟢 CORREÇÃO: Propriedade 'Endereco' removida daqui de dentro para parar o erro de compilação
            Cesta cestaClonadaParaCarrinho = new Cesta(CestaAtual.Id)
            {
                Nome = nomeVerdadeiroDaCesta,
                Preco = precoFinalCesta,
                ImagemPath = CestaAtual.ImagemPath,
                Itens = listaFinalParaCarrinho
            };

            // 🚀 SOLUÇÃO: O endereço da tela agora deve ser associado ao ItemCarrinho.
            // Certifique-se de que sua classe 'ItemCarrinho' tenha a propriedade 'EnderecoEntrega'.
            ItemCarrinho novoItem = new ItemCarrinho
            {
                CestaSelecionada = cestaClonadaParaCarrinho,
                Quantidade = 1,
                Observacoes = this.Observacoes,
                EnderecoEntrega = this.EnderecoEntrega // Atribuição correta na classe de transporte do carrinho
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