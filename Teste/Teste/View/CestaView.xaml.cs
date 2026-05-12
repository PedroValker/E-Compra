using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Teste.Model;

namespace CestaApp.Views
{
    public partial class CestaView : UserControl
    {
        public Cesta CestaAtual { get; set; }
        public ObservableCollection<Produto> ProdutosDaCesta { get; set; }
        public string Observacoes { get; set; }

        public CestaView(Cesta cesta)
        {
            InitializeComponent();
            CestaAtual = cesta;

            ProdutosDaCesta = new ObservableCollection<Produto>();

            if (CestaAtual != null && CestaAtual.Itens != null)
            {
                // Agrupamos os produtos para exibição na tela (1 linha por produto com a Qtd somada)
                var itensAgrupados = CestaAtual.Itens
                    .GroupBy(p => p.Nome)
                    .Select(grupo => new Produto
                    {
                        Nome = grupo.First().Nome,
                        Peso = grupo.First().Peso,
                        Preco = grupo.First().Preco,
                        QuantidadeSelecionada = grupo.Count() // Conta quantos tem na lista original
                    });

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

        private void AumentarQtd_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button botao && botao.DataContext is Produto produto)
            {
                produto.QuantidadeSelecionada++;
                GridProdutos.Items.Refresh(); // Força a atualização visual da linha
            }
        }

        private void DiminuirQtd_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button botao && botao.DataContext is Produto produto)
            {
                if (produto.QuantidadeSelecionada > 0)
                {
                    produto.QuantidadeSelecionada--;
                    GridProdutos.Items.Refresh();
                }
            }
        }

        private void AdicionarCarrinho_Click(object sender, RoutedEventArgs e)
        {
            if (CestaAtual == null) return;

            // --- LÓGICA DE SINCRONIZAÇÃO ---
            // Precisamos que a CestaAtual.Itens tenha exatamente o que o usuário escolheu na tela.
            // Se ele aumentou a goiabada para 3, a lista final deve conter o objeto "Goiabada" 3 vezes.
            List<Produto> listaFinalParaCarrinho = new List<Produto>();

            foreach (var p in ProdutosDaCesta)
            {
                // Adicionamos o produto na lista a quantidade de vezes definida no seletor (+ e -)
                for (int i = 0; i < p.QuantidadeSelecionada; i++)
                {
                    listaFinalParaCarrinho.Add(new Produto
                    {
                        Nome = p.Nome,
                        Preco = p.Preco,
                        Peso = p.Peso
                    });
                }
            }

            // Atualizamos a lista de itens da cesta com a escolha atual do cliente
            CestaAtual.Itens = listaFinalParaCarrinho;

            // Criamos o item do carrinho passando a cesta já atualizada
            ItemCarrinho novoItem = new ItemCarrinho
            {
                CestaSelecionada = CestaAtual,
                Quantidade = 1, // 1 unidade da Cesta (que agora contém os itens extras)
                Observacoes = this.Observacoes
            };

            MemoriaCarrinho.Itens.Add(novoItem);

            MessageBox.Show($"'{CestaAtual.Nome}' foi adicionada ao seu carrinho com sucesso!",
                            "Carrinho",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

            // Limpa campos e atualiza tela
            this.Observacoes = "";
            this.DataContext = null;
            this.DataContext = this;
        }
    }
}