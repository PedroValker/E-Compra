using System;
using System.IO;
using System.Text;
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
            var itensFormatados = new List<string>();

            if (p.CestaComprada != null && !string.IsNullOrEmpty(p.CestaComprada.Nome))
            {
                itensFormatados.Add($"CESTA={p.CestaComprada.Nome.Trim()}");
            }

            var dicionarioAgrupado = new Dictionary<string, int>();

            if (p.Itens != null && p.Itens.Any())
            {
                foreach (var item in p.Itens)
                {
                    if (string.IsNullOrEmpty(item.Nome)) continue;
                    if (p.CestaComprada != null && item.Nome.Trim().ToUpper() == p.CestaComprada.Nome.Trim().ToUpper()) continue;

                    string nome = item.Nome.Trim();
                    int qtd = item.Quantidade > 0 ? item.Quantidade : 1;

                    if (dicionarioAgrupado.ContainsKey(nome))
                        dicionarioAgrupado[nome] += qtd;
                    else
                        dicionarioAgrupado[nome] = qtd;
                }
            }

            foreach (var kvp in dicionarioAgrupado)
            {
                itensFormatados.Add($"{kvp.Value}x {kvp.Key}");
            }

            return string.Join(";", itensFormatados);
        }

        // 🚀 ATUALIZADO: Inclui a propriedade p.TipoComposicao no final da linha de texto
        private string MontarLinhaTexto(Pedido p, string stringDosItens, int numeroLinha)
        {
            string totalFormatado = p.Total.ToString("F2", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
            string dataEntregaStr = p.DataEntrega.HasValue ? p.DataEntrega.Value.ToString("yyyy-MM-dd") : "NULL";

            return $"{numeroLinha}- Data:{p.DataDoPedido} |IdUsuario:{p.IdUsuario} |NomePedido:{p.NomePedido} |Recebedor:{p.Recebedor} |Endereco:{p.Endereco} |Pagamento:{p.FormaPagamento} |Status:{p.Status} |Total:{totalFormatado} |Obs:{p.Observacoes} |Itens:{stringDosItens} |DataEntrega:{dataEntregaStr} |Composicao:{p.TipoComposicao}";
        }

        public void AdicionarNovoPedidoNoTxt(Pedido p)
        {
            try
            {
                if (Sessao.UsuarioLogado != null)
                {
                    p.IdUsuario = Sessao.UsuarioLogado.Id;
                }

                MemoriaPedidos.Lista.Add(p);
                AtualizarArquivoTxt();
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

                File.WriteAllLines(caminho, linesParaSalvar, Encoding.UTF8);
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

                var linhas = File.ReadAllLines(caminho, Encoding.UTF8);

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

                    // 🚀 ATUALIZADO: Recupera a string da composição salva se houver no arquivo (Retrocompatível)
                    string composicaoSalva = "Completa";
                    if (partes.Length >= 12)
                    {
                        composicaoSalva = partes[11].Replace("Composicao:", "").Trim();
                    }

                    totalStr = totalStr.Replace(",", ".");
                    decimal.TryParse(totalStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal totalConvertConvertido);

                    Pedido p = new Pedido
                    {
                        DataDoPedido = dataPedido,
                        IdUsuario = idUsuarioConvertido,
                        NomePedido = nomePedido,
                        Recebedor = recebedor,
                        Endereco = endereco,
                        FormaPagamento = pagamento,
                        Status = status,
                        Total = totalConvertConvertido,
                        Observacoes = obs,
                        DataEntrega = dataEntregaConvertida,
                        Itens = new List<ItemPedido>()
                    };

                    if (!string.IsNullOrEmpty(itensStr))
                    {
                        var itensSeparados = itensStr.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var itemRaw in itensSeparados)
                        {
                            if (itemRaw.StartsWith("CESTA="))
                            {
                                string nomeCestaExtraida = itemRaw.Replace("CESTA=", "").Trim();
                                p.CestaComprada = MemoriaCestas.Lista.FirstOrDefault(c =>
                                    c.Nome.Trim().ToUpper() == nomeCestaExtraida.ToUpper()) ?? new Cesta { Nome = nomeCestaExtraida };

                                p.Itens.Add(new ItemPedido { Nome = nomeCestaExtraida, Quantidade = 1 });
                                continue;
                            }

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
                        }
                    }

                    // Se a sua propriedade "TipoComposicao" for estritamente um get-only dinâmico, 
                    // o próprio motor do C# vai calculá-la em tempo de execução ao ler os 'Itens' processados acima. 
                    // Mas salvando-a na linha, garantimos a integridade do histórico estático no arquivo txt.

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