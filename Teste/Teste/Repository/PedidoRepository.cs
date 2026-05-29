using System;
using System.IO;
using System.Collections.Generic;
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

        private string GerarStringDosItens(Pedido p)
        {
            var dicionarioAgrupado = new Dictionary<string, int>();

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
            else if (p.CestaComprada != null)
            {
                var itemNoCarrinho = MemoriaCarrinho.Itens.FirstOrDefault(c =>
                    c.CestaSelecionada != null &&
                    c.CestaSelecionada.Nome.Trim().ToUpper() == p.CestaComprada.Nome.Trim().ToUpper());

                if (itemNoCarrinho != null && itemNoCarrinho.CestaSelecionada.Itens != null && itemNoCarrinho.CestaSelecionada.Itens.Any())
                {
                    foreach (var prod in itemNoCarrinho.CestaSelecionada.Itens)
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

            if (!dicionarioAgrupado.Any() && p.CestaComprada != null)
            {
                dicionarioAgrupado[p.CestaComprada.Nome ?? "Cesta"] = 1;
            }

            var itensFormatados = dicionarioAgrupado.Select(kvp => $"{kvp.Value}x {kvp.Key}");
            return string.Join(",", itensFormatados);
        }

        private string MontarLinhaTexto(Pedido p, string stringDosItens, int numeroLinha)
        {
            string totalFormatado = p.Total.ToString("F2", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
            string dataEntregaStr = p.DataEntrega.HasValue ? p.DataEntrega.Value.ToString("yyyy-MM-dd") : "NULL";

            return $"{numeroLinha}- Data:{p.DataDoPedido} |IdUsuario:{p.IdUsuario} |NomePedido:{p.NomePedido} |Recebedor:{p.Recebedor} |Endereco:{p.Endereco} |Pagamento:{p.FormaPagamento} |Status:{p.Status} |Total:{totalFormatado} |Obs:{p.Observacoes} |Itens:{stringDosItens} |DataEntrega:{dataEntregaStr}";
        }

        // 🛠️ MODIFICADO: Agora este método manipula APENAS a lista em memória.
        // O arquivo físico não é tocado aqui.
        public void AdicionarNovoPedidoNoTxt(Pedido p)
        {
            try
            {
                if (Sessao.UsuarioLogado != null)
                {
                    p.IdUsuario = Sessao.UsuarioLogado.Id;
                }

                // Apenas insere na lista global de memória
                MemoriaPedidos.Lista.Add(p);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao adicionar novo pedido na memória: " + ex.Message);
            }
        }

        // 🌟 GARANTIDO: Este método reescreve o arquivo inteiro com tudo o que está na memória 
        // (registros que já vieram do arquivo + novos pedidos adicionados nesta execução).
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

                // File.WriteAllLines substitui o arquivo antigo completamente pela lista atualizada da memória
                File.WriteAllLines(caminho, linesParaSalvar);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao salvar arquivo de pedidos: " + ex.Message);
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

                    if (partes.Length < 10) continue;

                    string dataPedido = partes[0].Replace("Data:", "").Trim();
                    string idUsuarioStr = partes[1].Replace("IdUsuario:", "").Trim();
                    int.TryParse(idUsuarioStr, out int idUsuarioConvertido);

                    string nomePedido = partes[2].Replace("NomePedido:", "").Trim();
                    string recebedor = partes[3].Replace("Recebedor:", "").Trim();
                    string endereco = partes[4].Replace("Endereco:", "").Trim();
                    string pagamento = partes[5].Replace("Pagamento:", "").Trim();
                    string status = partes[6].Replace("Status:", "").Trim();
                    string totalStr = partes[7].Replace("Total:", "").Trim();
                    string obs = partes[8].Replace("Obs:", "").Trim();
                    string itensStr = partes[9].Replace("Itens:", "").Trim();

                    DateTime? dataEntregaConvertida = null;
                    if (partes.Length >= 11)
                    {
                        string dataEntregaStr = partes[10].Replace("DataEntrega:", "").Trim();
                        if (!string.IsNullOrEmpty(dataEntregaStr) && dataEntregaStr != "NULL" && DateTime.TryParse(dataEntregaStr, out DateTime dt))
                        {
                            dataEntregaConvertida = dt;
                        }
                    }

                    totalStr = totalStr.Replace(",", ".");
                    decimal.TryParse(totalStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal totalConvertido);

                    Pedido p = new Pedido
                    {
                        DataDoPedido = dataPedido,
                        IdUsuario = idUsuarioConvertido,
                        NomePedido = nomePedido,
                        Recebedor = recebedor,
                        Endereco = endereco,
                        FormaPagamento = pagamento,
                        Status = status,
                        Total = totalConvertido,
                        Observacoes = obs,
                        DataEntrega = dataEntregaConvertida,
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