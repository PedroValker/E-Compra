using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ObservableCollection<ItemCestaDisplay> ItensDaCestaAtual { get; set; } = new ObservableCollection<ItemCestaDisplay>();

        public CadastroCesta()
        {
            InitializeComponent();

            ListaItensAtuais.ItemsSource = ItensDaCestaAtual;
            ProdutosComboBox.ItemsSource = MemoriaProdutos.Lista;

            AtualizarListaCestas();
        }

        private void RecalcularPrecoSugerido()
        {
            decimal totalSoma = ItensDaCestaAtual.Sum(item => item.Preco * item.Quantidade);
            TxtPrecoSugerido.Text = $"Soma dos Itens: R$ {totalSoma:N2}";

            if (_cestaEmEdicao == null || string.IsNullOrWhiteSpace(PrecoCestaBox.Text))
            {
                PrecoCestaBox.Text = totalSoma.ToString("F2");
            }
        }

        private void AdicionarItem_Click(object sender, RoutedEventArgs e)
        {
            Produto produtoSelecionado = ProdutosComboBox.SelectedItem as Produto;
            if (produtoSelecionado != null)
            {
                if (!int.TryParse(QuantidadeBox.Text, out int qtd) || qtd <= 0) qtd = 1;

                var itemExistente = ItensDaCestaAtual.FirstOrDefault(i => i.Nome == produtoSelecionado.Nome && i.Marca == produtoSelecionado.Marca);
                if (itemExistente != null)
                {
                    itemExistente.Quantidade += qtd;
                    ListaItensAtuais.Items.Refresh();
                }
                else
                {
                    ItensDaCestaAtual.Add(new ItemCestaDisplay
                    {
                        Nome = produtoSelecionado.Nome,
                        Marca = produtoSelecionado.Marca,
                        Peso = produtoSelecionado.Peso,
                        Preco = produtoSelecionado.Preco,
                        Quantidade = qtd
                    });
                }
                QuantidadeBox.Text = "1";
                RecalcularPrecoSugerido();
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
                RecalcularPrecoSugerido();
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

            List<Produto> listaFinalProdutos = new List<Produto>();
            foreach (var item in ItensDaCestaAtual)
            {
                for (int i = 0; i < item.Quantidade; i++)
                {
                    listaFinalProdutos.Add(new Produto
                    {
                        Nome = item.Nome,
                        Marca = item.Marca,
                        Peso = item.Peso,
                        Preco = item.Preco
                    });
                }
            }

            if (_cestaEmEdicao != null)
            {
                _cestaEmEdicao.Nome = NomeCestaBox.Text;
                _cestaEmEdicao.Preco = preco;
                if (!string.IsNullOrEmpty(caminhoImagemSelecionada))
                {
                    _cestaEmEdicao.ImagemPath = caminhoImagemSelecionada;
                }
                _cestaEmEdicao.Itens = listaFinalProdutos;

                if (repo.Salvar(_cestaEmEdicao, out string erroUp))
                {
                    repo.SalvarTudo();
                    MessageBox.Show("Cesta atualizada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    _cestaEmEdicao = null;
                }
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

                repo.SalvarTudo();
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
                PrecoCestaBox.Text = cestaClicada.Preco.ToString("F2");
                ItensDaCestaAtual.Clear();

                var grupos = cestaClicada.Itens.GroupBy(p => new { p.Nome, p.Marca });
                foreach (var g in grupos)
                {
                    var p = g.First();
                    ItensDaCestaAtual.Add(new ItemCestaDisplay
                    {
                        Nome = p.Nome,
                        Marca = p.Marca,
                        Peso = p.Peso,
                        Preco = p.Preco,
                        Quantidade = g.Count() // Ajustado para corresponder à propriedade Quantidade da classe Display
                    });
                }

                caminhoImagemSelecionada = "";
                ImagemPathBox.Text = cestaClicada.ImagemPath;

                PreviewImagem.Source = cestaClicada.ImagemCompleta;
                RecalcularPrecoSugerido();
            }
        }

        private void ExcluirCesta_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Cesta cestaClicada)
            {
                if (MessageBox.Show($"Deseja excluir '{cestaClicada.Nome}'?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    MemoriaCestas.Lista.Remove(cestaClicada);
                    new CestaRepository().SalvarTudo();
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
            TxtPrecoSugerido.Text = "Soma dos Itens: R$ 0,00";
        }
    }

    // 🟢 CLASSE LOCAL MANTIDA PARA EVITAR ERROS DE NAMESPACE
    public class ItemCestaDisplay
    {
        public string Nome { get; set; }
        public string Marca { get; set; } = "";
        public string Peso { get; set; }
        public decimal Preco { get; set; }
        public int Quantidade { get; set; }
    }
}