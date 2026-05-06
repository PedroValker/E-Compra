using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Teste.Model;

namespace Teste.Repository
{
    public class CestaRepository
    {
        private string ObterPastaProjeto()
        {
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
        }

        private string ObterCaminhoArquivo()
        {
            return Path.Combine(ObterPastaProjeto(), "Dados", "cestas.txt");
        }

        private string ObterPastaImagens()
        {
            return Path.Combine(ObterPastaProjeto(), "Dados", "imagem");
        }

        public bool Salvar(Cesta cesta, out string mensagemErro)
        {
            mensagemErro = "";

            try
            {
                string caminho = ObterCaminhoArquivo();
                string? pasta = Path.GetDirectoryName(caminho);

                if (!string.IsNullOrEmpty(pasta))
                    Directory.CreateDirectory(pasta);

                string imagemFinal = "null";

                // 🔥 TRATAMENTO DA IMAGEM
                if (!string.IsNullOrEmpty(cesta.ImagemPath) && File.Exists(cesta.ImagemPath))
                {
                    string pastaImagens = ObterPastaImagens();
                    Directory.CreateDirectory(pastaImagens);

                    string extensao = Path.GetExtension(cesta.ImagemPath);

                    // 🔥 Nome único evita conflito e sobrescrita
                    string nomeArquivo = $"{Guid.NewGuid()}{extensao}";
                    string destino = Path.Combine(pastaImagens, nomeArquivo);

                    string origemCompleta = Path.GetFullPath(cesta.ImagemPath);
                    string destinoCompleto = Path.GetFullPath(destino);

                    // 🔥 Evita copiar o arquivo para ele mesmo
                    if (!origemCompleta.Equals(destinoCompleto, StringComparison.OrdinalIgnoreCase))
                    {
                        // 🔥 Copia de forma segura (evita arquivo travado)
                        using (var streamOrigem = new FileStream(origemCompleta, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (var streamDestino = new FileStream(destinoCompleto, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            streamOrigem.CopyTo(streamDestino);
                        }
                    }

                    imagemFinal = Path.Combine("Dados", "imagem", nomeArquivo);
                }

                string nomesProdutos = string.Join(",", cesta.Itens.Select(p => p.Nome));

                string linha = $"ID:{cesta.Id} |Nome:{cesta.Nome} |Preco:{cesta.Preco} |Imagem:{imagemFinal} |Produtos:{nomesProdutos}";

                File.AppendAllLines(caminho, new List<string> { linha });

                cesta.ImagemPath = imagemFinal;
                MemoriaCestas.Lista.Add(cesta);

                return true;
            }
            catch (Exception ex)
            {
                mensagemErro = "Erro ao salvar cesta: " + ex.Message;
                return false;
            }
        }

        public void AtualizarArquivoTxt()
        {
            try
            {
                string caminho = ObterCaminhoArquivo();
                string? pasta = Path.GetDirectoryName(caminho);

                if (!string.IsNullOrEmpty(pasta))
                    Directory.CreateDirectory(pasta);

                List<string> linhasParaSalvar = new List<string>();

                foreach (var cesta in MemoriaCestas.Lista)
                {
                    string nomesProdutos = string.Join(",", cesta.Itens.Select(p => p.Nome));

                    string imagem = string.IsNullOrEmpty(cesta.ImagemPath) ? "null" : cesta.ImagemPath;

                    string linha = $"ID:{cesta.Id} |Nome:{cesta.Nome} |Preco:{cesta.Preco} |Imagem:{imagem} |Produtos:{nomesProdutos}";
                    linhasParaSalvar.Add(linha);
                }

                File.WriteAllLines(caminho, linhasParaSalvar);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao atualizar cestas TXT: " + ex.Message);
            }
        }

        public void CarregarDoArquivo()
        {
            MemoriaCestas.Lista.Clear();
            string caminho = ObterCaminhoArquivo();

            if (!File.Exists(caminho)) return;

            var linhas = File.ReadAllLines(caminho);

            foreach (var linha in linhas)
            {
                var partes = linha.Split('|');

                if (partes.Length < 5) continue;

                string idLimpo = partes[0].Replace("ID:", "").Trim();
                string nomeLimpo = partes[1].Replace("Nome:", "").Trim();
                string precoLimpo = partes[2].Replace("Preco:", "").Trim();
                string imagemLimpa = partes[3].Replace("Imagem:", "").Trim();
                string produtosLimpos = partes[4].Replace("Produtos:", "").Trim();

                if (!int.TryParse(idLimpo, out int id))
                    continue;

                decimal.TryParse(precoLimpo, out decimal precoConvertido);

                Cesta c = new Cesta(id)
                {
                    Nome = nomeLimpo,
                    Preco = precoConvertido,
                    ImagemPath = imagemLimpa == "null" ? "" : imagemLimpa
                };

                string[] nomesProdutos = produtosLimpos.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var nome in nomesProdutos)
                {
                    Produto prodEncontrado = MemoriaProdutos.Lista
                        .FirstOrDefault(p => p.Nome == nome.Trim());

                    if (prodEncontrado != null)
                        c.Itens.Add(prodEncontrado);
                }

                MemoriaCestas.Lista.Add(c);
            }
        }
    }
}