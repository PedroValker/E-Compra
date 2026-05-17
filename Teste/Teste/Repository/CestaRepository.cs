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

                // TRATAMENTO DA IMAGEM
                if (!string.IsNullOrEmpty(cesta.ImagemPath) && File.Exists(cesta.ImagemPath))
                {
                    string pastaImagens = ObterPastaImagens();
                    Directory.CreateDirectory(pastaImagens);

                    string extensao = Path.GetExtension(cesta.ImagemPath);
                    string nomeArquivo = $"{Guid.NewGuid()}{extensao}";
                    string destino = Path.Combine(pastaImagens, nomeArquivo);

                    string origemCompleta = Path.GetFullPath(cesta.ImagemPath);
                    string destinoCompleto = Path.GetFullPath(destino);

                    if (!origemCompleta.Equals(destinoCompleto, StringComparison.OrdinalIgnoreCase))
                    {
                        using (var streamOrigem = new FileStream(origemCompleta, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (var streamDestino = new FileStream(destinoCompleto, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            streamOrigem.CopyTo(streamDestino);
                        }
                    }

                    imagemFinal = Path.Combine("Dados", "imagem", nomeArquivo);
                }

                // 🔥 NOVA LÓGICA DE ESCREVER (Agrupa e salva como 10x Pão)
                var stringsProdutos = cesta.Itens
                    .Where(p => p != null && !string.IsNullOrEmpty(p.Nome))
                    .GroupBy(p => p.Nome.Trim())
                    .Select(grupo => $"{grupo.Count()}x {grupo.Key}");

                string nomesProdutos = string.Join(",", stringsProdutos);

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
                    // 🔥 NOVA LÓGICA DE ATUALIZAR (Agrupa e salva como 10x Pão)
                    var stringsProdutos = cesta.Itens
                        .Where(p => p != null && !string.IsNullOrEmpty(p.Nome))
                        .GroupBy(p => p.Nome.Trim())
                        .Select(grupo => $"{grupo.Count()}x {grupo.Key}");

                    string nomesProdutos = string.Join(",", stringsProdutos);

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

                // Pega cada pedaço (ex: "10x Pão" ou "Pão" puro caso venha do modelo antigo)
                string[] itensComQuantidade = produtosLimpos.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var itemRaw in itensComQuantidade)
                {
                    string itemTratado = itemRaw.Trim();
                    int quantidade = 1;
                    string nomeProduto = itemTratado;

                    // 🔥 NOVA LÓGICA DE LEITURA: Verifica se o texto contém o multiplicador "x "
                    if (itemTratado.Contains("x "))
                    {
                        var partesQtd = itemTratado.Split(new[] { "x " }, StringSplitOptions.None);
                        if (partesQtd.Length == 2 && int.TryParse(partesQtd[0].Trim(), out int qtdInterpretada))
                        {
                            quantidade = qtdInterpretada;
                            nomeProduto = partesQtd[1].Trim();
                        }
                    }

                    // Busca o produto correspondente no cadastro geral de produtos
                    Produto prodEncontrado = MemoriaProdutos.Lista
         .FirstOrDefault(p => p.Nome != null && p.Nome.Trim().ToUpper() == nomeProduto.Trim().ToUpper());

                    if (prodEncontrado != null)
                    {
                        // Adiciona o produto na lista a quantidade de vezes que o arquivo mandou
                        for (int i = 0; i < quantidade; i++)
                        {
                            c.Itens.Add(new Produto
                            {
                                Nome = prodEncontrado.Nome,
                                Preco = prodEncontrado.Preco,
                                Peso = prodEncontrado.Peso,
                                QuantidadeSelecionada = 1
                            });
                        }
                    }
                }

                MemoriaCestas.Lista.Add(c);
            }
        }
    }
}