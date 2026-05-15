using CestaApp.Views;
using System.Windows;
using Teste.Model;

namespace Teste.View
{
    public partial class PrincipalAdministrador : Window
    {
        private User usuarioAdmin;

        public PrincipalAdministrador(User user)
        {
            InitializeComponent();

            usuarioAdmin = user;

            // ✔ guarda apenas ID na sessão
            Sessao.UsuarioLogado = user.Id;

            NomeAdminMenu.Text = $"Olá, {user.Nome}";

            ConteudoPrincipal.Content = new HomeAdministrador(user);
        }

        private void Inicio_Click(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new HomeAdministrador(usuarioAdmin);
        }

        private void Pedidos_Click(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new PedidosAdminView();
        }

        private void Cadastrar_Click(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new CadastroProduto();
        }

        private void Logoff_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Deseja realmente sair do sistema?",
                "Confirmação",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // ✔ limpar sessão corretamente
                Sessao.UsuarioLogado = 0;

                MainWindow login = new MainWindow();
                login.Show();

                this.Close();
            }
        }

        private void Pendencias_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Cestas_Click(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new CadastroCesta();
        }

        private void Estatisticas_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Clientes_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}