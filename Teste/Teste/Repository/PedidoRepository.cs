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

            return $"IdPedido:{p.IdPedido} |Data:{p.DataDoPedido} |IdUsuario:{p.IdUsuario} |NomePedido:{p.NomePedido} |Recebedor:{p.Recebedor} |Endereco:{p.Endereco} |Pagamento:{p.FormaPagamento} |Status:{p.Status} |Total:{totalFormatado} |Obs:{p.Observacoes} |Itens:{stringDosItens} |DataEntrega:{dataEntregaStr} |Composicao:{p.TipoComposicao} |Pago:{p.Pago}";
        }

        public void AdicionarNovoPedidoNoTxt(Pedido p)
        {
            try
            {
                if (Sessao.UsuarioLogado != null)
                {
                    p.IdUsuario = Sessao.UsuarioLogado.Id;
                }

                Console.WriteLine("=== NOVO PEDIDO ===");
                Console.WriteLine($"IdPedido: {p.IdPedido}");
                Console.WriteLine($"IdUsuario: {p.IdUsuario}");
                Console.WriteLine($"NomePedido: {p.NomePedido}");
                Console.WriteLine($"Recebedor: {p.Recebedor}");
                Console.WriteLine($"Endereco: {p.Endereco}");
                Console.WriteLine($"Pagamento: {p.FormaPagamento}");
                Console.WriteLine($"Status: {p.Status}");
                Console.WriteLine($"Total: {p.Total}");
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
                    var partes = linhaProcessada.Split('|');

                    if (partes.Length < 14)
                        continue;

                    string idPedidoStr = partes[0].Replace("IdPedido:", "").Trim();
                    int.TryParse(idPedidoStr, out int idPedido);

                    string dataPedido = partes[1].Replace("Data:", "").Trim();

                    string idUsuarioStr = partes[2].Replace("IdUsuario:", "").Trim();
                    int.TryParse(idUsuarioStr, out int idUsuarioConvertido);

                    string nomePedido = partes[3].Replace("NomePedido:", "").Trim();

                    string recebedor = partes[4].Replace("Recebedor:", "").Trim();

                    string endereco = partes[5].Replace("Endereco:", "").Trim();

                    string pagamento = partes[6].Replace("Pagamento:", "").Trim();

                    string status = partes[7].Replace("Status:", "").Trim();

                    string totalStr = partes[8].Replace("Total:", "").Trim();

                    string obs = partes[9].Replace("Obs:", "").Trim();

                    string itensStr = partes[10].Replace("Itens:", "").Trim();

                    string dataEntregaStr = partes[11]
                        .Replace("DataEntrega:", "")
                        .Trim();

                    string composicaoSalva = partes[12]
                        .Replace("Composicao:", "")
                        .Trim();

                    string pagoStr = partes[13]
                        .Replace("Pago:", "")
                        .Trim();

                    bool.TryParse(pagoStr, out bool pagoConvertido);

                    DateTime? dataEntregaConvertida = null;

                    if (!string.IsNullOrEmpty(dataEntregaStr)
                        && dataEntregaStr != "NULL"
                        && DateTime.TryParse(dataEntregaStr, out DateTime dt))
                    {
                        dataEntregaConvertida = dt;
                    }

                    totalStr = totalStr.Replace(",", ".");

                    decimal.TryParse(
                        totalStr,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out decimal totalConvertConvertido);

                    Pedido p = new Pedido
                    {
                        IdPedido = idPedido,
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
                        Pago = pagoConvertido,
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