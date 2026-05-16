using CestaApp.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Teste.Model;
using Teste.Repository;
using System.IO;

namespace Teste.View
{
    public partial class TelaPrincipalCliente : Window
    {
        public ObservableCollection<Cesta> ListaCestas { get; set; }

        private object _telaInicial;

        // 🔥 CORREÇÃO: Construtor agora recebe o objeto User completo!
        public TelaPrincipalCliente(User usuario)
        {
            InitializeComponent();

            // Guardamos o usuário completo na Sessão para não perder o ID!
            Sessao.UsuarioLogado = usuario;

            ListaCestas = new ObservableCollection<Cesta>();

            this.DataContext = this;

            // Buscamos o nome de dentro do objeto do usuário
            NomeUsuarioText.Text = $"Olá, {usuario.Nome}";

            CarregarCestasDoBanco();

            CarrinhoRepository repoCarrinho = new CarrinhoRepository();
            repoCarrinho.CarregarDoArquivo();

            Loaded += TelaPrincipalCliente_Loaded;
        }

        public void UpdateUsuario(string nome)
        {
            NomeUsuarioText.Text = $"Olá, {nome}";
        }

        public void AtualizarFoto(string caminho)
        {
            try
            {
                if (!string.IsNullOrEmpty(caminho) && File.Exists(caminho))
                {
                    ImagemPerfil.Source = new BitmapImage(new Uri(caminho));
                }
            }
            catch { }
        }

        private void TelaPrincipalCliente_Loaded(object sender, RoutedEventArgs e)
        {
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

        private void AbrirMenuPerfil_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        private void FaçaPedido(object sender, RoutedEventArgs e)
        {
            var telaPedido = new FacaSeuPedidoView();

            telaPedido.CestaSelecionada += (cesta) =>
            {
                ConteudoPrincipal.Content = new CestaView(cesta);
            };

            ConteudoPrincipal.Content = telaPedido;
        }

        private void Pedidos(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new PedidosView();
        }

        private void EntreEmContato(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new ContatoNovo();
        }

        private void EditarPerfil_Click(object sender, RoutedEventArgs e)
        {
            // 🔥 CORREÇÃO: Passamos o objeto Usuário que está salvo na Sessão.
            // Ele vai com ID, Nome, Email e tudo preenchido do TXT!
            ConteudoPrincipal.Content = new EditarPerfilCliente(Sessao.UsuarioLogado);
        }

        private void Logoff_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBoxResult resposta = MessageBox.Show(
                "Tem certeza que deseja sair da sua conta?",
                "Confirmação de Logoff",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resposta == MessageBoxResult.Yes)
            {
                // Limpa a sessão colocando null
                Sessao.UsuarioLogado = null;

                var telaLogin = new Login();
                telaLogin.Show();

                this.Close();
            }
        }

        private void VoltarInicio_Click(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = _telaInicial;
        }
    }
}