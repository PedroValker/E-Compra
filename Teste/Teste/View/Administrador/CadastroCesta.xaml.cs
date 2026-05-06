using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Teste.Model;
using Teste.Repository;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.IO;
using System;

namespace Teste.View
{
    public partial class CadastroCesta : UserControl
    {
        private Cesta _cestaEmEdicao = null;

        private List<Produto> _itensDaCestaAtual = new List<Produto>();

        private string caminhoImagemSelecionada = "";

        public CadastroCesta()
        {
            InitializeComponent();

            ProdutosComboBox.ItemsSource = MemoriaProdutos.Lista;

            AtualizarListaItensAtuais();
            AtualizarListaCestas();
        }

        private void AdicionarItem_Click(object sender, RoutedEventArgs e)
        {
            Produto produtoSelecionado = ProdutosComboBox.SelectedItem as Produto;

            if (produtoSelecionado != null)
            {
                _itensDaCestaAtual.Add(produtoSelecionado);
                AtualizarListaItensAtuais();
            }
            else
            {
                MessageBox.Show("Selecione um produto primeiro!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RemoverItemDaCesta_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Produto produtoClicado = btn.DataContext as Produto;

            if (produtoClicado != null)
            {
                _itensDaCestaAtual.Remove(produtoClicado);
                AtualizarListaItensAtuais();
            }
        }

        private void SelecionarImagem_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Imagens (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

            if (dialog.ShowDialog() == true)
            {
                caminhoImagemSelecionada = dialog.FileName;
                ImagemPathBox.Text = caminhoImagemSelecionada;

                try
                {
                    PreviewImagem.Source = new BitmapImage(new Uri(caminhoImagemSelecionada));
                }
                catch
                {
                    PreviewImagem.Source = null;
                }
            }
        }

        private void SalvarCesta_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NomeCestaBox.Text) || string.IsNullOrWhiteSpace(PrecoCestaBox.Text))
            {
                MessageBox.Show("Preencha o nome e o preço da cesta!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_itensDaCestaAtual.Count == 0)
            {
                MessageBox.Show("A cesta precisa ter pelo menos um produto!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal.TryParse(PrecoCestaBox.Text, out decimal preco);

            CestaRepository repo = new CestaRepository();

            if (_cestaEmEdicao != null)
            {
                // 🔥 EDIÇÃO
                _cestaEmEdicao.Nome = NomeCestaBox.Text;
                _cestaEmEdicao.Preco = preco;

                if (!string.IsNullOrEmpty(caminhoImagemSelecionada))
                {
                    _cestaEmEdicao.ImagemPath = caminhoImagemSelecionada;
                }

                _cestaEmEdicao.Itens = new List<Produto>(_itensDaCestaAtual);

                repo.AtualizarArquivoTxt();

                MessageBox.Show("Cesta atualizada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

                _cestaEmEdicao = null;
            }
            else
            {
                // 🔥 NOVA CESTA
                Cesta novaCesta = new Cesta
                {
                    Nome = NomeCestaBox.Text,
                    Preco = preco,
                    ImagemPath = caminhoImagemSelecionada,
                    Itens = new List<Produto>(_itensDaCestaAtual)
                };

                if (!repo.Salvar(novaCesta, out string erro))
                {
                    MessageBox.Show(erro, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 🔥 Atualiza caminho após cópia da imagem
                caminhoImagemSelecionada = novaCesta.ImagemPath;

                MessageBox.Show("Cesta criada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LimparCampos();
            AtualizarListaCestas();
        }

        private void EditarCesta_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Cesta cestaClicada = btn.DataContext as Cesta;

            if (cestaClicada != null)
            {
                _cestaEmEdicao = cestaClicada;

                NomeCestaBox.Text = cestaClicada.Nome;
                PrecoCestaBox.Text = cestaClicada.Preco.ToString();

                _itensDaCestaAtual = new List<Produto>(cestaClicada.Itens);

                // 🔥 IMAGEM
                caminhoImagemSelecionada = cestaClicada.ImagemPath;
                ImagemPathBox.Text = cestaClicada.ImagemPath;

                try
                {
                    if (!string.IsNullOrEmpty(cestaClicada.ImagemPath) && File.Exists(cestaClicada.ImagemPath))
                    {
                        PreviewImagem.Source = new BitmapImage(new Uri(Path.GetFullPath(cestaClicada.ImagemPath)));
                    }
                    else
                    {
                        PreviewImagem.Source = null;
                    }
                }
                catch
                {
                    PreviewImagem.Source = null;
                }

                AtualizarListaItensAtuais();
            }
        }

        private void ExcluirCesta_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Cesta cestaClicada = btn.DataContext as Cesta;

            if (cestaClicada != null)
            {
                MessageBoxResult resposta = MessageBox.Show(
                    $"Deseja excluir a '{cestaClicada.Nome}'?",
                    "Confirmar",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (resposta == MessageBoxResult.Yes)
                {
                    MemoriaCestas.Lista.Remove(cestaClicada);

                    CestaRepository repo = new CestaRepository();
                    repo.AtualizarArquivoTxt();

                    AtualizarListaCestas();

                    if (_cestaEmEdicao == cestaClicada)
                    {
                        _cestaEmEdicao = null;
                        LimparCampos();
                    }
                }
            }
        }

        private void VerItens_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Cesta cestaClicada = btn.DataContext as Cesta;

            if (cestaClicada != null)
            {
                DetalhesCestaWindow popup = new DetalhesCestaWindow(cestaClicada);
                popup.ShowDialog();
            }
        }

        private void AtualizarListaItensAtuais()
        {
            ListaItensAtuais.ItemsSource = null;
            ListaItensAtuais.ItemsSource = _itensDaCestaAtual;
        }

        private void AtualizarListaCestas()
        {
            ListaCestas.ItemsSource = null;
            ListaCestas.ItemsSource = MemoriaCestas.Lista;
        }

        private void LimparCampos()
        {
            NomeCestaBox.Clear();
            PrecoCestaBox.Clear();
            ProdutosComboBox.SelectedIndex = -1;

            _itensDaCestaAtual.Clear();
            AtualizarListaItensAtuais();

            // 🔥 LIMPA IMAGEM
            caminhoImagemSelecionada = "";
            ImagemPathBox.Clear();
            PreviewImagem.Source = null;
        }
    }
}