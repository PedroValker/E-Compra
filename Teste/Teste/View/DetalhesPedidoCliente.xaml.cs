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
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Teste.Model;
using Teste.Repository;

namespace Teste.View
{
    public partial class DetalhesPedidoCliente : Window
    {
        private Pedido _pedidoAtual;

        private Dictionary<string, int> _dicionarioPadrao = new Dictionary<string, int>();
        private Dictionary<string, int> _dicionarioCliente = new Dictionary<string, int>();

        public DetalhesPedidoCliente(Pedido pedido)
        {
            InitializeComponent();
            _pedidoAtual = pedido;

            // Ajuste automático de endereço em memória se estiver vazio
            if (string.IsNullOrWhiteSpace(_pedidoAtual.Endereco) || _pedidoAtual.Endereco.Equals("A combinar", StringComparison.OrdinalIgnoreCase))
            {
                if (Sessao.UsuarioLogado != null && Sessao.UsuarioLogado.Endereco != null)
                {
                    Teste.Model.Endereco end = Sessao.UsuarioLogado.Endereco;
                    _pedidoAtual.Endereco = $"{end.Rua}, nº {end.Numero} - {end.Bairro}";
                }
            }

            this.DataContext = _pedidoAtual;

            if (_pedidoAtual.TipoComposicao == "Modificada")
            {
                BtnMarcarPronta.Visibility = Visibility.Visible;
            }
            else
            {
                BtnMarcarPronta.Visibility = Visibility.Collapsed;
            }

            SystematizarEGradeLogica();
        }

