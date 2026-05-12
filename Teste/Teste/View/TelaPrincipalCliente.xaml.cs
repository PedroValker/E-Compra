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

        // 🔥 CORREÇÃO 1: Trocado de UserControl para object
        private object _telaInicial;

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
            // 🔥 CORREÇÃO 2: Removido o "as UserControl". Agora ele pega o conteúdo puro!
            _telaInicial = ConteudoPrincipal.Content;
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
            // 1. Instancia a View
            var telaPedido = new FacaSeuPedidoView();

            // 2. "Escuta" o evento. Quando a CestaSelecionada for disparada lá dentro...
            telaPedido.CestaSelecionada += (cesta) =>
            {
                // ...ele troca o conteúdo principal para a CestaView!
                ConteudoPrincipal.Content = new CestaView(cesta);
            };

            // 3. Mostra a tela de pedidos na tela
            ConteudoPrincipal.Content = telaPedido;
        }

        private void Pedidos(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new PedidosView();
        }

        private void VoltarInicio_Click(object sender, RoutedEventArgs e)
        {
            // Agora isso vai funcionar perfeitamente e voltar o conteúdo original
            ConteudoPrincipal.Content = _telaInicial;
        }
    }
}