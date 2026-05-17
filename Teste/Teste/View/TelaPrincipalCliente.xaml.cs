using CestaApp.Views;
using System;
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

        // 🔥 Construtor recebe o objeto User completo
        public TelaPrincipalCliente(User usuario)
        {
            InitializeComponent();

            // Guardamos o usuário completo na Sessão para não perder o ID!
            Sessao.UsuarioLogado = usuario;

            AtualizarFotoPerfilNaTela();

            ListaCestas = new ObservableCollection<Cesta>();

            this.DataContext = this;

            // Buscamos o nome de dentro do objeto do usuário
            NomeUsuarioText.Text = $"Olá, {usuario.Nome}";

            CarregarCestasDoBanco();

            CarrinhoRepository repoCarrinho = new CarrinhoRepository();
            repoCarrinho.CarregarDoArquivo();

            Loaded += TelaPrincipalCliente_Loaded;
        }

        public void UpdateUsuario(string nome)
        {
            NomeUsuarioText.Text = $"Olá, {nome}";
        }

        public void AtualizarFoto(string caminho)
        {
            try
            {
                if (!string.IsNullOrEmpty(caminho) && File.Exists(caminho))
                {
                    BitmapImage imagem = new BitmapImage();
                    using (var stream = new FileStream(caminho, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        imagem.BeginInit();
                        imagem.CacheOption = BitmapCacheOption.OnLoad;
                        imagem.StreamSource = stream;
                        imagem.EndInit();
                        imagem.Freeze();
                    }
                    ImagemPerfil.Source = imagem;
                }
            }
            catch { }
        }

        private void TelaPrincipalCliente_Loaded(object sender, RoutedEventArgs e)
        {
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

        // 🔥 CORRIGIDO: Removido o ShowDialog que quebrava a compilação
        private void EditarPerfil_Click(object sender, RoutedEventArgs e)
        {
            // Instancia o painel passando o usuário da sessão e joga diretamente no centro da tela
            ConteudoPrincipal.Content = new EditarPerfilCliente(Sessao.UsuarioLogado);

            // Nota: Como o UserControl roda acoplado dentro desta tela, a atualização visual da bolinha 
            // e do nome do usuário é feita diretamente pelo método "Salvar_Click" da tela EditarPerfilCliente,
            // que chama "janela.UpdateUsuario()" e "janela.AtualizarFoto()" em tempo real!
        }

        private void AtualizarFotoPerfilNaTela()
        {
            try
            {
                if (Sessao.UsuarioLogado != null &&
                    !string.IsNullOrEmpty(Sessao.UsuarioLogado.FotoPerfil) &&
                    System.IO.File.Exists(Sessao.UsuarioLogado.FotoPerfil))
                {
                    BitmapImage imagem = new BitmapImage();
                    using (var stream = new System.IO.FileStream(Sessao.UsuarioLogado.FotoPerfil, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
                    {
                        imagem.BeginInit();
                        imagem.CacheOption = BitmapCacheOption.OnLoad;
                        imagem.StreamSource = stream;
                        imagem.EndInit();
                        imagem.Freeze();
                    }

                    ImagemPerfil.Source = imagem;
                }
                else
                {
                    ImagemPerfil.Source = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao carregar foto na bolinha: " + ex.Message);
            }
        }

        private void Logoff_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MessageBoxResult resposta = MessageBox.Show(
                "Tem certeza que deseja sair da sua conta?",
                "Confirmação de Logoff",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resposta == MessageBoxResult.Yes)
            {
                Sessao.UsuarioLogado = null;

                var telaLogin = new Login();
                telaLogin.Show();

                this.Close();
            }
        }

        private void VoltarInicio_Click(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = _telaInicial;
        }
    }
}