        private void SystematizarEGradeLogica()
        {
            // 1. Estiliza a Tag do Cabeçalho com base no Modelo Customizado
            if (_pedidoAtual.IsModificada)
            {
                BadgeComposicao.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFEDD5"));
                TxtBadge.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#C2410C"));
            }
            else
            {
                BadgeComposicao.Background = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E0F2FE"));
                TxtBadge.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0369A1"));
            }

            _dicionarioPadrao.Clear();
            _dicionarioCliente.Clear();

            var itensFinalTabela = new List<ItemPedido>();
            var itensAdicionados = new List<object>();
            var itensRemovidos = new List<object>();

            // 2. MAPEA A RECEITA PADRÃO DE FÁBRICA
            if (_pedidoAtual.ProdutosOriginaisCesta != null)
            {
                foreach (var itemOrig in _pedidoAtual.ProdutosOriginaisCesta)
                {
                    if (itemOrig == null || string.IsNullOrEmpty(itemOrig.Nome)) continue;
                    string chave = itemOrig.Nome.Trim().ToUpper();

                    int qtdDefasagem = itemOrig.QuantidadeSelecionada > 0 ? itemOrig.QuantidadeSelecionada : 1;

                    if (_dicionarioPadrao.ContainsKey(chave))
                        _dicionarioPadrao[chave] += qtdDefasagem;
                    else
                        _dicionarioPadrao[chave] = qtdDefasagem;
                }
            }

            // 3. MAPEA A COMPOSIÇÃO REAL RECUPERADA DO PEDIDO DO CLIENTE
            if (_pedidoAtual.Itens != null)
            {
                foreach (var itemCli in _pedidoAtual.Itens)
                {
                    if (itemCli == null || string.IsNullOrEmpty(itemCli.Nome)) continue;

                    if (_pedidoAtual.CestaComprada != null && itemCli.Nome.Trim().ToUpper() == _pedidoAtual.CestaComprada.Nome.Trim().ToUpper())
                        continue;

                    string chave = itemCli.Nome.Trim().ToUpper();

                    if (_dicionarioCliente.ContainsKey(chave))
                        _dicionarioCliente[chave] += itemCli.Quantidade;
                    else
                        _dicionarioCliente[chave] = itemCli.Quantidade;
                }
            }

            if (!_dicionarioCliente.Any())
            {
                foreach (var kvp in _dicionarioPadrao)
                {
                    _dicionarioCliente[kvp.Key] = kvp.Value;
                }
            }

            // 4. CONSTRÓI A LISTA DA ESQUERDA
            foreach (var kvp in _dicionarioCliente)
            {
                string nomeFormatado = _pedidoAtual.ProdutosOriginaisCesta?
                    .FirstOrDefault(p => p != null && p.Nome.Trim().ToUpper() == kvp.Key)?.Nome
                    ?? _pedidoAtual.Itens.FirstOrDefault(i => i.Nome.Trim().ToUpper() == kvp.Key)?.Nome
                    ?? kvp.Key;

                itensFinalTabela.Add(new ItemPedido { Nome = nomeFormatado, Quantidade = kvp.Value });
            }

            // 5. CALCULA O DIFFING LOGÍSTICO
            if (_pedidoAtual.IsModificada)
            {
                var todosOsProdutos = _dicionarioPadrao.Keys.Union(_dicionarioCliente.Keys).Distinct();

                foreach (var chaveProduto in todosOsProdutos)
                {
                    _dicionarioPadrao.TryGetValue(chaveProduto, out int qtdPadrao);
                    _dicionarioCliente.TryGetValue(chaveProduto, out int qtdCliente);

                    int delta = qtdCliente - qtdPadrao;

                    string nomeProdutoUI = _pedidoAtual.ProdutosOriginaisCesta?
                        .FirstOrDefault(p => p != null && p.Nome.Trim().ToUpper() == chaveProduto)?.Nome
                        ?? _pedidoAtual.Itens.FirstOrDefault(i => i.Nome.Trim().ToUpper() == chaveProduto)?.Nome
                        ?? chaveProduto;

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

            GridItensFinais.ItemsSource = itensFinalTabela.OrderBy(i => i.Nome).ToList();
            GridAdicionados.ItemsSource = itensAdicionados;
            GridRemovidos.ItemsSource = itensRemovidos;
        }



        // 🚀 ATUALIZADO: Salva o endereço alterado diretamente no TXT ao fechar
        private void Fechar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PedidoRepository repoPedidos = new PedidoRepository();
                repoPedidos.AtualizarArquivoTxt();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar alterações do pedido: " + ex.Message, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            this.Close();
        }

        private void BtnMarcarPronta_Click(object sender, RoutedEventArgs e)
        {
            _pedidoAtual.ModificadaPronta = true;

            PedidoRepository repo = new PedidoRepository();
            repo.AtualizarArquivoTxt();

            MessageBox.Show("Cesta marcada como pronta.");
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
                    iText.Kernel.Colors.Color azulTema = new DeviceRgb(30, 34, 216);
                    iText.Kernel.Colors.Color cinzaEscuro = new DeviceRgb(15, 23, 42);
                    iText.Kernel.Colors.Color cinzaTexto = new DeviceRgb(71, 85, 105);
                    iText.Kernel.Colors.Color cinzaClaro = new DeviceRgb(241, 245, 249);
                    iText.Kernel.Colors.Color verdeSucesso = new DeviceRgb(21, 128, 61);
                    iText.Kernel.Colors.Color vermelhoErro = new DeviceRgb(185, 28, 28);

                    PdfFont fonteNormal = PdfFontFactory.CreateFont(fontNormalPath, PdfEncodings.IDENTITY_H);
                    PdfFont fonteNegrito = PdfFontFactory.CreateFont(fontBoldPath, PdfEncodings.IDENTITY_H);
                    PdfFont fonteItalico = PdfFontFactory.CreateFont(fontItalicPath, PdfEncodings.IDENTITY_H);

                    document.SetFont(fonteNormal);
                    document.SetFontColor(cinzaTexto);

                    document.Add(new Paragraph("COMPROVANTE DE PEDIDO")
                        .SetFontSize(22)
                        .SetFont(fonteNegrito)
                        .SetFontColor(azulTema)
                        .SetMarginBottom(2));

                    document.Add(new Paragraph($"Pedido: {Safe(_pedidoAtual.NomePedido)} | Status: {Safe(_pedidoAtual.Status)} | Composição: {_pedidoAtual.TipoComposicao}")
                        .SetFontSize(11)
                        .SetFontColor(cinzaTexto)
                        .SetMarginBottom(20));

                    Table tabelaInfo = new Table(UnitValue.CreatePercentArray(new float[] { 25, 75 })).UseAllAvailableWidth();

                    tabelaInfo.AddCell(new Cell().Add(new Paragraph("Cliente").SetFont(fonteNegrito)).SetBackgroundColor(cinzaClaro).SetBorder(new iText.Layout.Borders.SolidBorder(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY, 0.5f)));
                    tabelaInfo.AddCell(new Cell().Add(new Paragraph(Safe(_pedidoAtual.Recebedor))).SetBorder(new iText.Layout.Borders.SolidBorder(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY, 0.5f)));

                    // 📍 O endereço alterado pelo usuário na tela já sairá correto aqui no PDF
                    tabelaInfo.AddCell(new Cell().Add(new Paragraph("Endereço").SetFont(fonteNegrito)).SetBackgroundColor(cinzaClaro).SetBorder(new iText.Layout.Borders.SolidBorder(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY, 0.5f)));
                    tabelaInfo.AddCell(new Cell().Add(new Paragraph(Safe(_pedidoAtual.Endereco))).SetBorder(new iText.Layout.Borders.SolidBorder(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY, 0.5f)));

                    tabelaInfo.AddCell(new Cell().Add(new Paragraph("Forma de Pagamento").SetFont(fonteNegrito)).SetBackgroundColor(cinzaClaro).SetBorder(new iText.Layout.Borders.SolidBorder(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY, 0.5f)));
                    tabelaInfo.AddCell(new Cell().Add(new Paragraph(Safe(_pedidoAtual.FormaPagamento))).SetBorder(new iText.Layout.Borders.SolidBorder(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY, 0.5f)));

                    tabelaInfo.AddCell(new Cell().Add(new Paragraph("Previsão de Entrega").SetFont(fonteNegrito)).SetBackgroundColor(cinzaClaro).SetBorder(new iText.Layout.Borders.SolidBorder(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY, 0.5f)));
                    tabelaInfo.AddCell(new Cell().Add(new Paragraph(_pedidoAtual.DataEntrega.HasValue ? _pedidoAtual.DataEntrega.Value.ToString("dd/MM/yyyy") : "Não Agendada")).SetBorder(new iText.Layout.Borders.SolidBorder(iText.Kernel.Colors.ColorConstants.LIGHT_GRAY, 0.5f)));

                    tabelaInfo.SetMarginBottom(25);
                    document.Add(tabelaInfo);

                    document.Add(new Paragraph("Composição Física Final para Montagem")
                        .SetFontSize(14)
                        .SetFont(fonteNegrito)
                        .SetFontColor(cinzaEscuro)
                        .SetMarginBottom(10));

                    Table tabelaItens = new Table(UnitValue.CreatePercentArray(new float[] { 15, 85 })).UseAllAvailableWidth();
                    tabelaItens.AddHeaderCell(new Cell().Add(new Paragraph("Qtd Final").SetFont(fonteNegrito).SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE)).SetBackgroundColor(azulTema));
                    tabelaItens.AddHeaderCell(new Cell().Add(new Paragraph("Produto a Inserir na Caixa").SetFont(fonteNegrito).SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE)).SetBackgroundColor(azulTema));

