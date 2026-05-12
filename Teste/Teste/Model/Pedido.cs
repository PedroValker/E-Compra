using System.Collections.ObjectModel;

namespace Teste.Model
{
    public class ItemPedido
    {
        public string Nome { get; set; }
        public int Quantidade { get; set; }
    }

    public class Pedido
    {
        public string NomePedido { get; set; }
        public string Recebedor { get; set; }
        public string Endereco { get; set; }
        public string FormaPagamento { get; set; }
        public string Status { get; set; }
        public decimal Total { get; set; }
        public DateTime Dia { get; set; }
        public string DataDoPedido { get; set; } = DateTime.Now.ToString("dd/MM/yyyy");
        public string Produto { get; set; }
        public int Quantidade { get; set; }
        public decimal Valor { get; set; }

        public List<ItemPedido> Itens { get; set; }
        public string Observacoes { get; set; }
    }

    public class ItemResumo
    {
        public int Quantidade { get; set; }
        public string Nome { get; set; }
    }

    // 🔥 ADICIONE ISTO AQUI: Guarda os pedidos globais do sistema
    public static class MemoriaPedidos
    {
        public static List<Pedido> Lista { get; set; } = new List<Pedido>();
    
    }
}