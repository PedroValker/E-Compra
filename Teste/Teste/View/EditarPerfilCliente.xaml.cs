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

            caminhoFoto = usuario.FotoPerfil;

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
            catch
            {
                // opcional: log de erro
            }
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
            // Atualiza objeto em memória
            usuario.Nome = TxtNome.Text;
            usuario.Email = TxtEmail.Text;
            usuario.Telefone = TxtTelefone.Text;

            if (!string.IsNullOrWhiteSpace(TxtSenha.Password))
                usuario.Senha = TxtSenha.Password;

            usuario.FotoPerfil = caminhoFoto;

            // Atualiza apenas memória (NÃO arquivo aqui)
            UserRepository repo = new UserRepository();
            repo.Atualizar(usuario);

            // Sessão
            Sessao.UsuarioLogado = usuario.Nome;

            // Atualiza UI principal
            var janela = Window.GetWindow(this) as TelaPrincipalCliente;

            if (janela != null)
            {
                janela.AtualizarUsuario(usuario.Nome);
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