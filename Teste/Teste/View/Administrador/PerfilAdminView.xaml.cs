using iText.StyledXmlParser.Jsoup.Nodes;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Teste.Model;      // 🚀 Ajuste para o seu namespace da Model de Usuários/Sessão
using Teste.Repository; // 🚀 Ajuste para o seu namespace do UserRepository

namespace Teste.View.Administrador
{
    public partial class PerfilAdminView : UserControl
    {
        private string _caminhoFotoTemporaria = "";

        public PerfilAdminView()
        {
            InitializeComponent();
            CarregarDadosDoAdministrador();
        }

        private void CarregarDadosDoAdministrador()
        {
            if (Sessao.UsuarioLogado != null)
            {
                // Preenche os campos de texto com o que já está salvo
                TxtNome.Text = Sessao.UsuarioLogado.Nome;
                TxtEmail.Text = Sessao.UsuarioLogado.Email;
                TxtTelefone.Text = Sessao.UsuarioLogado.Telefone;

                // Carrega a foto na visualização interna se ela existir
                if (!string.IsNullOrEmpty(Sessao.UsuarioLogado.FotoPerfil) && File.Exists(Sessao.UsuarioLogado.FotoPerfil))
                {
                    try
                    {
                        BitmapImage img = new BitmapImage();
                        img.BeginInit();
                        img.CacheOption = BitmapCacheOption.OnLoad;
                        img.UriSource = new Uri(Sessao.UsuarioLogado.FotoPerfil);
                        img.EndInit();
                        ImgPerfilPreview.ImageSource = img;
                        _caminhoFotoTemporaria = Sessao.UsuarioLogado.FotoPerfil;
                    }
                    catch { /* Fallback */ }
                }
            }
        }

        private void BtnAlterarFoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog abrir = new OpenFileDialog();
            abrir.Title = "Escolha uma foto de perfil";
            abrir.Filter = "Arquivos de Imagem|*.png;*.jpg;*.jpeg";

            if (abrir.ShowDialog() == true)
            {
                try
                {
                    BitmapImage imagemNova = new BitmapImage();
                    imagemNova.BeginInit();
                    imagemNova.CacheOption = BitmapCacheOption.OnLoad;
                    imagemNova.UriSource = new Uri(abrir.FileName);
                    imagemNova.EndInit();

                    ImgPerfilPreview.ImageSource = imagemNova;
                    _caminhoFotoTemporaria = abrir.FileName; // Guarda o caminho escolhido temporariamente
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao carregar a imagem: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            if (Sessao.UsuarioLogado != null)
            {
                // Validação simples
                if (string.IsNullOrWhiteSpace(TxtNome.Text))
                {
                    MessageBox.Show("O campo Nome não pode ficar vazio.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 1. Atualiza o objeto na memória da Sessão ativa
                Sessao.UsuarioLogado.Nome = TxtNome.Text;
                Sessao.UsuarioLogado.Email = TxtEmail.Text;
                Sessao.UsuarioLogado.Telefone = TxtTelefone.Text;
                Sessao.UsuarioLogado.FotoPerfil = _caminhoFotoTemporaria;

                MessageBox.Show("Informações salvas com sucesso! As alterações serão aplicadas permanentemente ao fechar o sistema.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

                // Opcional: Força a atualização do nome no menu lateral se a janela mãe estiver aberta
                var janelaPrincipal = Window.GetWindow(this) as Teste.View.PrincipalAdministrador;
                if (janelaPrincipal != null)
                {
                    janelaPrincipal.NomeAdminMenu.Text = $"Olá, {Sessao.UsuarioLogado.Nome}";
                    if (!string.IsNullOrEmpty(_caminhoFotoTemporaria) && File.Exists(_caminhoFotoTemporaria))
                    {
                        janelaPrincipal.ImagemPerfilBrush.ImageSource = ImgPerfilPreview.ImageSource;
                    }
                }
                Sessao.UsuarioLogado.Nome = TxtNome.Text;
                Sessao.UsuarioLogado.Email = TxtEmail.Text;
                Sessao.UsuarioLogado.Telefone = TxtTelefone.Text;
                Sessao.UsuarioLogado.FotoPerfil = _caminhoFotoTemporaria;

                UserRepository repo = new UserRepository();
                repo.Atualizar(Sessao.UsuarioLogado);
                repo.SalvarArquivo();

                MessageBox.Show("Informações salvas com sucesso!");
            }
        }
    }
}