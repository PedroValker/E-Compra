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

        // Construtor recebe o objeto User completo
        public TelaPrincipalCliente(User usuario)
        {
            InitializeComponent();
            this.Loaded += (s, e) => AtualizarBadgeCarrinho();
            // Guardamos o usuário completo na Sessão para não perder o ID e Endereço!
            Sessao.UsuarioLogado = usuario;

            AtualizarFotoPerfilNaTela();

            ListaCestas = new ObservableCollection<Cesta>();

            this.DataContext = this;

            // === RECUPERAÇÃO DOS DADOS DO USUÁRIO ===
            NomeUsuarioText.Text = $"Olá, {usuario.Nome}";

        

            CarregarCestasDoBanco();

            CarrinhoRepository repoCarrinho = new CarrinhoRepository();
            repoCarrinho.CarregarDoArquivo();

            // Injeta o endereço salvo assim que a tela abre
            AtualizarEnderecoNaTela();

            Loaded += TelaPrincipalCliente_Loaded;
        }

        // Método atualizado para também poder atualizar o email caso seja editado no perfil
        public void UpdateUsuario(string nome, string email = null)
        {
            NomeUsuarioText.Text = $"Olá, {nome}";

        
        }
        // 🔥 COLOQUE ESTE MÉTODO DENTRO DA SUA CLASSE TelaPrincipalCliente
        public void AtualizarBadgeCarrinho()
        {
            // 1. Localiza o botão do carrinho na árvore visual da janela
            Button botaoCarrinhoObjeto = this.FindName("BtnCarrinhoPrincipal") as Button;

            // 2. Verifica se o botão e o template dele existem
            if (botaoCarrinhoObjeto != null && botaoCarrinhoObjeto.Template != null)
            {
                // 3. Força a busca dos componentes ocultos DENTRO do ControlTemplate do botão
                var badgeVisual = botaoCarrinhoObjeto.Template.FindName("BadgeCarrinho", botaoCarrinhoObjeto) as FrameworkElement;
                var textoVisual = botaoCarrinhoObjeto.Template.FindName("TxtQtdBadge", botaoCarrinhoObjeto) as TextBlock;

                // 4. Pega a contagem real e atualizada de itens em memória
                int totalItens = Teste.Model.MemoriaCarrinho.Itens.Count;

                // 5. Aplica a regra visual baseada na quantidade
                if (totalItens > 0)
                {
                    if (badgeVisual != null) badgeVisual.Visibility = Visibility.Visible;
                    if (textoVisual != null) textoVisual.Text = totalItens.ToString();
                }
                else
                {
                    if (badgeVisual != null) badgeVisual.Visibility = Visibility.Collapsed;
                }
            }
        }
        public void AtualizarFoto(string caminho)
        {
            try
            {
                BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                img.CacheOption = BitmapCacheOption.OnLoad;

                if (caminho.StartsWith("pack://"))
                {
                    img.UriSource = new Uri(caminho);
                }
                else
                {
                    img.UriSource = new Uri(caminho, UriKind.Absolute);
                }

                img.EndInit();

                ImagemPerfil.ImageSource = img;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro visual: " + ex.Message);
            }
        }

        public void AtualizarEnderecoNaTela()
        {
            // Busca simplificada direta (evita falhas com o FindName em Grids proporcionais)
            if (EnderecoTextBlock != null)
            {
                if (Sessao.UsuarioLogado != null && Sessao.UsuarioLogado.Endereco != null)
                {
                    var end = Sessao.UsuarioLogado.Endereco;

                    if (!string.IsNullOrWhiteSpace(end.Rua))
                    {
                        EnderecoTextBlock.Text = $"{end.Rua}, {end.Numero} - {end.Bairro} ";
                    }
                    else
                    {
                        EnderecoTextBlock.Text = "Nenhum endereço cadastrado";
                    }
                }
                else
                {
                    EnderecoTextBlock.Text = "Nenhum endereço cadastrado";
                }
            }
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

        private void EditarPerfil_Click(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new EditarPerfilCliente(Sessao.UsuarioLogado);
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

                    ImagemPerfil.ImageSource = imagem;
                }
                else
                {
                    ImagemPerfil.ImageSource = null;
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

        private void VerTodasCestas_Click(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new FacaSeuPedidoView();
        }

        private void VoltarParaLoja_Click(object sender, RoutedEventArgs e)
        {
            var janelaPrincipal = Window.GetWindow(this) as Teste.View.TelaPrincipalCliente;
            if (janelaPrincipal != null)
            {
                janelaPrincipal.RetornarParaHome();
            }
        }

        public void RetornarParaHome()
        {
            ConteudoPrincipal.Content = _telaInicial;
        }

        public void VoltarInicio_Click(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = _telaInicial;
        }
    }
}