using System;
using System.Windows;
using System.Windows.Input;
using TelaClientes;
using Teste.Model;
using Teste.View;
using Teste.ViewModel;

namespace Teste
{
    public partial class Login : Window
    {
        private bool senhaVisivel = false;

        public Login()
        {
            InitializeComponent();
        }

        private void ToggleSenha_Click(object sender, RoutedEventArgs e)
        {
            if (senhaVisivel)
            {
                SenhaBox.Password = SenhaVisivelBox.Text;
                SenhaBox.Visibility = Visibility.Visible;
                SenhaVisivelBox.Visibility = Visibility.Collapsed;
                BotaoSenha.Content = "Ver senha";
            }
            else
            {
                SenhaVisivelBox.Text = SenhaBox.Password;
                SenhaVisivelBox.Visibility = Visibility.Visible;
                SenhaBox.Visibility = Visibility.Collapsed;
                BotaoSenha.Content = "Esconder senha";
            }

            senhaVisivel = !senhaVisivel;
        }

        private void SenhaBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            SenhaPlaceholder.Visibility =
                string.IsNullOrEmpty(SenhaBox.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void SenhaVisivelBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            SenhaPlaceholder.Visibility =
                string.IsNullOrEmpty(SenhaVisivelBox.Text)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void BotaoEntrar_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailBox.Text.Trim();
            string senha = senhaVisivel ? SenhaVisivelBox.Text : SenhaBox.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha))
            {
                MessageBox.Show("Preencha email e senha.");
                return;
            }

            LoginViewModel vm = new LoginViewModel();
            User user = vm.FazerLogin(email, senha);

            if (user == null)
            {
                MessageBox.Show("Usuário não encontrado ou senha incorreta.");
                return;
            }

            MessageBox.Show($"Bem-vindo, {user.Nome}!");

            // ✅ GUARDA APENAS O ID (SEU PADRÃO)
            Sessao.UsuarioLogado = user.Id;

            Window tela;

            if (user.IsAdmin)
                tela = new PrincipalAdministrador(user);
            else
                tela = new TelaPrincipalCliente(user);

            tela.Show();
            this.Close();
        }

        private void AbrirCadastro_Click(object sender, MouseButtonEventArgs e)
        {
            MainWindow cadastro = new MainWindow();
            cadastro.Show();
            this.Close();
        }
    }
}