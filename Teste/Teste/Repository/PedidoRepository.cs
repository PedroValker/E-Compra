using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Teste.Model;

namespace Teste.Repository
{
    public class PedidoRepository
    {
        private string ObterCaminhoArquivo()
        {
            string pastaProjeto = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
            return Path.Combine(pastaProjeto, "Dados", "pedidos.txt");
        }

        // 🔥 MÉTODO AUXILIAR CORRIGIDO: Varre o carrinho e agrupa perfeitamente as alterações da UI
        private string GerarStringDosItens(Pedido p)
        {
            var dicionarioAgrupado = new Dictionary<string, int>();

            // ESTRATÉGIA 1: Se o pedido já possui os sub-itens por extenso explicitamente em p.Itens
            if (p.Itens != null && p.Itens.Count > 1)
            {
                foreach (var item in p.Itens)
                {
                    if (string.IsNullOrEmpty(item.Nome)) continue;
                    string nome = item.Nome.Trim();
                    int qtd = item.Quantidade > 0 ? item.Quantidade : 1;

                    if (dicionarioAgrupado.ContainsKey(nome))
                        dicionarioAgrupado[nome] += qtd;
                    else
                        dicionarioAgrupado[nome] = qtd;
                }
            }
            // ESTRATÉGIA 2: INTERCEPTADOR GLOBAL - Pega os produtos gerados pelo laço 'for' do botão Adicionar ao Carrinho
            else if (p.CestaComprada != null)
            {
                // Busca o item correspondente dentro da MemoriaCarrinho que foi preenchida na CestaView
                var itemNoCarrinho = MemoriaCarrinho.Itens.FirstOrDefault(c =>
                    c.CestaSelecionada != null &&
                    c.CestaSelecionada.Nome.Trim().ToUpper() == p.CestaComprada.Nome.Trim().ToUpper());

                if (itemNoCarrinho != null && itemNoCarrinho.CestaSelecionada.Itens != null && itemNoCarrinho.CestaSelecionada.Itens.Any())
                {
                    foreach (var prod in itemNoCarrinho.CestaSelecionada.Itens)
                    {
                        if (string.IsNullOrEmpty(prod.Nome)) continue;
                        string nome = prod.Nome.Trim();

                        // Como cada item da lista final clonada foi inserido um por um com valor 1, agrupamos somando
                        int qtdReal = prod.QuantidadeSelecionada > 0 ? prod.QuantidadeSelecionada : 1;

                        if (dicionarioAgrupado.ContainsKey(nome))
                            dicionarioAgrupado[nome] += qtdReal;
                        else
                            dicionarioAgrupado[nome] = qtdReal;
                    }
                }
                // Fallback local: Se não encontrou na memória global do carrinho, tenta ler da árvore do parâmetro
                else if (p.CestaComprada.Itens != null && p.CestaComprada.Itens.Any())
                {
                    foreach (var prod in p.CestaComprada.Itens)
                    {
                        if (string.IsNullOrEmpty(prod.Nome)) continue;
                        string nome = prod.Nome.Trim();
                        int qtdReal = prod.QuantidadeSelecionada > 0 ? prod.QuantidadeSelecionada : 1;

                        if (dicionarioAgrupado.ContainsKey(nome))
                            dicionarioAgrupado[nome] += qtdReal;
                        else
                            dicionarioAgrupado[nome] = qtdReal;
                    }
                }
            }

            // ESTRATÉGIA 3: Fallback de segurança para listas simples de 1 item
            if (!dicionarioAgrupado.Any() && p.Itens != null && p.Itens.Any())
            {
                foreach (var item in p.Itens)
                {
                    if (string.IsNullOrEmpty(item.Nome)) continue;
                    string nome = item.Nome.Trim();
                    int qtd = item.Quantidade > 0 ? item.Quantidade : 1;

                    if (dicionarioAgrupado.ContainsKey(nome))
                        dicionarioAgrupado[nome] += qtd;
                    else
                        dicionarioAgrupado[nome] = qtd;
                }
            }

            // Segurança contra strings vazias
            if (!dicionarioAgrupado.Any() && p.CestaComprada != null)
            {
                dicionarioAgrupado[p.CestaComprada.Nome ?? "Cesta"] = 1;
            }

            // Converte para o formato compactado desejado: "14x Pão,5x Arroz Agulhinha"
            var itensFormatados = dicionarioAgrupado.Select(kvp => $"{kvp.Value}x {kvp.Key}");
            return string.Join(",", itensFormatados);
        }

