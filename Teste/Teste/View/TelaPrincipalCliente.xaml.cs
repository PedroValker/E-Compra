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

            // Guardamos o usuário completo na Sessão para não perder o ID e Endereço!
            Sessao.UsuarioLogado = usuario;

            AtualizarFotoPerfilNaTela();

            ListaCestas = new ObservableCollection<Cesta>();

            this.DataContext = this;

            // Buscamos o nome de dentro do objeto do usuário
            NomeUsuarioText.Text = $"Olá, {usuario.Nome}";

            CarregarCestasDoBanco();

            CarrinhoRepository repoCarrinho = new CarrinhoRepository();
            repoCarrinho.CarregarDoArquivo();

            // 🚀 INJEÇÃO CRÍTICA: Força a verificação e renderização do endereço salvo assim que a tela abre
            AtualizarEnderecoNaTela();

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
                    // Mude para ImageSource
                    ImagemPerfil.ImageSource = new BitmapImage(new Uri("caminho_da_imagem"));
                }
            }
            catch { }
        }
        public void AtualizarEnderecoNaTela()
        {
            // Verifica se o TextBlock com o x:Name="EnderecoTextBlock" existe no XAML
            if (this.FindName("EnderecoTextBlock") is TextBlock textBlockEndereco)
            {
                if (Sessao.UsuarioLogado != null && Sessao.UsuarioLogado.Endereco != null)
                {
                    var end = Sessao.UsuarioLogado.Endereco;

                    // Só monta o endereço se o usuário digitou alguma coisa
                    if (!string.IsNullOrWhiteSpace(end.Rua))
                    {
                        textBlockEndereco.Text = $"{end.Rua}, {end.Numero} - {end.Bairro} ˅";
                    }
                    else
                    {
                        textBlockEndereco.Text = "Nenhum endereço cadastrado ˅";
                    }
                }
                else
                {
                    textBlockEndereco.Text = "Nenhum endereço cadastrado ˅";
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
        private void VoltarParaLoja_Click(object sender, RoutedEventArgs e)
        {
            var janelaPrincipal = Window.GetWindow(this) as Teste.View.TelaPrincipalCliente;
            if (janelaPrincipal != null)
            {
                janelaPrincipal.RetornarParaHome(); // 🚀 Mais limpo e elegante!
            }
        }
        // Método limpo para ser chamado de fora
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