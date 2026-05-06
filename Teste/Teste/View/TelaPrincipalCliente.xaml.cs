using CestaApp.Views;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Teste.Model;
using Teste.Repository;
using Teste.View.ContatoNovo;

namespace Teste.View
{
    public partial class TelaPrincipalCliente : Window
    {
        public ObservableCollection<Cesta> ListaCestas { get; set; }

        private UserControl _telaInicial;

        public TelaPrincipalCliente(string nome)
        {
            InitializeComponent();

            Sessao.UsuarioLogado = nome;

            ListaCestas = new ObservableCollection<Cesta>();

            this.DataContext = this;
            NomeUsuarioText.Text = $"Olá, {nome}";

            CarregarCestasDoBanco();

            CarrinhoRepository repoCarrinho = new CarrinhoRepository();
            repoCarrinho.CarregarDoArquivo();

            Loaded += TelaPrincipalCliente_Loaded;
        }

        private void TelaPrincipalCliente_Loaded(object sender, RoutedEventArgs e)
        {
            _telaInicial = ConteudoPrincipal.Content as UserControl;
        }

        private void VerCarrinho_Click(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new CarrinhoView();
        }

        private void ComprarCesta_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Cesta cesta)
            {
                ConteudoPrincipal.Content = new CestaView(cesta);
            }
        }

        private void CarregarCestasDoBanco()
        {
            CestaRepository repo = new CestaRepository();
            repo.CarregarDoArquivo();

            ListaCestas.Clear();

            foreach (var cesta in MemoriaCestas.Lista.Take(3))
                ListaCestas.Add(cesta);
        }

        private void FaçaPedido(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new FacaSeuPedidoView();
        }
        


        private void VoltarInicio_Click(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = _telaInicial;
        }
    }
}