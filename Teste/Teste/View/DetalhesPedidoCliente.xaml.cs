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

using Teste.Model;
using Teste.Repository;

namespace Teste.View
{
    public partial class DetalhesPedidoCliente : Window
    {
        public DetalhesPedidoCliente(Pedido pedido)
        {
            InitializeComponent();

            var dicionarioPadrao = new Dictionary<string, int>();
            var dicionarioCliente = new Dictionary<string, int>();

            var itensAdicionados = new List<object>();
            var itensRemovidos = new List<object>();
            var linesTabelaEsquerda = new List<object>();

            // 1. DESCOBRE A CESTA ORIGINAL
            Cesta receitaOriginal = null;

            if (pedido.Observacoes != null)
            {
                foreach (var cestaCadastrada in MemoriaCestas.Lista)
                {
                    if (cestaCadastrada.Nome != null &&
                        pedido.Observacoes.Trim().ToUpper().Contains(cestaCadastrada.Nome.Trim().ToUpper()))
                    {
                        receitaOriginal = cestaCadastrada;
                        break;
                    }
                }
            }

            if (receitaOriginal == null && pedido.Itens != null && pedido.Itens.Any())
            {
                receitaOriginal = MemoriaCestas.Lista.FirstOrDefault(c =>
                    c.Itens != null && c.Itens.Any(i => i != null && i.Nome.Trim().ToUpper() == pedido.Itens.First().Nome.Trim().ToUpper()));
            }

            if (receitaOriginal == null)
            {
                receitaOriginal = MemoriaCestas.Lista.FirstOrDefault();
            }

            // 2. MONTA A TABELA DA ESQUERDA (COM RECALCULO DE TOTAL DINÂMICO)
            decimal acumuladorTotalMapeado = 0;

            if (pedido.Itens != null && pedido.Itens.Any())
            {
                foreach (var item in pedido.Itens)
                {
                    if (string.IsNullOrEmpty(item.Nome)) continue;
                    string chave = item.Nome.Trim().ToUpper();
                    int qtd = item.Quantidade > 0 ? item.Quantidade : 1;

                    if (dicionarioCliente.ContainsKey(chave))
                        dicionarioCliente[chave] += qtd;
                    else
                        dicionarioCliente[chave] = qtd;
                }

                foreach (var kvp in dicionarioCliente)
                {
                    var itemOriginal = pedido.Itens.FirstOrDefault(i => i.Nome?.Trim().ToUpper() == kvp.Key);
                    string nomeFormatado = itemOriginal?.Nome ?? kvp.Key;

                    decimal precoUnitario = 0;

                    // 1ª Tentativa: Busca o preço do produto dentro da receita padrão associada
                    if (receitaOriginal != null && receitaOriginal.Itens != null)
                    {
                        var produtoCadastrado = receitaOriginal.Itens.FirstOrDefault(p => p != null && p.Nome.Trim().ToUpper() == kvp.Key);
                        if (produtoCadastrado != null)
                        {
                            precoUnitario = produtoCadastrado.Preco;
                        }
                    }

                    // 2ª Tentativa (Garantia): Se o produto foi adicionado como extra e não existe na receita original,
                    // varre todas as cestas cadastradas no sistema em busca do preço de mercado do produto
                    if (precoUnitario == 0)
                    {
                        var produtoGlobal = MemoriaCestas.Lista
                            .Where(c => c.Itens != null)
                            .SelectMany(c => c.Itens)
                            .FirstOrDefault(p => p != null && p.Nome.Trim().ToUpper() == kvp.Key);

                        if (produtoGlobal != null)
                        {
                            precoUnitario = produtoGlobal.Preco;
                        }
                    }

                    decimal subtotal = precoUnitario * kvp.Value;
                    acumuladorTotalMapeado += subtotal;

                    linesTabelaEsquerda.Add(new
                    {
                        Quantidade = kvp.Value,
                        Nome = nomeFormatado,
                        Subtotal = subtotal
                    });
                }
            }

            // 🔥 ATUALIZAÇÃO CRÍTICA: Se a soma dos produtos mapeados for maior que zero,
            // força o objeto pedido a assumir o valor recalculado com base nos preços reais do banco
            if (acumuladorTotalMapeado > 0)
            {
                pedido.Total = acumuladorTotalMapeado;
            }

            // Vincula o DataContext após aplicar as correções de preço e totais
            DataContext = pedido;
            GridCestas.ItemsSource = linesTabelaEsquerda;

            // 3. FAZ O CÁLCULO DE COMPARAÇÃO (DIFFING)
            if (receitaOriginal != null && receitaOriginal.Itens != null)
            {
                foreach (var itemOriginal in receitaOriginal.Itens)
                {
                    if (itemOriginal == null || string.IsNullOrEmpty(itemOriginal.Nome)) continue;
                    string chave = itemOriginal.Nome.Trim().ToUpper();

                    if (dicionarioPadrao.ContainsKey(chave))
                        dicionarioPadrao[chave]++;
                    else
                        dicionarioPadrao[chave] = 1;
                }

                var todosOsProdutos = dicionarioPadrao.Keys.Union(dicionarioCliente.Keys).Distinct();

                foreach (var chaveProduto in todosOsProdutos)
                {
                    dicionarioPadrao.TryGetValue(chaveProduto, out int qtdPadrao);
                    dicionarioCliente.TryGetValue(chaveProduto, out int qtdCliente);

                    if (dicionarioCliente.Any() && !dicionarioCliente.ContainsKey(chaveProduto) && dicionarioPadrao.ContainsKey(chaveProduto))
                    {
                        qtdCliente = 0;
                    }

                    int delta = qtdCliente - qtdPadrao;

                    string nomeProdutoUI = receitaOriginal.Itens.FirstOrDefault(p => p != null && p.Nome.Trim().ToUpper() == chaveProduto)?.Nome;
                    if (string.IsNullOrEmpty(nomeProdutoUI))
                    {
                        var itemDoPedido = pedido.Itens.FirstOrDefault(p => p != null && p.Nome.Trim().ToUpper() == chaveProduto);
                        nomeProdutoUI = itemDoPedido != null ? itemDoPedido.Nome : chaveProduto;
                    }

                    if (delta > 0)
                    {
                        itensAdicionados.Add(new { Produto = nomeProdutoUI, Qtd = $"+{delta}" });
                    }
                    else if (delta < 0)
                    {
                        itensRemovidos.Add(new { Produto = nomeProdutoUI, Qtd = $"-{Math.Abs(delta)}" });
                    }
                }
            }

            GridAdicionados.ItemsSource = itensAdicionados;
            GridRemovidos.ItemsSource = itensRemovidos;
        }

