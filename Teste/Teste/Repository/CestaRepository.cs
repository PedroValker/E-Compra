using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                // Se a cesta já tiver uma imagem salva de forma relativa, preserva
                string imagemFinal = string.IsNullOrEmpty(cesta.ImagemPath) ? "null" : cesta.ImagemPath;

                // 🟢 Se for uma imagem vinda de fora da aplicação (caminho absoluto do computador)
                if (!string.IsNullOrEmpty(cesta.ImagemPath) && File.Exists(cesta.ImagemPath) && Path.IsPathRooted(cesta.ImagemPath))
                {
                    string pastaImagens = ObterPastaImagens();
                    Directory.CreateDirectory(pastaImagens);

                    string extensao = Path.GetExtension(cesta.ImagemPath);
                    string nomeArquivo = $"{Guid.NewGuid()}{extensao}";
                    string destinoCompletoFisico = Path.Combine(pastaImagens, nomeArquivo);

                    // Faz a cópia física segura para a pasta Dados/imagem do projeto
                    File.Copy(cesta.ImagemPath, destinoCompletoFisico, true);

                    // 🌟 FORÇANDO TEXTO PURO SEM SEPARADORES DE MÁQUINA LOCAL:
                    // Isso impede que o C# gere caminhos como "C:\Users\pedro" no arquivo de texto
                    imagemFinal = "Dados/imagem/" + nomeArquivo;
                }

                cesta.ImagemPath = imagemFinal;

                // Adiciona apenas se for uma nova ID na memória
                if (!MemoriaCestas.Lista.Any(c => c.Id == cesta.Id))
                {
                    MemoriaCestas.Lista.Add(cesta);
                }

                return true;
            }
            catch (Exception ex)
            {
                mensagemErro = "Erro ao processar imagem da cesta: " + ex.Message;
                return false;
            }
        }

        public void AtualizarArquivoTxt()
        {
            SalvarTudo();
        }

        // 💾 SALVAMENTO DEFINITIVO EM ARQUIVO TEXTO
        public void SalvarTudo()
        {
            try
            {
                string caminho = ObterCaminhoArquivo();
                string pasta = Path.GetDirectoryName(caminho);

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

                    // Garante que se o caminho estiver vazio ou nulo por falha de digitação vire string "null"
                    string imagem = string.IsNullOrWhiteSpace(cesta.ImagemPath) ? "null" : cesta.ImagemPath.Replace("\\", "/");

                    string linha = $"ID:{cesta.Id} |Nome:{cesta.Nome} |Preco:{cesta.Preco.ToString("F2")} |Imagem:{imagem} |Produtos:{nomesProdutos}";
                    linhasParaSalvar.Add(linha);
                }

                File.WriteAllLines(caminho, linhasParaSalvar, Encoding.UTF8);
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

            var linhas = File.ReadAllLines(caminho, Encoding.UTF8);

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

                // Normaliza o caminho do arquivo para o separador nativo do sistema operacional atual
                string caminhoNormalizado = imagemLimpa == "null" ? "" : imagemLimpa.Replace("/", Path.DirectorySeparatorChar.ToString());

                Cesta c = new Cesta(id)
                {
                    Nome = nomeLimpo,
                    Preco = precoConvertido,
                    ImagemPath = caminhoNormalizado
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
                                Marca = prodEncontrado.Marca,
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