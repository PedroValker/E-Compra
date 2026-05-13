using CestaApp.Views;
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
        public void AtualizarUsuario(string nome)
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
        private void EntreEmContato(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new ContatoNovo();
        }
        private void EditarPerfil_Click(object sender, RoutedEventArgs e)
        {
            User user = new User
            {
                Nome = Sessao.UsuarioLogado, // você já tem o nome
                Email = "",
                Telefone = "",
                FotoPerfil = ""
            };

            ConteudoPrincipal.Content = new EditarPerfilCliente(user);
        }
        private void Logoff_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // 1. Exibe a caixa de diálogo perguntando se ele quer sair
            MessageBoxResult resposta = MessageBox.Show(
                "Tem certeza que deseja sair da sua conta?",
                "Confirmação de Logoff",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            // 2. Verifica se o usuário clicou em "Sim"
            if (resposta == MessageBoxResult.Yes)
            {
                // Limpa os dados do usuário logado na Sessão
                Teste.Model.Sessao.UsuarioLogado = null;

                // Instancia e abre a tela de Login novamente
                var telaLogin = new Login();
                telaLogin.Show();

                // Fecha a tela atual (Dashboard)
                this.Close();
            }
            // Se ele clicar em "Não", o código ignora o IF e a tela continua aberta normalmente!
        }
        private void VoltarInicio_Click(object sender, RoutedEventArgs e)
        {
            // Agora isso vai funcionar perfeitamente e voltar o conteúdo original
            ConteudoPrincipal.Content = _telaInicial;
        }
    }
}