                    var itensFinaisSeguros = GridItensFinais.ItemsSource as List<ItemPedido>;
                    if (itensFinaisSeguros != null)
                    {
                        foreach (var item in itensFinaisSeguros)
                        {
                            tabelaItens.AddCell(new Cell().Add(new Paragraph(item.Quantidade.ToString()).SetFont(fonteNegrito)).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                            tabelaItens.AddCell(new Cell().Add(new Paragraph(item.Nome)));
                        }
                    }

                    document.Add(tabelaItens);

                    if (_pedidoAtual.IsModificada)
                    {
                        document.Add(new Paragraph("\nHistórico de Customizações (Opcionais):")
                            .SetFont(fonteNegrito).SetFontSize(11).SetFontColor(cinzaEscuro));

                        var todosOsProdutos = _dicionarioPadrao.Keys.Union(_dicionarioCliente.Keys).Distinct();
                        Paragraph pMods = new Paragraph().SetFontSize(10);

                        foreach (var ch in todosOsProdutos)
                        {
                            _dicionarioPadrao.TryGetValue(ch, out int pD);
                            _dicionarioCliente.TryGetValue(ch, out int cD);
                            int diff = cD - pD;

                            string nUI = _pedidoAtual.ProdutosOriginaisCesta?.FirstOrDefault(x => x.Nome.Trim().ToUpper() == ch)?.Nome ?? ch;

                            if (diff > 0) pMods.Add(new Text($"  [+] {nUI} (+{diff})\n").SetFontColor(verdeSucesso).SetFont(fonteItalico));
                            if (diff < 0) pMods.Add(new Text($"  [-] {nUI} (-{Math.Abs(diff)})\n").SetFontColor(vermelhoErro).SetFont(fonteItalico));
                        }
                        document.Add(pMods);
                    }

                    Paragraph pTotal = new Paragraph($"\nVALOR TOTAL DO PEDIDO CONSOLIDADO: R$ {_pedidoAtual.Total:N2}")
                        .SetFont(fonteNegrito).SetFontSize(12).SetFontColor(azulTema).SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT);
                    document.Add(pTotal);

                    if (!string.IsNullOrWhiteSpace(_pedidoAtual.Observacoes))
                    {
                        document.Add(new Paragraph("\nObservações do Pedido:")
                            .SetFont(fonteNegrito).SetFontSize(11).SetMarginTop(10).SetFontColor(cinzaEscuro));
                        document.Add(new Paragraph(Safe(_pedidoAtual.Observacoes)).SetFont(fonteItalico).SetFontSize(10));
                    }
                }

                MessageBox.Show("PDF gerado com sucesso com a lista de separação e conferência!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro ao gerar PDF", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string Safe(object value) => value?.ToString() ?? "";
    }



}