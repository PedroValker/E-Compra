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

        public void AdicionarNovoPedidoNoTxt(Pedido p)
        {
            try
            {
                string caminho = ObterCaminhoArquivo();
                Directory.CreateDirectory(Path.GetDirectoryName(caminho));

                string stringDosItens = string.Join(",", p.Itens.Select(i => $"{i.Nome}={i.Quantidade}"));

                // 🔥 PADRÃO DEFINIDO: Data na posição 0
                string linha = $"Data:{p.DataDoPedido} |NomePedido:{p.NomePedido} |Recebedor:{p.Recebedor} |Endereco:{p.Endereco} |Pagamento:{p.FormaPagamento} |Status:{p.Status} |Total:{p.Total} |Obs:{p.Observacoes} |Itens:{stringDosItens}";

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

                List<string> linhasParaSalvar = new List<string>();

                foreach (var p in MemoriaPedidos.Lista)
                {
                    string stringDosItens = string.Join(",", p.Itens.Select(i => $"{i.Nome}={i.Quantidade}"));

                    // 🔥 CORRIGIDO: A data precisava estar aqui também, na mesma ordem!
                    string linha = $"Data:{p.DataDoPedido} |NomePedido:{p.NomePedido} |Recebedor:{p.Recebedor} |Endereco:{p.Endereco} |Pagamento:{p.FormaPagamento} |Status:{p.Status} |Total:{p.Total} |Obs:{p.Observacoes} |Itens:{stringDosItens}";
                    linhasParaSalvar.Add(linha);
                }

                File.WriteAllLines(caminho, linhasParaSalvar);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao salvar pedidos: " + ex.Message);
            }
        }

        public void CarregarDoArquivo()
        {
            MemoriaPedidos.Lista.Clear();
            string caminho = ObterCaminhoArquivo();

            if (!File.Exists(caminho)) return;

            var linhas = File.ReadAllLines(caminho);

            foreach (var linha in linhas)
            {
                var partes = linha.Split('|');

                // 🔥 Agora são 9 partes por causa da data!
                if (partes.Length < 9) continue;

                // 🔥 Ordem correta seguindo a linha que montamos em cima
                string dataPedido = partes[0].Replace("Data:", "").Trim();
                string nomePedido = partes[1].Replace("NomePedido:", "").Trim();
                string recebedor = partes[2].Replace("Recebedor:", "").Trim();
                string endereco = partes[3].Replace("Endereco:", "").Trim();
                string pagamento = partes[4].Replace("Pagamento:", "").Trim();
                string status = partes[5].Replace("Status:", "").Trim();
                string totalStr = partes[6].Replace("Total:", "").Trim();
                string obs = partes[7].Replace("Obs:", "").Trim();
                string itensStr = partes[8].Replace("Itens:", "").Trim();

                Pedido p = new Pedido
                {
                    DataDoPedido = dataPedido,
                    NomePedido = nomePedido,
                    Recebedor = recebedor,
                    Endereco = endereco,
                    FormaPagamento = pagamento,
                    Status = status,
                    Total = decimal.Parse(totalStr),
                    Observacoes = obs,
                    Itens = new List<ItemPedido>()
                };

                if (!string.IsNullOrEmpty(itensStr))
                {
                    var itensSeparados = itensStr.Split(',');
                    foreach (var itemRaw in itensSeparados)
                    {
                        var dadosItem = itemRaw.Split('=');
                        if (dadosItem.Length == 2)
                        {
                            p.Itens.Add(new ItemPedido
                            {
                                Nome = dadosItem[0].Trim(),
                                Quantidade = int.Parse(dadosItem[1].Trim())
                            });
                        }
                    }
                }

                MemoriaPedidos.Lista.Add(p);
            }
        }
    }
}