using iText.IO.Font;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Teste.Model;
using Teste.Repository;

namespace Teste.View
{
    public partial class DetalhesPedidoCliente : Window
    {
        private Pedido _pedidoAtual;
        private List<CestaInfoExibicao> _cestasMapeadas;

        // Classe auxiliar interna para organizar a tabela da esquerda de forma limpa
        public class CestaInfoExibicao
        {
            public int Quantidade { get; set; }
            public string Nome { get; set; }
            public decimal Subtotal { get; set; }
            public Cesta ReceitaOriginal { get; set; }
            public List<ItemPedido> ItensModificados { get; set; }
        }

        public DetalhesPedidoCliente(Pedido pedido)
        {
            InitializeComponent();
            _pedidoAtual = pedido;

            // 🚀 CORREÇÃO DO ENDEREÇO EM MEMÓRIA
            if (string.IsNullOrWhiteSpace(_pedidoAtual.Endereco) || _pedidoAtual.Endereco.Equals("A combinar", StringComparison.OrdinalIgnoreCase))
            {
                if (Sessao.UsuarioLogado != null && Sessao.UsuarioLogado.Endereco != null)
                {
                    Teste.Model.Endereco end = Sessao.UsuarioLogado.Endereco;
                    _pedidoAtual.Endereco = $"{end.Rua}, nº {end.Numero} - {end.Bairro}";
                }
            }

            this.DataContext = _pedidoAtual;

            // Vincular o evento de clique na tabela da esquerda
            GridCestas.SelectionChanged += GridCestas_SelectionChanged;

            ProcessarEAgruparCestas();
        }

        private void ProcessarEAgruparCestas()
        {
            _cestasMapeadas = new List<CestaInfoExibicao>();

            if (_pedidoAtual.Itens == null || !_pedidoAtual.Itens.Any()) return;

            // 🚀 O SEGREDO DO AGRUPAMENTO:
            // Vamos identificar o que é Cesta Principal. Se o seu sistema salva os produtos modificados 
            // logo abaixo da cesta ou se agrupa por nome, vamos separar o que é a Cesta Mãe.
            var agrupamentoCestas = _pedidoAtual.Itens
                .Where(i => MemoriaCestas.Lista.Any(c => c.Nome.Trim().ToUpper() == i.Nome.Trim().ToUpper()))
                .ToList();

            // Se o arquivo antigo não tiver a estrutura separada, tratamos o primeiro item como a cesta principal
            if (!agrupamentoCestas.Any())
            {
                var primeiro = _pedidoAtual.Itens.First();
                var cestaFallback = MemoriaCestas.Lista.FirstOrDefault(c => c.Nome.Trim().ToUpper() == primeiro.Nome.Trim().ToUpper())
                                    ?? MemoriaCestas.Lista.FirstOrDefault();

                if (cestaFallback != null)
                {
                    _cestasMapeadas.Add(new CestaInfoExibicao
                    {
                        Quantidade = primeiro.Quantidade,
                        Nome = cestaFallback.Nome,
                        Subtotal = cestaFallback.Preco * primeiro.Quantidade,
                        ReceitaOriginal = cestaFallback,
                        ItensModificados = _pedidoAtual.Itens.ToList()
                    });
                }
            }
            else
            {
                foreach (var itemCesta in agrupamentoCestas)
                {
                    var cestaOriginal = MemoriaCestas.Lista.First(c => c.Nome.Trim().ToUpper() == itemCesta.Nome.Trim().ToUpper());

                    // Captura os itens modificados pertencentes a esta cesta específica (ou os produtos do próprio pedido)
                    var produtosDestaCesta = _pedidoAtual.Itens
                        .Where(i => !MemoriaCestas.Lista.Any(c => c.Nome.Trim().ToUpper() == i.Nome.Trim().ToUpper()))
                        .ToList();

                    // Se não houver sub-produtos modificados no pedido, usamos a receita padrão da fábrica
                    if (!produtosDestaCesta.Any())
                    {
                        produtosDestaCesta = cestaOriginal.Itens.Select(p => new ItemPedido
                        {
                            Nome = p.Nome,
                            Quantidade = p.QuantidadeSelecionada > 0 ? p.QuantidadeSelecionada : 1
                        }).ToList();
                    }

                    _cestasMapeadas.Add(new CestaInfoExibicao
                    {
                        Quantidade = itemCesta.Quantidade,
                        Nome = itemCesta.Nome,
                        Subtotal = cestaOriginal.Preco * itemCesta.Quantidade,
                        ReceitaOriginal = cestaOriginal,
                        ItensModificados = produtosDestaCesta
                    }
                    );
                }
            }

            // Atualiza o total se houver divergência (Apenas em memória)
            decimal totalRecalculado = _cestasMapeadas.Sum(c => c.Subtotal);
            if (totalRecalculado > 0)
            {
                _pedidoAtual.Total = totalRecalculado;
            }

            GridCestas.ItemsSource = _cestasMapeadas;

            // Seleciona automaticamente a primeira cesta da lista para não abrir o lado direito em branco
            if (_cestasMapeadas.Any())
            {
                GridCestas.SelectedIndex = 0;
            }
        }

