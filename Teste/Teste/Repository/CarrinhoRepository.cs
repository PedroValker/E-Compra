using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Teste.Model;

namespace Teste.Repository
{
    public class CarrinhoRepository
    {
        private string ObterCaminhoArquivo()
        {
            // Busca a pasta "Dados" na raiz do projeto
            string pastaProjeto = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));

            // 🔥 CORREÇÃO E MELHORIA: Proteção contra nulo e uso do ID como chave única
            string sufixoArquivo = "Visitante";

            if (Sessao.UsuarioLogado != null)
            {
                // Usar o ID garante que, mesmo se existirem dois usuários com o mesmo nome, os carrinhos não se misturem
                sufixoArquivo = Sessao.UsuarioLogado.Id.ToString();
            }

            string nomeArquivo = $"carrinho_{sufixoArquivo}.txt";

            return Path.Combine(pastaProjeto, "Dados", nomeArquivo);
        }

        // 🔥 SALVA TUDO NO TXT DO USUÁRIO LOGADO
        public void AtualizarArquivoTxt()
        {
            try
            {
                string caminho = ObterCaminhoArquivo();
                Directory.CreateDirectory(Path.GetDirectoryName(caminho));

                List<string> linhasParaSalvar = new List<string>();

                foreach (var item in MemoriaCarrinho.Itens)
                {
                    // 🛠️ SEGURANÇA: Se a obs estiver vazia, salva como "NENHUMA" para não quebrar o Split('|')
                    string obsSalvar = string.IsNullOrWhiteSpace(item.Observacoes) ? "NENHUMA" : item.Observacoes.Trim();

                    string linha = $"CestaID:{item.CestaSelecionada.Id} |Qtd:{item.Quantidade} |Obs:{obsSalvar}";
                    linhasParaSalvar.Add(linha);
                }

                File.WriteAllLines(caminho, linhasParaSalvar);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao salvar o carrinho no TXT: " + ex.Message);
            }
        }

        // 🔥 CARREGA DO TXT DO USUÁRIO PARA A MEMÓRIA
        public void CarregarDoArquivo()
        {
            MemoriaCarrinho.Itens.Clear();
            string caminho = ObterCaminhoArquivo();

            if (!File.Exists(caminho)) return;

            var linhas = File.ReadAllLines(caminho);

            foreach (var linha in linhas)
            {
                if (string.IsNullOrWhiteSpace(linha)) continue;

                var partes = linha.Split('|');

                if (partes.Length < 3) continue;

                string idLimpo = partes[0].Replace("CestaID:", "").Trim();
                string qtdLimpa = partes[1].Replace("Qtd:", "").Trim();
                string obsLimpa = partes[2].Replace("Obs:", "").Trim();

                if (!int.TryParse(idLimpo, out int idCesta)) continue;
                if (!int.TryParse(qtdLimpa, out int quantidade)) continue;

                Cesta cestaEncontrada = MemoriaCestas.Lista.FirstOrDefault(c => c.Id == idCesta);

                if (cestaEncontrada != null)
                {
                    ItemCarrinho itemSalvo = new ItemCarrinho
                    {
                        CestaSelecionada = cestaEncontrada,
                        Quantidade = quantidade,
                        // 🛠️ RECUPERAÇÃO: Se for "NENHUMA", volta a ser string vazia na memória
                        Observacoes = obsLimpa == "NENHUMA" ? "" : obsLimpa
                    };

                    MemoriaCarrinho.Itens.Add(itemSalvo);
                }
            }
        }
    }
}