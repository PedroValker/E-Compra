using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Teste.Model
{
    public class Cesta
    {
        private static int contador = 1;

        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public decimal Preco { get; set; }
        public string ImagemPath { get; set; }
        public List<Produto> Itens { get; set; } = new List<Produto>();

        // CONSTRUTOR PADRÃO
        public Cesta()
        {
            Id = contador++;
        }

        // CONSTRUTOR DE CARREGAMENTO
        public Cesta(int id)
        {
            Id = id;
            if (id >= contador)
            {
                contador = id + 1;
            }
        }

        /// <summary>
        /// Gera o resumo dos itens agrupados de forma profissional
        /// </summary>
        public string ResumoItens
        {
            get
            {
                if (Itens == null || !Itens.Any())
                    return "Cesta vazia";

                // Lógica unificada:
                // 1. Agrupamos por nome para evitar duplicatas na string
                // 2. Para a quantidade, somamos a 'QuantidadeSelecionada' (se houver) 
                //    ou contamos as ocorrências do objeto na lista.
                var agrupados = Itens.GroupBy(i => i.Nome)
                                     .Select(g =>
                                     {
                                         int qtd = g.Sum(p => p.QuantidadeSelecionada > 0 ? p.QuantidadeSelecionada : 1);
                                         return $"{qtd}x {g.Key}";
                                     });

                return string.Join(", ", agrupados);
            }
        }

        /// <summary>
        /// Carrega a imagem de forma assíncrona para não travar o arquivo no Windows
        /// </summary>
        public BitmapImage ImagemCompleta
        {
            get
            {
                try
                {
                    if (string.IsNullOrEmpty(ImagemPath))
                        return null;

                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    // Ajuste de caminho para ambiente de desenvolvimento (pula bin/debug)
                    string projeto = Path.GetFullPath(Path.Combine(baseDir, @"..\..\.."));
                    string caminhoCompleto = Path.Combine(projeto, ImagemPath);

                    // Se não achou no caminho de dev, tenta no caminho relativo direto (produção)
                    if (!File.Exists(caminhoCompleto))
                        caminhoCompleto = Path.Combine(baseDir, ImagemPath);

                    if (!File.Exists(caminhoCompleto))
                        return null;

                    BitmapImage imagem = new BitmapImage();
                    using (var stream = new FileStream(caminhoCompleto, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        imagem.BeginInit();
                        imagem.CacheOption = BitmapCacheOption.OnLoad;
                        imagem.StreamSource = stream;
                        imagem.EndInit();
                        imagem.Freeze(); // Garante performance e evita erro de thread
                    }
                    return imagem;
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}