        // 🔄 EVENTO DE SELEÇÃO: Roda o Diffing (Comparação) dinamicamente para a cesta que o Admin clicar
        private void GridCestas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridCestas.SelectedItem is CestaInfoExibicao cestaSelecionada)
            {
                var dicionarioPadrao = new Dictionary<string, int>();
                var dicionarioCliente = new Dictionary<string, int>();
                var itensAdicionados = new List<object>();
                var itensRemovidos = new List<object>();

                // Mapeia a receita original da fábrica
                if (cestaSelecionada.ReceitaOriginal != null && cestaSelecionada.ReceitaOriginal.Itens != null)
                {
                    foreach (var itemOrig in cestaSelecionada.ReceitaOriginal.Itens)
                    {
                        if (itemOrig == null || string.IsNullOrEmpty(itemOrig.Nome)) continue;
                        string chave = itemOrig.Nome.Trim().ToUpper();
                        dicionarioPadrao[chave] = itemOrig.QuantidadeSelecionada > 0 ? itemOrig.QuantidadeSelecionada : 1;
                    }
                }

                // Mapeia o que o cliente de fato levou nessa execução
                if (cestaSelecionada.ItensModificados != null)
                {
                    foreach (var itemCli in cestaSelecionada.ItensModificados)
                    {
                        if (itemCli == null || string.IsNullOrEmpty(itemCli.Nome)) continue;
                        string chave = itemCli.Nome.Trim().ToUpper();
                        dicionarioCliente[chave] = itemCli.Quantidade;
                    }
                }

                var todosOsProdutos = dicionarioPadrao.Keys.Union(dicionarioCliente.Keys).Distinct();

                foreach (var chaveProduto in todosOsProdutos)
                {
                    dicionarioPadrao.TryGetValue(chaveProduto, out int qtdPadrao);
                    dicionarioCliente.TryGetValue(chaveProduto, out int qtdCliente);

                    int delta = qtdCliente - qtdPadrao;

                    string nomeProdutoUI = cestaSelecionada.ReceitaOriginal?.Itens?
                        .FirstOrDefault(p => p != null && p.Nome.Trim().ToUpper() == chaveProduto)?.Nome ?? chaveProduto;

                    if (delta > 0)
                    {
                        itensAdicionados.Add(new { Produto = nomeProdutoUI, Qtd = $"+{delta}" });
                    }
                    else if (delta < 0)
                    {
                        itensRemovidos.Add(new { Produto = nomeProdutoUI, Qtd = $"-{Math.Abs(delta)}" });
                    }
                }

                GridAdicionados.ItemsSource = itensAdicionados;
                GridRemovidos.ItemsSource = itensRemovidos;
            }
        }

        private void Fechar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void GerarPdf_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Arquivos PDF (*.pdf)|*.pdf",
                FileName = $"Pedido_{Safe(_pedidoAtual.NomePedido)}.pdf",
                Title = "Salvar Comprovante do Pedido"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            try
            {
                string fontNormalPath = @"C:\Windows\Fonts\arial.ttf";
                string fontBoldPath = @"C:\Windows\Fonts\arialbd.ttf";
                string fontItalicPath = @"C:\Windows\Fonts\ariali.ttf";

                using (PdfWriter writer = new PdfWriter(saveFileDialog.FileName))
                using (PdfDocument pdf = new PdfDocument(writer))
                using (Document document = new Document(pdf))
                {
                    Color azulTema = new DeviceRgb(43, 108, 176);
                    Color cinzaEscuro = new DeviceRgb(45, 55, 72);
                    Color cinzaClaro = new DeviceRgb(247, 250, 252);

                    PdfFont fonteNormal = PdfFontFactory.CreateFont(fontNormalPath, PdfEncodings.IDENTITY_H);
                    PdfFont fonteNegrito = PdfFontFactory.CreateFont(fontBoldPath, PdfEncodings.IDENTITY_H);
                    PdfFont fonteItalico = PdfFontFactory.CreateFont(fontItalicPath, PdfEncodings.IDENTITY_H);

                    document.SetFont(fonteNormal);

                    document.Add(new Paragraph("COMPROVANTE DE PEDIDO").SetFontSize(20).SetFont(fonteNegrito).SetFontColor(azulTema));
                    document.Add(new Paragraph($"Pedido: {Safe(_pedidoAtual.NomePedido)} | Status: {Safe(_pedidoAtual.Status)}").SetFontSize(11).SetFontColor(cinzaEscuro).SetMarginBottom(20));

                    Table tabelaInfo = new Table(UnitValue.CreatePercentArray(new float[] { 25, 75 })).UseAllAvailableWidth();
                    tabelaInfo.AddCell(new Cell().Add(new Paragraph("Cliente").SetFont(fonteNegrito)).SetBackgroundColor(cinzaClaro));
                    tabelaInfo.AddCell(new Cell().Add(new Paragraph(Safe(_pedidoAtual.Recebedor))));
                    tabelaInfo.AddCell(new Cell().Add(new Paragraph("Endereço").SetFont(fonteNegrito)).SetBackgroundColor(cinzaClaro));
                    tabelaInfo.AddCell(new Cell().Add(new Paragraph(Safe(_pedidoAtual.Endereco))));
                    tabelaInfo.AddCell(new Cell().Add(new Paragraph("Previsão de Entrega").SetFont(fonteNegrito)).SetBackgroundColor(cinzaClaro));
                    tabelaInfo.AddCell(new Cell().Add(new Paragraph(_pedidoAtual.DataEntrega.HasValue ? _pedidoAtual.DataEntrega.Value.ToString("dd/MM/yyyy") : "Não Agendada")));
                    tabelaInfo.SetMarginBottom(20);
                    document.Add(tabelaInfo);

                    document.Add(new Paragraph("Resumo das Cestas").SetFontSize(13).SetFont(fonteNegrito).SetFontColor(azulTema));
                    Table tabelaItens = new Table(UnitValue.CreatePercentArray(new float[] { 15, 60, 25 })).UseAllAvailableWidth();
                    tabelaItens.AddHeaderCell(new Cell().Add(new Paragraph("Qtd").SetFont(fonteNegrito).SetFontColor(ColorConstants.WHITE)).SetBackgroundColor(azulTema));
                    tabelaItens.AddHeaderCell(new Cell().Add(new Paragraph("Cesta").SetFont(fonteNegrito).SetFontColor(ColorConstants.WHITE)).SetBackgroundColor(azulTema));
                    tabelaItens.AddHeaderCell(new Cell().Add(new Paragraph("Subtotal").SetFont(fonteNegrito).SetFontColor(ColorConstants.WHITE)).SetBackgroundColor(azulTema));

                    foreach (var cesta in _cestasMapeadas)
                    {
                        tabelaItens.AddCell(new Cell().Add(new Paragraph(cesta.Quantidade.ToString())).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                        tabelaItens.AddCell(new Cell().Add(new Paragraph(cesta.Nome)));
                        tabelaItens.AddCell(new Cell().Add(new Paragraph($"R$ {cesta.Subtotal:N2}")).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
                    }

                    tabelaItens.AddCell(new Cell(1, 2).Add(new Paragraph("Valor Total:").SetFont(fonteNegrito)).SetBackgroundColor(cinzaClaro).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
                    tabelaItens.AddCell(new Cell().Add(new Paragraph($"R$ {_pedidoAtual.Total:N2}").SetFont(fonteNegrito)).SetBackgroundColor(cinzaClaro).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
                    document.Add(tabelaItens);
                }

                MessageBox.Show("PDF gerado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro ao gerar PDF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string Safe(object value) => value?.ToString() ?? "";
    }
}