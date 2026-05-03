using System.Collections.Generic;
using System.Linq;

namespace Teste.Model
{
    public class Cesta
    {
        // Variável estática que controla o próximo ID a ser gerado
        private static int contador = 1;

        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public decimal Preco { get; set; }

        public List<Produto> Itens { get; set; } = new List<Produto>();

        public string ResumoItens => string.Join(", ", Itens
      .Where(p => p.QuantidadeSelecionada > 0) // Pega só os itens que o cliente não zerou
      .Select(p => $"{p.QuantidadeSelecionada}x {p.Nome}")); // Junta a quantidade com o nome

        // 🔥 CONSTRUTOR PADRÃO (Para quando você clica em Salvar na tela)
        public Cesta()
        {
            Id = contador++;
        }

        // 🔥 CONSTRUTOR DE CARREGAMENTO (Para quando ler do TXT)
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