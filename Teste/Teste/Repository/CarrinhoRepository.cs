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
            string pastaProjeto = Path.GetFullPath(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\")
            );

            string nomeArquivo;

            if (Sessao.UsuarioLogado == null)
            {
                nomeArquivo = "carrinho_visitante.txt";
            }
            else
            {
                // 🔥 agora usa ID (correto e seguro)
                nomeArquivo = $"carrinho_{Sessao.UsuarioLogado.Id}.txt";
            }

            return Path.Combine(pastaProjeto, "Dados", nomeArquivo);
        }

        // 🔥 SALVA NO TXT DO USUÁRIO
        public void AtualizarArquivoTxt()
        {
            try
            {
                string caminho = ObterCaminhoArquivo();
                Directory.CreateDirectory(Path.GetDirectoryName(caminho));

                List<string> linhasParaSalvar = new List<string>();

                foreach (var item in MemoriaCarrinho.Itens)
                {
                    string linha =
                        $"CestaID:{item.CestaSelecionada.Id} |Qtd:{item.Quantidade} |Obs:{item.Observacoes}";

                    linhasParaSalvar.Add(linha);
                }

                File.WriteAllLines(caminho, linhasParaSalvar);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao salvar o carrinho: " + ex.Message);
            }
        }

        // 🔥 CARREGA DO TXT
        public void CarregarDoArquivo()
        {
            MemoriaCarrinho.Itens.Clear();

            string caminho = ObterCaminhoArquivo();

            if (!File.Exists(caminho))
                return;

            var linhas = File.ReadAllLines(caminho);

            foreach (var linha in linhas)
            {
                var partes = linha.Split('|');

                if (partes.Length < 3)
                    continue;

                string idLimpo = partes[0].Replace("CestaID:", "").Trim();
                string qtdLimpa = partes[1].Replace("Qtd:", "").Trim();
                string obsLimpa = partes[2].Replace("Obs:", "").Trim();

                if (!int.TryParse(idLimpo, out int idCesta))
                    continue;

                var cestaEncontrada =
                    MemoriaCestas.Lista.FirstOrDefault(c => c.Id == idCesta);

                if (cestaEncontrada != null)
                {
                    MemoriaCarrinho.Itens.Add(new ItemCarrinho
                    {
                        CestaSelecionada = cestaEncontrada,
                        Quantidade = int.Parse(qtdLimpa),
                        Observacoes = obsLimpa
                    });
                }
            }
        }
    }
}