        private string MontarLinhaTexto(Pedido p, string stringDosItens, int numeroLinha)
        {
            string totalFormatado = p.Total.ToString("F2", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
            return $"{numeroLinha}- Data:{p.DataDoPedido} |NomePedido:{p.NomePedido} |Recebedor:{p.Recebedor} |Endereco:{p.Endereco} |Pagamento:{p.FormaPagamento} |Status:{p.Status} |Total:{totalFormatado} |Obs:{p.Observacoes} |Itens:{stringDosItens}";
        }

        public void AdicionarNovoPedidoNoTxt(Pedido p)
        {
            try
            {
                string caminho = ObterCaminhoArquivo();
                Directory.CreateDirectory(Path.GetDirectoryName(caminho));

                int proximoId = MemoriaPedidos.Lista.Count + 1;

                string stringDosItens = GerarStringDosItens(p);
                string linha = MontarLinhaTexto(p, stringDosItens, proximoId);

                File.AppendAllText(caminho, linha + Environment.NewLine);
                MemoriaPedidos.Lista.Add(p);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao adicionar novo pedido: " + ex.Message);
            }
        }

        public void AtualizarArquivoTxt()
        {
            try
            {
                string caminho = ObterCaminhoArquivo();
                Directory.CreateDirectory(Path.GetDirectoryName(caminho));

                List<string> linesParaSalvar = new List<string>();
                int contadorId = 1;

                foreach (var p in MemoriaPedidos.Lista)
                {
                    string stringDosItens = GerarStringDosItens(p);
                    string linha = MontarLinhaTexto(p, stringDosItens, contadorId);
                    linesParaSalvar.Add(linha);
                    contadorId++;
                }

                File.WriteAllLines(caminho, linesParaSalvar);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao salvar pedidos: " + ex.Message);
            }
        }

        public void CarregarDoArquivo()
        {
            try
            {
                MemoriaPedidos.Lista.Clear();
                string caminho = ObterCaminhoArquivo();

                if (!File.Exists(caminho)) return;

                var linhas = File.ReadAllLines(caminho);

                foreach (var linhaBruta in linhas)
                {
                    if (string.IsNullOrWhiteSpace(linhaBruta)) continue;

                    string linhaProcessada = linhaBruta;

                    int indexTraco = linhaProcessada.IndexOf('-');
                    int indexPipe = linhaProcessada.IndexOf('|');

                    if (indexTraco > 0 && (indexPipe == -1 || indexTraco < indexPipe))
                    {
                        linhaProcessada = linhaProcessada.Substring(indexTraco + 1).Trim();
                    }

                    var partes = linhaProcessada.Split('|');
                    if (partes.Length < 9) continue;

                    string dataPedido = partes[0].Replace("Data:", "").Trim();
                    string nomePedido = partes[1].Replace("NomePedido:", "").Trim();
                    string recebedor = partes[2].Replace("Recebedor:", "").Trim();
                    string endereco = partes[3].Replace("Endereco:", "").Trim();
                    string pagamento = partes[4].Replace("Pagamento:", "").Trim();
                    string status = partes[5].Replace("Status:", "").Trim();
                    string totalStr = partes[6].Replace("Total:", "").Trim();
                    string obs = partes[7].Replace("Obs:", "").Trim();
                    string itensStr = partes[8].Replace("Itens:", "").Trim();

                    totalStr = totalStr.Replace(",", ".");
                    decimal.TryParse(totalStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal totalConvertido);

                    Pedido p = new Pedido
                    {
                        DataDoPedido = dataPedido,
                        NomePedido = nomePedido,
                        Recebedor = recebedor,
                        Endereco = endereco,
                        FormaPagamento = pagamento,
                        Status = status,
                        Total = totalConvertido,
                        Observacoes = obs,
                        Itens = new List<ItemPedido>()
                    };

                    if (!string.IsNullOrEmpty(itensStr))
                    {
                        var itensSeparados = itensStr.Split(',');
                        foreach (var itemRaw in itensSeparados)
                        {
                            var indexX = itemRaw.IndexOf('x');
                            if (indexX > 0)
                            {
                                string qtdStr = itemRaw.Substring(0, indexX).Trim();
                                string nomeItem = itemRaw.Substring(indexX + 1).Trim();

                                int.TryParse(qtdStr, out int qtdItem);

                                p.Itens.Add(new ItemPedido
                                {
                                    Nome = nomeItem,
                                    Quantidade = qtdItem > 0 ? qtdItem : 1
                                });
                            }
                            else if (itemRaw.Contains('='))
                            {
                                var dadosItem = itemRaw.Split('=');
                                if (dadosItem.Length == 2)
                                {

                                    int.TryParse(dadosItem[1].Trim(), out int qtdItem);
                                    p.Itens.Add(new ItemPedido { Nome = dadosItem[0].Trim(), Quantidade = qtdItem });
                                }
                            }
                        }
                    }

                    MemoriaPedidos.Lista.Add(p);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao carregar histórico de pedidos: " + ex.Message);
            }
        }
    }
}