using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Teste.Model;

namespace Teste.Repository
{
    public class CarrinhoRepository
    {
        private string ObterCaminhoArquivo()
        {
            // Busca a pasta "Dados" na raiz do projeto
            string pastaProjeto = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

            // Definição padrão caso não haja usuário logado
            string sufixoArquivo = "Visitante";

            if (Sessao.UsuarioLogado != null)
            {
                // Vincula o ID do usuário ao nome do arquivo do carrinho individual
                sufixoArquivo = Sessao.UsuarioLogado.Id.ToString();
            }

            string nomeArquivo = $"carrinho_{sufixoArquivo}.txt";
            return Path.Combine(pastaProjeto, "Dados", nomeArquivo);
        }

        // 🔥 SALVA O RASCUNHO DO CARRINHO NO TXT INDIVIDUAL DO USUÁRIO
        public void AtualizarArquivoTxt()
        {
            try
            {
                string caminho = ObterCaminhoArquivo();
                Directory.CreateDirectory(Path.GetDirectoryName(caminho));

                List<string> linhasParaSalvar = new List<string>();

                foreach (var item in MemoriaCarrinho.Itens)
                {
                    // Proteções sanitárias para evitar quebras no delimitador '|'
                    string obsSalvar = string.IsNullOrWhiteSpace(item.Observacoes) ? "NENHUMA" : item.Observacoes.Trim().Replace("|", "");
                    string endSalvar = string.IsNullOrWhiteSpace(item.EnderecoEntrega) ? "A combinar" : item.EnderecoEntrega.Trim().Replace("|", "");

                    // Salva os dados estruturados do carrinho temporário
                    string linha = $"CestaID:{item.CestaSelecionada.Id} |Qtd:{item.Quantidade} |Obs:{obsSalvar} |End:{endSalvar}";
                    linhasParaSalvar.Add(linha);
                }

                File.WriteAllLines(caminho, linhasParaSalvar, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao salvar o carrinho no TXT: " + ex.Message);
            }
        }

        // 🔥 CARREGA O RASCUNHO DO CARRINHO DO USUÁRIO PARA A MEMÓRIA
        public void CarregarDoArquivo()
        {
            try
            {
                MemoriaCarrinho.Itens.Clear();
                string caminho = ObterCaminhoArquivo();

                if (!File.Exists(caminho)) return;

                var linhas = File.ReadAllLines(caminho, Encoding.UTF8);

                foreach (var linha in linhas)
                {
                    if (string.IsNullOrWhiteSpace(linha)) continue;

                    var partes = linha.Split('|');
                    if (partes.Length < 3) continue;

                    string idLimpo = partes[0].Replace("CestaID:", "").Trim();
                    string qtdLimpa = partes[1].Replace("Qtd:", "").Trim();
                    string obsLimpa = partes[2].Replace("Obs:", "").Trim();

                    // Recupera o endereço se ele existir (compatibilidade com arquivos antigos)
                    string endLimpo = "A combinar";
                    if (partes.Length >= 4)
                    {
                        endLimpo = partes[3].Replace("End:", "").Trim();
                    }

                    if (!int.TryParse(idLimpo, out int idCesta)) continue;
                    if (!int.TryParse(qtdLimpa, out int quantidade)) continue;

                    Cesta cestaEncontrada = MemoriaCestas.Lista.FirstOrDefault(c => c.Id == idCesta);

                    if (cestaEncontrada != null)
                    {
                        ItemCarrinho itemSalvo = new ItemCarrinho
                        {
                            CestaSelecionada = cestaEncontrada,
                            Quantidade = quantidade,
                            Observacoes = obsLimpa == "NENHUMA" ? "" : obsLimpa,
                            EnderecoEntrega = endLimpo
                        };

                        MemoriaCarrinho.Itens.Add(itemSalvo);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao carregar o arquivo de carrinho: " + ex.Message);
            }
        }
    }
}