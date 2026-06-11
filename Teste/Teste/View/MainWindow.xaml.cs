using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Teste.Model;
using Teste.Repository;

namespace Teste
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CriarConta_Click(object sender, RoutedEventArgs e)
        {
            string nome = NomeBox.Text.Trim();
            string email = EmailBox.Text.Trim();
            string telefone = TelefoneBox.Text.Trim();
            string senha = SenhaBox.Password;
            string confirmarSenha = ConfirmarSenhaBox.Password;

            // 🔥 VALIDAÇÃO DE CAMPOS VAZIOS
            if (string.IsNullOrWhiteSpace(nome) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(telefone) ||
                string.IsNullOrWhiteSpace(senha) ||
                string.IsNullOrWhiteSpace(confirmarSenha))
            {
                MessageBox.Show("Preencha todos os campos!");
                return;
            }

            // 🔥 VALIDAÇÃO DO GMAIL
            if (!EmailValido(email))
            {
                MessageBox.Show("E-mail inválido! O cadastro permite apenas contas @gmail.com");
                return;
            }

            // 🔥 VALIDAÇÃO DO TELEFONE
            if (!TelefoneValido(telefone))
            {
                MessageBox.Show("Telefone inválido! Digite o DDD + Número (10 ou 11 dígitos).");
                return;
            }

            if (senha != confirmarSenha)
            {
                MessageBox.Show("As senhas não coincidem!");
                return;
            }

            // 🔥 VERIFICA DUPLICIDADE
            foreach (var u in MemoriaUsuarios.Lista)
            {
                if (u.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Esse e-mail já está cadastrado!");
                    return;
                }
            }

            // 🔥 CRIA USUÁRIO
            User user = new User
            {
                Nome = nome,
                Email = email,
                Telefone = telefone,
                Senha = senha,
                DataCriacao = DateTime.Now
            };

            // 🔥 SALVA NA MEMÓRIA
            MemoriaUsuarios.Lista.Add(user);
            MessageBox.Show("Conta criada com sucesso!");

            // abre login
            Login login = new Login();
            login.Show();

            this.Close();
        }

        // Método atualizado para aceitar apenas @gmail.com
        private bool EmailValido(string email)
        {
            string emailMinusculo = email.ToLower().Trim();
            // Garante o formato correto e o final fixo @gmail.com
            string pattern = @"^[^@\s]+@gmail\.com$";
            return Regex.IsMatch(emailMinusculo, pattern);
        }

        private bool TelefoneValido(string telefone)
        {
            return telefone.Length == 10 || telefone.Length == 11;
        }

        private void TelefoneBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Entrar_Click(object sender, RoutedEventArgs e)
        {
            Login loginWindow = new Login();
            loginWindow.Show();
            this.Close();
        }
    }
}