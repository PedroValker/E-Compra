using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Teste.Model
{
    public class Cesta
    {
        // Variável estática que controla o próximo ID a ser gerado
        private static int contador = 1;
        public string ImagemPath { get; set; }
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public decimal Preco { get; set; }

        public List<Produto> Itens { get; set; } = new List<Produto>();

        public string ResumoItens => string.Join(", ", Itens
      .Where(p => p.QuantidadeSelecionada > 0) // Pega só os itens que o cliente não zerou
      .Select(p => $"{p.QuantidadeSelecionada}x {p.Nome}")); // Junta a quantidade com o nome

        // CONSTRUTOR PADRÃO (Para quando você clica em Salvar na tela)
        public Cesta()
        {
            Id = contador++;
        }
        public BitmapImage ImagemCompleta
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(ImagemPath))
                        return null;

                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string projeto = Path.GetFullPath(Path.Combine(baseDir, @"..\..\.."));
                    string caminhoCompleto = Path.Combine(projeto, ImagemPath);

                    if (!File.Exists(caminhoCompleto))
                        return null;

                    BitmapImage imagem = new BitmapImage();

                    using (var stream = new FileStream(caminhoCompleto, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        imagem.BeginInit();
                        imagem.CacheOption = BitmapCacheOption.OnLoad; // 🔥 evita travar o arquivo
                        imagem.StreamSource = stream;
                        imagem.EndInit();
                        imagem.Freeze(); // 🔥 melhora performance (thread-safe)
                    }

                    return imagem;
                }
                catch
                {
                    return null;
                }
            }
        }
        // CONSTRUTOR DE CARREGAMENTO (Para quando ler do TXT)
        public Cesta(int id)
        {
            Id = id;

            // Ajusta o contador para não repetir ID
            if (id >= contador)
            {
                contador = id + 1;
            }
        }
    }
}