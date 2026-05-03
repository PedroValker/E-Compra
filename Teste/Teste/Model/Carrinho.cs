using System.Collections.Generic;

namespace Teste.Model
{
    // Representa um item adicionado ao carrinho
    public class ItemCarrinho
    {
        public Cesta CestaSelecionada { get; set; }
        public int Quantidade { get; set; } = 1;
        public string Observacoes { get; set; }
        public decimal Subtotal => CestaSelecionada.Preco * Quantidade;
    }

    // Guarda os itens enquanto o cliente navega no sistema
    public static class MemoriaCarrinho
    {
        public static List<ItemCarrinho> Itens { get; set; } = new List<ItemCarrinho>();
    }
}