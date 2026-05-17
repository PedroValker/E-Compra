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

            // Se já tiver foto salva
            caminhoFoto = usuario.FotoPerfil;

            // 🔥 REMOVIDO DAQUI: O repositório não deve atualizar o TXT ao abrir a tela!

            CarregarFotoPerfil();
        }

        // 🔥 CORREÇÃO: Carregamento usando FileStream seguro (evita travar o arquivo no Windows)
        private void CarregarFotoPerfil()
        {
            try
            {
                if (!string.IsNullOrEmpty(caminhoFoto) && File.Exists(caminhoFoto))
                {
                    BitmapImage imagem = new BitmapImage();
                    using (var stream = new FileStream(caminhoFoto, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        imagem.BeginInit();
                        imagem.CacheOption = BitmapCacheOption.OnLoad;
                        imagem.StreamSource = stream;
                        imagem.EndInit();
                        imagem.Freeze(); // Libera o arquivo para que o Repository consiga copiá-lo
                    }
                    ImagemPerfil.Source = imagem;
                }
                else
                {
                    ImagemPerfil.Source = new BitmapImage(
                        new Uri("pack://application:,,,/Dados/imagem/perfil.png"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao carregar foto de perfil: " + ex.Message);
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

                // Carrega a pré-visualização da foto escolhida também de forma segura
                try
                {
                    BitmapImage imagem = new BitmapImage();
                    using (var stream = new FileStream(caminhoFoto, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        imagem.BeginInit();
                        imagem.CacheOption = BitmapCacheOption.OnLoad;
                        imagem.StreamSource = stream;
                        imagem.EndInit();
                        imagem.Freeze();
                    }
                    ImagemPerfil.Source = imagem;
                }
                catch
                {
                    ImagemPerfil.Source = new BitmapImage(new Uri(caminhoFoto));
                }
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

            // ✔ Salva o caminho temporário ou definitivo da foto no usuário
            usuario.FotoPerfil = caminhoFoto;

            // 🔥 CORRIGIDO: Agora aponta para "Atualizar" com apenas um 'l'
            UserRepository repo = new UserRepository();
            repo.Atuallizar(usuario);

            // A propriedade 'usuario.FotoPerfil' agora contém o caminho definitivo (C:\...) gerado pelo Repository
            caminhoFoto = usuario.FotoPerfil;

            // 🔥 ATUALIZA SESSÃO GLOBAL
            Sessao.UsuarioLogado = usuario;

            // 🔥 ATUALIZA HEADER DA TELA PRINCIPAL
            var janela = Window.GetWindow(this) as TelaPrincipalCliente;
            if (janela != null)
            {
                // Certifique-se de que esses métodos existam na sua TelaPrincipalCliente
                janela.UpdateUsuario(usuario.Nome);
                janela.AtualizarFoto(usuario.FotoPerfil);
            }

            MessageBox.Show(
                "Perfil updated com sucesso!",
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