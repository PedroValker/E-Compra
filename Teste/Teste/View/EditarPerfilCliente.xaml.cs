using Microsoft.Win32;
using System;
using System.IO;
using System.Net.Http;
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
            CarregarDadosEndereco();
        }

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
                        imagem.Freeze();
                    }
                    ImagemPerfil.ImageSource = imagem;
                }
                else
                {
                    ImagemPerfil.ImageSource = new BitmapImage(
                        new Uri("pack://application:,,,/Dados/imagem/perfil.png"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao carregar foto de perfil: " + ex.Message);
            }
        }

        private void CarregarDadosEndereco()
        {
            if (usuario.Endereco != null)
            {
                TxtCEP.Text = usuario.Endereco.CEP;
                TxtRua.Text = usuario.Endereco.Rua;
                TxtNumero.Text = usuario.Endereco.Numero;
                TxtBairro.Text = usuario.Endereco.Bairro;
            }
        }

        // 🚀 NOVO: Captura a digitação e consulta a API de CEP automaticamente ao chegar a 8 dígitos
        private async void TxtCEP_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Remove traços ou espaços que o usuário possa digitar
            string cepLimpo = TxtCEP.Text.Replace("-", "").Replace(" ", "").Trim();

            // A API do ViaCEP exige exatamente 8 dígitos numéricos
            if (cepLimpo.Length == 8)
            {
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        // Faz a requisição assíncrona para não congelar o layout do software
                        string url = $"https://viacep.com.br/ws/{cepLimpo}/json/";
                        HttpResponseMessage response = await client.GetAsync(url);

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonResult = await response.Content.ReadAsStringAsync();

                            // Tratamento de string simples para não te obrigar a instalar pacotes adicionais como Newtonsoft.Json
                            if (jsonResult.Contains("\"erro\": true") || jsonResult.Contains("\"erro\":\"true\""))
                            {
                                MessageBox.Show("CEP não encontrado base de dados dos Correios.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                                LimparCamposEndereco(manterCep: true);
                                return;
                            }

                            // Extrai os valores das tags do JSON de forma manual e segura
                            string logradouro = ExtrairValorJson(jsonResult, "logradouro");
                            string bairro = ExtrairValorJson(jsonResult, "bairro");

                            // Injeta os dados direto nas caixas de texto
                            TxtRua.Text = logradouro;
                            TxtBairro.Text = bairro;

                            // Move o foco do teclado direto para o Número para agilizar o preenchimento do cliente
                            TxtNumero.Focus();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Falha de conexão com a API de CEP: " + ex.Message);
                }
            }
        }

        // Método auxiliar para ler o JSON sem dependências externas
        private string ExtrairValorJson(string json, string chave)
        {
            try
            {
                string chaveMapeada = $"\"{chave}\": \"";
                int indexChave = json.IndexOf(chaveMapeada);
                if (indexChave == -1) return "";

                int inicioValor = indexChave + chaveMapeada.Length;
                int fimValor = json.IndexOf("\"", inicioValor);

                return json.Substring(inicioValor, fimValor - inicioValor);
            }
            catch
            {
                return "";
            }
        }

        private void LimparCamposEndereco(bool manterCep)
        {
            if (!manterCep) TxtCEP.Text = "";
            TxtRua.Text = "";
            TxtBairro.Text = "";
            TxtNumero.Text = "";
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
                    ImagemPerfil.ImageSource = imagem;
                }
                catch
                {
                    ImagemPerfil.ImageSource = new BitmapImage(new Uri(caminhoFoto));
                }
            }
        }

        private void Salvar_Click(object sender, RoutedEventArgs e)
        {
            usuario.Nome = TxtNome.Text;
            usuario.Email = TxtEmail.Text;
            usuario.Telefone = TxtTelefone.Text;

        

            usuario.FotoPerfil = caminhoFoto;

            if (usuario.Endereco == null)
            {
                usuario.Endereco = new Endereco();
            }

            usuario.Endereco.CEP = TxtCEP.Text;
            usuario.Endereco.Rua = TxtRua.Text;
            usuario.Endereco.Numero = TxtNumero.Text;
            usuario.Endereco.Bairro = TxtBairro.Text;

            UserRepository repo = new UserRepository();
            repo.Atualizar(usuario);

            caminhoFoto = usuario.FotoPerfil;

            Sessao.UsuarioLogado = usuario;

            var janela = Window.GetWindow(this) as TelaPrincipalCliente;
            if (janela != null)
            {
                janela.UpdateUsuario(usuario.Nome);
                janela.AtualizarFoto(usuario.FotoPerfil);
                janela.AtualizarEnderecoNaTela();
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