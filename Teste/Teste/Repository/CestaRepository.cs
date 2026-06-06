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

        // 🔥 MODIFICADO: Salva apenas na memória RAM durante a execução
        public bool Salvar(Cesta cesta, out string mensagemErro)
        {
            mensagemErro = "";

            try
            {
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

                cesta.ImagemPath = imagemFinal;

                // Adiciona apenas na memória
                MemoriaCestas.Lista.Add(cesta);

                return true;
            }
            catch (Exception ex)
            {
                mensagemErro = "Erro ao salvar cesta na memória: " + ex.Message;
                return false;
            }
        }

        // 🔥 MODIFICADO: Atualiza apenas a instância em memória
        public void AtualizarArquivoTxt()
        {
            // Como a orientação agora é persistir no arquivo apenas ao fechar o app,
            // este método deixa de gravar no disco imediatamente para evitar gargalos.
            // A lista em memória já é atualizada por referência diretamente no seu View/Code-behind.
        }

        // 🔥 NOVO MÉTODO: Grava tudo no arquivo texto (Chamar no encerramento do programa)
        public void SalvarTudo()
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
                Console.WriteLine("Erro ao descarregar cestas no arquivo TXT: " + ex.Message);
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

                string[] itensComQuantidade = produtosLimpos.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var itemRaw in itensComQuantidade)
                {
                    string itemTratado = itemRaw.Trim();
                    int quantidade = 1;
                    string nomeProduto = itemTratado;

                    if (itemTratado.Contains("x "))
                    {
                        var partesQtd = itemTratado.Split(new[] { "x " }, StringSplitOptions.None);
                        if (partesQtd.Length == 2 && int.TryParse(partesQtd[0].Trim(), out int qtdInterpretada))
                        {
                            quantidade = qtdInterpretada;
                            nomeProduto = partesQtd[1].Trim();
                        }
                    }

                    Produto prodEncontrado = MemoriaProdutos.Lista
                        .FirstOrDefault(p => p.Nome != null && p.Nome.Trim().ToUpper() == nomeProduto.Trim().ToUpper());

                    if (prodEncontrado != null)
                    {
                        for (int i = 0; i < quantidade; i++)
                        {
                            c.Itens.Add(new Produto
                            {
                                Nome = prodEncontrado.Nome,
                                Marca = prodEncontrado.Marca, // 🔥 CORREÇÃO CRÍTICA: Resgatando a marca da memória de produtos
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