        private void Fechar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void GerarPdf_Click(object sender, RoutedEventArgs e)
        {
            if (!(this.DataContext is Model.Pedido pedido))
            {
                MessageBox.Show("Erro ao recuperar os dados do pedido.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Arquivos PDF (*.pdf)|*.pdf",
                FileName = $"Pedido_{Safe(pedido.NomePedido)}.pdf",
                Title = "Salvar Comprovante do Pedido"
            };

            if (saveFileDialog.ShowDialog() != true)
                return;

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

                    // TÍTULO
                    Paragraph titulo = new Paragraph("COMPROVANTE DE PEDIDO").SetFontSize(20).SetFont(fonteNegrito).SetFontColor(azulTema);
                    document.Add(titulo);

                    Paragraph subTitulo = new Paragraph($"Pedido: {Safe(pedido.NomePedido)} | Status: {Safe(pedido.Status)}").SetFontSize(11).SetFontColor(cinzaEscuro).SetMarginBottom(20);
                    document.Add(subTitulo);

                    // INFORMAÇÕES GERAIS
                    Paragraph infoTitulo = new Paragraph("Informações Gerais").SetFontSize(13).SetFont(fonteNegrito).SetFontColor(azulTema);
                    document.Add(infoTitulo);

                    Table tabelaInfo = new Table(UnitValue.CreatePercentArray(new float[] { 25, 75 })).UseAllAvailableWidth();
                    tabelaInfo.AddCell(new Cell().Add(new Paragraph("Cliente").SetFont(fonteNegrito)).SetBackgroundColor(cinzaClaro));
                    tabelaInfo.AddCell(new Cell().Add(new Paragraph(Safe(pedido.Recebedor))));

                    string dataPedido;
                    try
                    {
                        dataPedido = pedido.DataDoPedido != null ? Convert.ToDateTime(pedido.DataDoPedido).ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy");
                    }
                    catch
                    {
                        dataPedido = DateTime.Now.ToString("dd/MM/yyyy");
                    }

                    tabelaInfo.AddCell(new Cell().Add(new Paragraph("Data").SetFont(fonteNegrito)).SetBackgroundColor(cinzaClaro));
                    tabelaInfo.AddCell(new Cell().Add(new Paragraph(dataPedido)));
                    tabelaInfo.AddCell(new Cell().Add(new Paragraph("Endereço").SetFont(fonteNegrito)).SetBackgroundColor(cinzaClaro));
                    tabelaInfo.AddCell(new Cell().Add(new Paragraph(Safe(pedido.Endereco))));
                    tabelaInfo.AddCell(new Cell().Add(new Paragraph("Pagamento").SetFont(fonteNegrito)).SetBackgroundColor(cinzaClaro));
                    tabelaInfo.AddCell(new Cell().Add(new Paragraph(Safe(pedido.FormaPagamento))));
                    tabelaInfo.SetMarginBottom(20);
                    document.Add(tabelaInfo);

                    // TABELA DE ITENS
                    Paragraph itensTitulo = new Paragraph("Itens do Pedido").SetFontSize(13).SetFont(fonteNegrito).SetFontColor(azulTema);
                    document.Add(itensTitulo);

                    Table tabelaItens = new Table(UnitValue.CreatePercentArray(new float[] { 15, 60, 25 })).UseAllAvailableWidth();

                    tabelaItens.AddHeaderCell(new Cell().Add(new Paragraph("Qtd").SetFont(fonteNegrito).SetFontColor(ColorConstants.WHITE)).SetBackgroundColor(azulTema).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                    tabelaItens.AddHeaderCell(new Cell().Add(new Paragraph("Descrição").SetFont(fonteNegrito).SetFontColor(ColorConstants.WHITE)).SetBackgroundColor(azulTema));
                    tabelaItens.AddHeaderCell(new Cell().Add(new Paragraph("Subtotal").SetFont(fonteNegrito).SetFontColor(ColorConstants.WHITE)).SetBackgroundColor(azulTema).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));

                    if (GridCestas.ItemsSource != null)
                    {
                        foreach (var row in GridCestas.ItemsSource.Cast<object>().Where(x => x != null))
                        {
                            try
                            {
                                var propQtd = row.GetType().GetProperty("Quantidade")?.GetValue(row);
                                var propNome = row.GetType().GetProperty("Nome")?.GetValue(row);
                                var propSubtotal = row.GetType().GetProperty("Subtotal")?.GetValue(row);

                                string qtd = Safe(propQtd);
                                string nome = Safe(propNome);

                                decimal subtotalDecimal = propSubtotal is decimal value ? value : 0;

                                if (string.IsNullOrWhiteSpace(qtd)) qtd = "1";
                                if (string.IsNullOrWhiteSpace(nome)) nome = "Produto";

                                tabelaItens.AddCell(new Cell().Add(new Paragraph(qtd)).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                                tabelaItens.AddCell(new Cell().Add(new Paragraph(nome)));
                                tabelaItens.AddCell(new Cell().Add(new Paragraph($"R$ {subtotalDecimal:N2}")).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
                            }
                            catch
                            {
                            }
                        }
                    }

                    // TOTAL DO PEDIDO
                    tabelaItens.AddCell(new Cell(1, 2).Add(new Paragraph("Valor Total:").SetFont(fonteNegrito)).SetBackgroundColor(cinzaClaro).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
                    tabelaItens.AddCell(new Cell().Add(new Paragraph($"R$ {pedido.Total:N2}").SetFont(fonteNegrito)).SetBackgroundColor(cinzaClaro).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
                    tabelaItens.SetMarginBottom(20);
                    document.Add(tabelaItens);

                    // OBSERVAÇÕES
                    if (!string.IsNullOrWhiteSpace(Safe(pedido.Observacoes)))
                    {
                        Paragraph tituloObs = new Paragraph("Observações").SetFontSize(13).SetFont(fonteNegrito).SetFontColor(azulTema);
                        document.Add(tituloObs);

                        Paragraph obs = new Paragraph(Safe(pedido.Observacoes)).SetFont(fonteItalico).SetFontSize(10).SetPadding(8).SetBackgroundColor(new DeviceRgb(255, 250, 240));
                        document.Add(obs);
                    }
                }

                MessageBox.Show("PDF gerado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Erro Completo", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string Safe(object value)
        {
            return value?.ToString() ?? "";
        }
    }
}