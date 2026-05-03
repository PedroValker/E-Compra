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
            return Path.Combine(pastaProjeto, "Dados", "pedidos.txt"); // Arquivo global do Admin
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
                    // Transforma a lista de itens num texto simples: "Cesta Básica=1,Cesta Premium=2"
                    string stringDosItens = string.Join(",", p.Itens.Select(i => $"{i.Nome}={i.Quantidade}"));

                    // Monta a linha com o formato que você já usa
                    string linha = $"NomePedido:{p.NomePedido} |Recebedor:{p.Recebedor} |Endereco:{p.Endereco} |Pagamento:{p.FormaPagamento} |Status:{p.Status} |Total:{p.Total} |Obs:{p.Observacoes} |Itens:{stringDosItens}";
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
                if (partes.Length < 8) continue;

                string nomePedido = partes[0].Replace("NomePedido:", "").Trim();
                string recebedor = partes[1].Replace("Recebedor:", "").Trim();
                string endereco = partes[2].Replace("Endereco:", "").Trim();
                string pagamento = partes[3].Replace("Pagamento:", "").Trim();
                string status = partes[4].Replace("Status:", "").Trim();
                string totalStr = partes[5].Replace("Total:", "").Trim();
                string obs = partes[6].Replace("Obs:", "").Trim();
                string itensStr = partes[7].Replace("Itens:", "").Trim();

                Pedido p = new Pedido
                {
                    NomePedido = nomePedido,
                    Recebedor = recebedor,
                    Endereco = endereco,
                    FormaPagamento = pagamento,
                    Status = status,
                    Total = decimal.Parse(totalStr),
                    Observacoes = obs,
                    Itens = new List<ItemPedido>()
                };

                // Desmembra os itens ("Cesta Básica=1") de volta para a lista ItemPedido
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