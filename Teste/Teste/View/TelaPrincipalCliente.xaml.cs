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

        private object _telaInicial;

        private User usuarioLogado;

        public TelaPrincipalCliente(User user)
        {
            InitializeComponent();

            // ✔ usuário real vindo do login (COM ID correto)
            usuarioLogado = user;

            // ✔ salva ID na sessão (ESSENCIAL)
            Sessao.UsuarioLogado = user.Id;

            NomeUsuarioText.Text = $"Olá, {user.Nome}";

            ListaCestas = new ObservableCollection<Cesta>();
            DataContext = this;

            CarregarCestasDoBanco();
        }

        // =========================
        // ATUALIZA UI
        // =========================
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

        // =========================
        // LOAD INICIAL
        // =========================
        private void TelaPrincipalCliente_Loaded(object sender, RoutedEventArgs e)
        {
            _telaInicial = ConteudoPrincipal.Content;
        }

        // =========================
        // CESTAS
        // =========================
        private void CarregarCestasDoBanco()
        {
            CestaRepository repo = new CestaRepository();
            repo.CarregarDoArquivo();

            ListaCestas.Clear();

            foreach (var cesta in MemoriaCestas.Lista.Take(3))
                ListaCestas.Add(cesta);
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

        // =========================
        // PERFIL (MENU)
        // =========================
        private void AbrirMenuPerfil_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement =
                    System.Windows.Controls.Primitives.PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
            }
        }

        // =========================
        // EDITAR PERFIL (CORRIGIDO)
        // =========================
        private void EditarPerfil_Click(object sender, RoutedEventArgs e)
        {
            // ✔ pega usuário REAL da memória pelo ID
            var user = MemoriaUsuarios.Lista
     .FirstOrDefault(u => u.Id == Sessao.UsuarioLogado);

            if (user == null)
            {
                MessageBox.Show("Usuário não encontrado.");
                return;
            }

            ConteudoPrincipal.Content = new EditarPerfilCliente(user);
        }

        // =========================
        // PEDIDOS
        // =========================
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

        // =========================
        // LOGOFF
        // =========================
        private void Logoff_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult resposta = MessageBox.Show(
                "Tem certeza que deseja sair da sua conta?",
                "Confirmação de Logoff",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resposta == MessageBoxResult.Yes)
            {
                Sessao.UsuarioLogado = 0;

                var telaLogin = new Login();
                telaLogin.Show();

                this.Close();
            }
        }

        // =========================
        // VOLTAR HOME
        // =========================
        private void VoltarInicio_Click(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = _telaInicial;
        }
    }
}