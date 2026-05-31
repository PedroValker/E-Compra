using System.Collections.Generic;
using System.Linq;

namespace Teste.Model
{
    // Representa um item adicionado ao carrinho
    public class ItemCarrinho
    {
        public Cesta CestaSelecionada { get; set; }
        public int Quantidade { get; set; } = 1;

        private string _observacoes = "";
        public string Observacoes
        {
            get => _observacoes;
            set => _observacoes = value ?? "";
        }

        public decimal Subtotal => (CestaSelecionada?.Preco ?? 0) * Quantidade;

        // 🚀 NOVO: Retorna a lista de produtos da cesta formatada para exibir na interface
        public string ProdutosDetalhado
        {
            get
            {
                if (CestaSelecionada == null || CestaSelecionada.Itens == null || !CestaSelecionada.Itens.Any())
                    return "Nenhum produto nesta cesta.";

                // Junta os produtos em formato de texto: "2x Arroz, 1x Feijão"
                return string.Join(", ", CestaSelecionada.Itens.Select(p => $"{p.QuantidadeSelecionada}x {p.Nome}"));
            }
        }
    }

    // Guarda os itens enquanto o cliente navega no sistema
    public static class MemoriaCarrinho
    {
        public static List<ItemCarrinho> Itens { get; set; } = new List<ItemCarrinho>();
    }
}