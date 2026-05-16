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

            try
            {
                LoginViewModel vm = new LoginViewModel();
                User user = vm.FazerLogin(email, senha);

                if (user == null)
                {
                    MessageBox.Show("Usuário não encontrado ou senha incorreta.");
                    return;
                }

                // 🔥 LINHA NOVA: Salva o usuário encontrado (com o ID correto) na sessão global
                Sessao.UsuarioLogado = user;

                MessageBox.Show($"Bem-vindo, {user.Nome}!");

                Window tela;
                if (user.IsAdmin)
                {
                    tela = new PrincipalAdministrador(user.Nome);
                }
                else
                {
                    // Se sua tela principal precisar do ID, você já pode pegar de Sessao.UsuarioLogado.Id
                    tela = new TelaPrincipalCliente(user);
                }

                tela.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro no login: " + ex.Message);
            }
        }
        private void AbrirCadastro_Click(object sender, MouseButtonEventArgs e)
        {
            MainWindow cadastro = new MainWindow();
            cadastro.Show();
            this.Close();
        }
    }
}