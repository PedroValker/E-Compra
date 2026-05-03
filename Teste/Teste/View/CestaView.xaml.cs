using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Teste.Model;

namespace CestaApp.Views
{
    public partial class CestaView : UserControl
    {
        public Cesta CestaAtual { get; set; }
        public ObservableCollection<Produto> ProdutosDaCesta { get; set; }

        // 🔥 Variável para guardar o texto da caixinha de observações
        public string Observacoes { get; set; }

        public CestaView(Cesta cesta)
        {
            InitializeComponent();
            CestaAtual = cesta;

            if (CestaAtual.Itens != null)
                ProdutosDaCesta = new ObservableCollection<Produto>(CestaAtual.Itens);
            else
                ProdutosDaCesta = new ObservableCollection<Produto>();

            this.DataContext = this;
        }

        public CestaView()
        {
            InitializeComponent();
        }
        // 🔥 Aumenta a quantidade do item específico
        // 🔥 Aumenta a quantidade do item específico
        private void AumentarQtd_Click(object sender, RoutedEventArgs e)
        {
            // O "is" já verifica se é um botão e já cria a variável "botao" ao mesmo tempo
            if (sender is Button botao && botao.DataContext is Teste.Model.Produto produto)
            {
                produto.QuantidadeSelecionada++;
                GridProdutos.Items.Refresh();
            }
        }

        // 🔥 Diminui a quantidade do item específico
        private void DiminuirQtd_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button botao && botao.DataContext is Teste.Model.Produto produto)
            {
                if (produto.QuantidadeSelecionada > 0)
                {
                    produto.QuantidadeSelecionada--;
                    GridProdutos.Items.Refresh();
                }
            }
        }
        // 🔥 Ação do botão Adicionar ao Carrinho
        private void AdicionarCarrinho_Click(object sender, RoutedEventArgs e)
        {
            if (CestaAtual == null) return;

            // 1. Cria um novo item para o carrinho
            ItemCarrinho novoItem = new ItemCarrinho
            {
                CestaSelecionada = CestaAtual,
                Quantidade = 1, // Por padrão adiciona 1 cesta
                Observacoes = this.Observacoes // Pega o texto que o usuário digitou
            };

            // 2. Salva na memória do sistema
            MemoriaCarrinho.Itens.Add(novoItem);

            // 3. Mostra mensagem de sucesso
            MessageBox.Show($"'{CestaAtual.Nome}' foi adicionada ao seu carrinho com sucesso!",
                            "Carrinho",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

            // Opcional: Limpa as observações após adicionar
            this.Observacoes = "";
            // Atualiza a tela para limpar a caixinha visualmente (gambiarra simples do WPF)
            this.DataContext = null;
            this.DataContext = this;
        }
    }
}