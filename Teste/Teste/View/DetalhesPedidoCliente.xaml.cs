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

            // Configura o contexto de dados geral da janela (Data, Recebedor, Total, Observações)
            DataContext = pedido;

            var dicionarioPadrao = new Dictionary<string, int>();
            var dicionarioCliente = new Dictionary<string, int>();

            var itensAdicionados = new List<object>();
            var itensRemovidos = new List<object>();

            // Esta lista vai alimentar a tabela da esquerda apenas com os produtos reais
            var linesTabelaEsquerda = new List<object>();

            // 1. DESCOBRE A CESTA ORIGINAL PARA FAZER A COMPARAÇÃO DEPOIS
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

            // 2. MONTA A TABELA DA ESQUERDA (APENAS OS PRODUTOS QUE VÃO NA CESTA)
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
                    string nomeFormatado = pedido.Itens.FirstOrDefault(i => i.Nome?.Trim().ToUpper() == kvp.Key)?.Nome ?? kvp.Key;

                    linesTabelaEsquerda.Add(new
                    {
                        Quantidade = kvp.Value,
                        Nome = nomeFormatado,
                        Subtotal = 0.00
                    });
                }
            }

            // Define a lista de produtos limpa na tabela da Esquerda
            GridCestas.ItemsSource = linesTabelaEsquerda;

            // 3. FAZ O CÁLCULO DE COMPARAÇÃO (DIFFING) PARA AS TABELAS DA DIREITA
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

                    // 🔥 MODIFICADO APENAS AQUI: Se o cliente comprou e alterou itens, mas um produto original
                    // de fábrica não veio listado no TXT do pedido, indica que a quantidade dele foi zerada (Removido).
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

            // Atualiza as tabelas da direita com as modificações realizadas
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
                FileName = $"Pedido_{pedido.NomePedido ?? "SemNome"}.pdf",
                Title = "Salvar Comprovante do Pedido"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 🔥 CORREÇÃO DO PDF CORROMPIDO: Todo o fluxo de gravação estruturado e isolado
                    using (iText.Kernel.Pdf.PdfWriter writer = new iText.Kernel.Pdf.PdfWriter(saveFileDialog.FileName))
                    using (iText.Kernel.Pdf.PdfDocument pdf = new iText.Kernel.Pdf.PdfDocument(writer))
                    using (iText.Layout.Document document = new iText.Layout.Document(pdf))
                    {
                        iText.Kernel.Colors.Color azulTema = new iText.Kernel.Colors.DeviceRgb(43, 108, 176);
                        iText.Kernel.Colors.Color cinzaEscuro = new iText.Kernel.Colors.DeviceRgb(45, 55, 72);
                        iText.Kernel.Colors.Color cinzaClaro = new iText.Kernel.Colors.DeviceRgb(247, 250, 252);
                        iText.Kernel.Colors.Color verdeAdd = new iText.Kernel.Colors.DeviceRgb(47, 133, 90);
                        iText.Kernel.Colors.Color vermelhoRem = new iText.Kernel.Colors.DeviceRgb(155, 44, 44);

                        iText.Kernel.Font.PdfFont fonteNormal = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
                        iText.Kernel.Font.PdfFont fonteNegrito = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_BOLD);
                        iText.Kernel.Font.PdfFont fontItalico = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA_OBLIQUE);

                        document.SetFont(fonteNormal);

                        // --- CABEÇALHO ---
                        iText.Layout.Element.Paragraph titulo = new iText.Layout.Element.Paragraph("COMPROVANTE DE PEDIDO")
                            .SetFontSize(20)
                            .SetFont(fonteNegrito)
                            .SetFontColor(azulTema);
                        document.Add(titulo);

                        iText.Layout.Element.Paragraph subTitulo = new iText.Layout.Element.Paragraph($"ID único: {pedido.NomePedido ?? "N/A"} | Status: {pedido.Status ?? "Pendente"}")
                            .SetFontSize(11)
                            .SetFontColor(cinzaEscuro)
                            .SetMarginBottom(20);
                        document.Add(subTitulo);

                        // --- BLOCO: INFORMAÇÕES GERAIS ---
                        iText.Layout.Element.Paragraph pInfoTitle = new iText.Layout.Element.Paragraph("Informações Gerais")
                            .SetFontSize(13)
                            .SetFont(fonteNegrito)
                            .SetFontColor(azulTema);
                        document.Add(pInfoTitle);

                        iText.Layout.Element.Table tabelaInfo = new iText.Layout.Element.Table(iText.Layout.Properties.UnitValue.CreatePercentArray(new float[] { 25, 75 })).UseAllAvailableWidth();

                        tabelaInfo.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Cliente / Recebedor:").SetFont(fonteNegrito))).SetBackgroundColor(cinzaClaro);
                        tabelaInfo.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph((pedido.Recebedor ?? "Não informado").ToString())));

                        tabelaInfo.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Data do Pedido:").SetFont(fonteNegrito))).SetBackgroundColor(cinzaClaro);
                        tabelaInfo.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph((pedido.DataDoPedido ?? DateTime.Now.ToString("dd/MM/yyyy")).ToString())));

                        tabelaInfo.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Endereço de Entrega:").SetFont(fonteNegrito))).SetBackgroundColor(cinzaClaro);
                        tabelaInfo.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph((pedido.Endereco ?? "A combinar").ToString())));

                        tabelaInfo.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Forma de Pagamento:").SetFont(fonteNegrito))).SetBackgroundColor(cinzaClaro);
                        tabelaInfo.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph((pedido.FormaPagamento ?? "A combinar").ToString())));

                        tabelaInfo.SetMarginBottom(20);
                        document.Add(tabelaInfo);

                        // --- BLOCO: CONTEÚDO FINAL DA CESTA ---
                        iText.Layout.Element.Paragraph pItensTitle = new iText.Layout.Element.Paragraph("Conteúdo Final da Cesta (Para Separação Física)")
                            .SetFontSize(13)
                            .SetFont(fonteNegrito)
                            .SetFontColor(azulTema);
                        document.Add(pItensTitle);

                        iText.Layout.Element.Table tabelaItens = new iText.Layout.Element.Table(iText.Layout.Properties.UnitValue.CreatePercentArray(new float[] { 15, 60, 25 })).UseAllAvailableWidth();

                        tabelaItens.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Qtd").SetFont(fonteNegrito).SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE)).SetBackgroundColor(azulTema).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                        tabelaItens.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Descrição do Item").SetFont(fonteNegrito).SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE)).SetBackgroundColor(azulTema));
                        tabelaItens.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Subtotal").SetFont(fonteNegrito).SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE)).SetBackgroundColor(azulTema).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));

                        if (GridCestas.ItemsSource != null)
                        {
                            foreach (var row in GridCestas.ItemsSource)
                            {
                                var propQtd = row.GetType().GetProperty("Quantidade")?.GetValue(row, null);
                                var propNome = row.GetType().GetProperty("Nome")?.GetValue(row, null);

                                string stringQtd = propQtd != null ? propQtd.ToString() : "1";
                                string stringNome = propNome != null ? propNome.ToString() : "Produto";

                                tabelaItens.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(stringQtd)).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                                tabelaItens.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(stringNome)));
                                tabelaItens.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("R$ 0,00")).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));
                            }
                        }

                        tabelaItens.AddCell(new iText.Layout.Element.Cell(1, 2).Add(new iText.Layout.Element.Paragraph("Valor Total do Pedido:").SetFont(fonteNegrito)).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT).SetBackgroundColor(cinzaClaro));
                        tabelaItens.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph($"R$ {pedido.Total:N2}").SetFont(fonteNegrito)).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT).SetBackgroundColor(cinzaClaro));

                        tabelaItens.SetMarginBottom(20);
                        document.Add(tabelaItens);

                        // --- BLOCO: HISTÓRICO DE MODIFICAÇÕES ---
                        iText.Layout.Element.Paragraph pDiffTitle = new iText.Layout.Element.Paragraph("Histórico de Alterações (Em relação à Cesta Padrão)")
                            .SetFontSize(13)
                            .SetFont(fonteNegrito)
                            .SetFontColor(azulTema);
                        document.Add(pDiffTitle);

                        iText.Layout.Element.Table tabelaDiff = new iText.Layout.Element.Table(iText.Layout.Properties.UnitValue.CreatePercentArray(new float[] { 50, 50 })).UseAllAvailableWidth();

                        // Célula da esquerda: Itens Adicionados
                        iText.Layout.Element.Cell cellAdd = new iText.Layout.Element.Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPaddingRight(5);
                        iText.Layout.Element.Table tAdd = new iText.Layout.Element.Table(iText.Layout.Properties.UnitValue.CreatePercentArray(new float[] { 75, 25 })).UseAllAvailableWidth();
                        tAdd.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Itens Adicionados").SetFont(fonteNegrito).SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE)).SetBackgroundColor(verdeAdd));
                        tAdd.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Qtd").SetFont(fonteNegrito).SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE)).SetBackgroundColor(verdeAdd).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                        bool temAdicionados = false;
                        if (GridAdicionados.ItemsSource != null)
                        {
                            foreach (var row in GridAdicionados.ItemsSource)
                            {
                                var propProd = row.GetType().GetProperty("Produto")?.GetValue(row, null);
                                var propQtdExtra = row.GetType().GetProperty("Qtd")?.GetValue(row, null);

                                if (propProd != null)
                                {
                                    tAdd.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(propProd.ToString())));
                                    tAdd.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(propQtdExtra?.ToString() ?? "+1")).SetFontColor(verdeAdd).SetFont(fonteNegrito).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                                    temAdicionados = true;
                                }
                            }
                        }
                        if (!temAdicionados)
                        {
                            tAdd.AddCell(new iText.Layout.Element.Cell(1, 2).Add(new iText.Layout.Element.Paragraph("Nenhuma alteração avulsa")).SetFont(fontItalico).SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY));
                        }
                        cellAdd.Add(tAdd);
                        tabelaDiff.AddCell(cellAdd);

                        // Célula da direita: Itens Retirados
                        iText.Layout.Element.Cell cellRem = new iText.Layout.Element.Cell().SetBorder(iText.Layout.Borders.Border.NO_BORDER).SetPaddingLeft(5);
                        iText.Layout.Element.Table tRem = new iText.Layout.Element.Table(iText.Layout.Properties.UnitValue.CreatePercentArray(new float[] { 75, 25 })).UseAllAvailableWidth();
                        tRem.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Itens Retirados").SetFont(fonteNegrito).SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE)).SetBackgroundColor(vermelhoRem));
                        tRem.AddHeaderCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph("Qtd").SetFont(fonteNegrito).SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE)).SetBackgroundColor(vermelhoRem).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                        bool temRemovidos = false;
                        if (GridRemovidos.ItemsSource != null)
                        {
                            foreach (var row in GridRemovidos.ItemsSource)
                            {
                                var propProd = row.GetType().GetProperty("Produto")?.GetValue(row, null);
                                var propQtdRet = row.GetType().GetProperty("Qtd")?.GetValue(row, null);

                                if (propProd != null)
                                {
                                    tRem.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(propProd.ToString())));
                                    tRem.AddCell(new iText.Layout.Element.Cell().Add(new iText.Layout.Element.Paragraph(propQtdRet?.ToString() ?? "-1")).SetFontColor(vermelhoRem).SetFont(fonteNegrito).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                                    temRemovidos = true;
                                }
                            }
                        }
                        if (!temRemovidos)
                        {
                            tRem.AddCell(new iText.Layout.Element.Cell(1, 2).Add(new iText.Layout.Element.Paragraph("Nenhum item removido")).SetFont(fontItalico).SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY));
                        }
                        cellRem.Add(tRem);
                        tabelaDiff.AddCell(cellRem);

                        tabelaDiff.SetMarginBottom(20);
                        document.Add(tabelaDiff);

                        // --- BLOCO: OBSERVAÇÕES ---
                        if (!string.IsNullOrEmpty(pedido.Observacoes))
                        {
                            document.Add(new iText.Layout.Element.Paragraph("Observações do Pedido").SetFontSize(13).SetFont(fonteNegrito).SetFontColor(azulTema));
                            iText.Layout.Element.Paragraph obs = new iText.Layout.Element.Paragraph(pedido.Observacoes.ToString())
                                .SetFontSize(10)
                                .SetFont(fontItalico)
                                .SetPadding(8)
                                .SetBackgroundColor(new iText.Kernel.Colors.DeviceRgb(255, 250, 240));
                            document.Add(obs);
                        }
                    } // 🔥 O arquivo PDF fecha fisicamente AQUI de forma segura!

                    // 🔥 AGORA SIM: Exibe o aviso após liberar o documento do iText
                    MessageBox.Show("Documento PDF gerado e salvo com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocorreu um erro ao gerar o PDF: {ex.Message}", "Erro Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}