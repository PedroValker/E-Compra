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

            // 🔥 ARQUIVO DINÂMICO: Cria um TXT separado para cada usuário
            // Se Sessao.UsuarioLogado estiver vazio, ele usa "Visitante" por segurança
            string nomeUsuario = string.IsNullOrWhiteSpace(Sessao.UsuarioLogado) ? "Visitante" : Sessao.UsuarioLogado;
            string nomeArquivo = $"carrinho_{nomeUsuario}.txt";

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
                    // Salvamos o ID da cesta, a quantidade e as observações
                    string linha = $"CestaID:{item.CestaSelecionada.Id} |Qtd:{item.Quantidade} |Obs:{item.Observacoes}";
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
            // Sempre limpa o carrinho atual antes de carregar o de outro usuário
            MemoriaCarrinho.Itens.Clear();
            string caminho = ObterCaminhoArquivo();

            if (!File.Exists(caminho)) return;

            var linhas = File.ReadAllLines(caminho);

            foreach (var linha in linhas)
            {
                var partes = linha.Split('|');

                // Prevenção de erro caso a linha esteja corrompida
                if (partes.Length < 3) continue;

                // Limpa as tags para pegar só os valores
                string idLimpo = partes[0].Replace("CestaID:", "").Trim();
                string qtdLimpa = partes[1].Replace("Qtd:", "").Trim();
                string obsLimpa = partes[2].Replace("Obs:", "").Trim();

                if (!int.TryParse(idLimpo, out int idCesta)) continue;

                // Vai na memória de cestas e acha a cesta original pelo ID
                Cesta cestaEncontrada = MemoriaCestas.Lista.FirstOrDefault(c => c.Id == idCesta);

                if (cestaEncontrada != null)
                {
                    ItemCarrinho itemSalvo = new ItemCarrinho
                    {
                        CestaSelecionada = cestaEncontrada,
                        Quantidade = int.Parse(qtdLimpa),
                        Observacoes = obsLimpa
                    };

                    MemoriaCarrinho.Itens.Add(itemSalvo);
                }
            }
        }
    }
}