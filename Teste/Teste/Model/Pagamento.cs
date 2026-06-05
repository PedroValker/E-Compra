using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Pagamento
{
    public int IdPagamento { get; set; }

    public int IdPedido { get; set; }

    public DateTime DataPagamento { get; set; }

    public bool Pago { get; set; }

    public Pagamento()
    {
        DataPagamento = DateTime.Now;
        Pago = false;
    }
}

public static class MemoriaPagamentos
{
    public static List<Pagamento> Lista { get; set; }
        = new List<Pagamento>();
}
