using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; // Necessário para a lista automática
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Teste.Model;
using Teste.Repository;

namespace Teste.View
{
    public partial class CadastroCesta : UserControl
    {
        private Cesta _cestaEmEdicao = null;
        private string caminhoImagemSelecionada = "";

        // 🔥 Mudamos para ObservableCollection para a tela atualizar sozinha e 
        // usamos uma classe que suporte Quantidade (ItemCestaDisplay)
        public ObservableCollection<ItemCestaDisplay> ItensDaCestaAtual { get; set; } = new ObservableCollection<ItemCestaDisplay>();

        public CadastroCesta()
        {
            InitializeComponent();

            // Vincula a lista da tela à nossa coleção inteligente
            ListaItensAtuais.ItemsSource = ItensDaCestaAtual;

            // Carrega os produtos disponíveis no estoque
            ProdutosComboBox.ItemsSource = MemoriaProdutos.Lista;

            AtualizarListaCestas();
        }

        // 🔥 LÓGICA PROFISSIONAL: Adicionar e Agrupar
        private void AdicionarItem_Click(object sender, RoutedEventArgs e)
        {
            Produto produtoSelecionado = ProdutosComboBox.SelectedItem as Produto;

            if (produtoSelecionado != null)
            {
                // 1. Tenta ler a quantidade digitada
                if (!int.TryParse(QuantidadeBox.Text, out int qtd) || qtd <= 0)
                {
                    qtd = 1; // Padrão caso o usuário apague ou digite errado
                }

                // 2. Verifica se o produto JÁ EXISTE na cesta que estamos montando
                var itemExistente = ItensDaCestaAtual.FirstOrDefault(i => i.Nome == produtoSelecionado.Nome);

                if (itemExistente != null)
                {
                    // Se já existe, apenas SOMA a quantidade na mesma linha
                    itemExistente.Quantidade += qtd;
                    ListaItensAtuais.Items.Refresh(); // Força o WPF a mostrar o novo número
                }
                else
                {
                    // Se é novo, adiciona uma nova linha
                    ItensDaCestaAtual.Add(new ItemCestaDisplay
                    {
                        Nome = produtoSelecionado.Nome,
                        Peso = produtoSelecionado.Peso,
                        Preco = produtoSelecionado.Preco,
                        Quantidade = qtd
                    });
                }

                // Reseta o campo de quantidade para 1 para facilitar a próxima inserção
                QuantidadeBox.Text = "1";
            }
            else
            {
                MessageBox.Show("Selecione um produto primeiro!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RemoverItemDaCesta_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ItemCestaDisplay itemClicado)
            {
                ItensDaCestaAtual.Remove(itemClicado);
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

            if (ItensDaCestaAtual.Count == 0)
            {
                MessageBox.Show("A cesta precisa ter pelo menos um produto!", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal.TryParse(PrecoCestaBox.Text, out decimal preco);
            CestaRepository repo = new CestaRepository();

            // Converter nossos itens de "Display" de volta para a lista de Produtos (repetindo conforme a Qtd)
            // se o seu repositório esperar uma lista simples de objetos Produto.
            List<Produto> listaFinalProdutos = new List<Produto>();
            foreach (var item in ItensDaCestaAtual)
            {
                for (int i = 0; i < item.Quantidade; i++)
                {
                    listaFinalProdutos.Add(new Produto { Nome = item.Nome, Peso = item.Peso, Preco = item.Preco });
                }
            }

            if (_cestaEmEdicao != null)
            {
                _cestaEmEdicao.Nome = NomeCestaBox.Text;
                _cestaEmEdicao.Preco = preco;
                if (!string.IsNullOrEmpty(caminhoImagemSelecionada)) _cestaEmEdicao.ImagemPath = caminhoImagemSelecionada;
                _cestaEmEdicao.Itens = listaFinalProdutos;

                repo.AtualizarArquivoTxt();
                MessageBox.Show("Cesta atualizada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                _cestaEmEdicao = null;
            }
            else
            {
                Cesta novaCesta = new Cesta
                {
                    Nome = NomeCestaBox.Text,
                    Preco = preco,
                    ImagemPath = caminhoImagemSelecionada,
                    Itens = listaFinalProdutos
                };

                if (!repo.Salvar(novaCesta, out string erro))
                {
                    MessageBox.Show(erro, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                MessageBox.Show("Cesta criada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LimparCampos();
            AtualizarListaCestas();
        }

        private void EditarCesta_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Cesta cestaClicada)
            {
                _cestaEmEdicao = cestaClicada;
                NomeCestaBox.Text = cestaClicada.Nome;
                PrecoCestaBox.Text = cestaClicada.Preco.ToString();

                ItensDaCestaAtual.Clear();
                // 🔥 Ao editar, agrupamos os produtos que vêm do TXT para a lista da tela
                var grupos = cestaClicada.Itens.GroupBy(p => p.Nome);
                foreach (var g in grupos)
                {
                    var p = g.First();
                    ItensDaCestaAtual.Add(new ItemCestaDisplay
                    {
                        Nome = p.Nome,
                        Peso = p.Peso,
                        Preco = p.Preco,
                        Quantidade = g.Count()
                    });
                }

                caminhoImagemSelecionada = cestaClicada.ImagemPath;
                ImagemPathBox.Text = cestaClicada.ImagemPath;
                try
                {
                    if (!string.IsNullOrEmpty(cestaClicada.ImagemPath) && File.Exists(cestaClicada.ImagemPath))
                        PreviewImagem.Source = new BitmapImage(new Uri(Path.GetFullPath(cestaClicada.ImagemPath)));
                }
                catch { PreviewImagem.Source = null; }
            }
        }

        private void ExcluirCesta_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Cesta cestaClicada)
            {
                if (MessageBox.Show($"Deseja excluir '{cestaClicada.Nome}'?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    MemoriaCestas.Lista.Remove(cestaClicada);
                    new CestaRepository().AtualizarArquivoTxt();
                    AtualizarListaCestas();
                }
            }
        }

        private void VerItens_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Cesta cestaClicada)
            {
                DetalhesCestaWindow popup = new DetalhesCestaWindow(cestaClicada);
                popup.ShowDialog();
            }
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
            ItensDaCestaAtual.Clear();
            caminhoImagemSelecionada = "";
            ImagemPathBox.Clear();
            PreviewImagem.Source = null;
            QuantidadeBox.Text = "1";
        }
    }

    // 🔥 Classe auxiliar para exibir na lista com coluna de Quantidade
    public class ItemCestaDisplay
    {
        public string Nome { get; set; }
        public string Peso { get; set; }
        public decimal Preco { get; set; }
        public int Quantidade { get; set; }
    }
}