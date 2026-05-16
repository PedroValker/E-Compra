using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Teste.Model;
using Teste.Repository;

namespace Teste.View
{
    public partial class EditarPerfilCliente : UserControl
    {
        private User usuario;
        private string caminhoFoto = "";

        public EditarPerfilCliente(User user)
        {
            InitializeComponent();

            usuario = user;

            DataContext = usuario;

            // se já tiver foto salva
            caminhoFoto = usuario.FotoPerfil;
            UserRepository repo = new UserRepository();
            repo.Atualizar(usuario);
            CarregarFotoPerfil();
        }

        private void CarregarFotoPerfil()
        {
            try
            {
                if (!string.IsNullOrEmpty(caminhoFoto) &&
                    File.Exists(caminhoFoto))
                {
                    ImagemPerfil.Source = new BitmapImage(new Uri(caminhoFoto));
                }
                else
                {
                    ImagemPerfil.Source = new BitmapImage(
                        new Uri("pack://application:,,,/Dados/imagem/perfil.png"));
                }
            }
            catch { }
        }

        private void AlterarFoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog abrir = new OpenFileDialog
            {
                Filter = "Imagens|*.png;*.jpg;*.jpeg"
            };

            if (abrir.ShowDialog() == true)
            {
                caminhoFoto = abrir.FileName;

                ImagemPerfil.Source = new BitmapImage(new Uri(caminhoFoto));
            }
        }

        private void Salvar_Click(object sender, RoutedEventArgs e)
        {
            usuario.Nome = TxtNome.Text;
            usuario.Email = TxtEmail.Text;
            usuario.Telefone = TxtTelefone.Text;

            if (!string.IsNullOrWhiteSpace(TxtSenha.Password))
            {
                usuario.Senha = TxtSenha.Password;
            }

            // ✔ salva foto no usuário
            usuario.FotoPerfil = caminhoFoto;

            // 🔥 ATUALIZA SESSÃO GLOBAL
          Sessao.UsuarioLogado = usuario;

            // 🔥 ATUALIZA HEADER DA TELA PRINCIPAL
            var janela = Window.GetWindow(this) as TelaPrincipalCliente;

            if (janela != null)
            {
                janela.UpdateUsuario(usuario.Nome);
                janela.AtualizarFoto(usuario.FotoPerfil);
            }

            MessageBox.Show(
                "Perfil atualizado com sucesso!",
                "Sucesso",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}