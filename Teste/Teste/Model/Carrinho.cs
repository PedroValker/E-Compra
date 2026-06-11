using System.Collections.Generic;
using System.Linq;

namespace Teste.Model
{
    public static class MemoriaCarrinho
    {
        public static List<ItemCarrinho> Itens { get; set; } = new List<ItemCarrinho>();
    }
    // Representa um item adicionado ao carrinho
    public class ItemCarrinho
    {
        public Cesta CestaSelecionada { get; set; }
        public int Quantidade { get; set; } = 1;
        private string _observacoes = "";
        public string EnderecoEntrega { get; set; } = "";
        public string Observacoes
        {
            get => _observacoes;
            set => _observacoes = value ?? "";
        }

        public decimal Subtotal => (CestaSelecionada?.Preco ?? 0) * Quantidade;

        public string ProdutosDetalhado
        {
            get
            {
                if (CestaSelecionada == null || CestaSelecionada.Itens == null || !CestaSelecionada.Itens.Any())
                    return "Nenhum produto incluso";
                var itensAgrupados = CestaSelecionada.Itens
                    .Where(p => p != null && !string.IsNullOrEmpty(p.Nome))
                    .GroupBy(p => p.Nome.Trim())
                    .Select(g =>
                    {
                        int qtdTotal = g.Sum(p => p.QuantidadeSelecionada > 0 ? p.QuantidadeSelecionada : 1);
                        return $"{qtdTotal}x {g.Key}";
                    });

                return string.Join(", ", itensAgrupados);
            }
        }
    }

}