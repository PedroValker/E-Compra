using CestaApp.Views;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Teste.Model;
using Teste.Repository;

namespace Teste.View
{
    public partial class TelaPrincipalCliente : Window
    {
        public ObservableCollection<Cesta> ListaCestas { get; set; }

        // 🔥 VARIÁVEL PARA GUARDAR A TELA INICIAL
        private object _telaInicial;

        public TelaPrincipalCliente(string nome)
        {
            InitializeComponent();

            // 🔥 1. REGISTRA QUEM ACABOU DE LOGAR
            Sessao.UsuarioLogado = nome;

            // 2. Continua o seu carregamento normal...
            _telaInicial = ConteudoPrincipal.Content;
            ListaCestas = new ObservableCollection<Cesta>();
            CarregarCestasDoBanco();
            this.DataContext = this;
            NomeUsuarioText.Text = $"Olá, {nome}";

            // 🔥 3. CARREGA O CARRINHO ESPECÍFICO DESTE USUÁRIO
            CarrinhoRepository repoCarrinho = new CarrinhoRepository();
            repoCarrinho.CarregarDoArquivo();
        }
        // 🔥 NOVO MÉTODO: Navega para a tela do carrinho
        private void VerCarrinho_Click(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new CarrinhoView();
        }
        public TelaPrincipalCliente()
        {
            InitializeComponent();

            // Fazemos o mesmo aqui no construtor vazio
            _telaInicial = ConteudoPrincipal.Content;

            ListaCestas = new ObservableCollection<Cesta>();
            CarregarCestasDoBanco();
            this.DataContext = this;
        }

        private void ComprarCesta_Click(object sender, RoutedEventArgs e)
        {
            // 1. Descobre qual botão foi clicado
            Button botaoClicado = sender as Button;

            // 2. O 'DataContext' do botão na lista é a própria Cesta que ele está desenhando
            Cesta cestaSelecionada = botaoClicado.DataContext as Cesta;

            if (cestaSelecionada != null)
            {
                // 3. Navega para a CestaView passando a Cesta escolhida no construtor
                ConteudoPrincipal.Content = new CestaView(cestaSelecionada);
            }
        }
        private void CarregarCestasDoBanco()
        {
            CestaRepository repo = new CestaRepository();
            repo.CarregarDoArquivo();
            ListaCestas.Clear();

            foreach (Cesta cesta in MemoriaCestas.Lista)
            {
                ListaCestas.Add(cesta);
            }
        }

        // Navega para a tela de Pedido
        private void FaçaPedido(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new CestaView();
        }

        // 🔥 NOVO MÉTODO: Volta para a tela inicial
        private void VoltarInicio_Click(object sender, RoutedEventArgs e)
        {
            // Pega o conteúdo original que salvamos e joga de volta na tela
            ConteudoPrincipal.Content = _telaInicial;
        }
    }
}