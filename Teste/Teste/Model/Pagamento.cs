using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Teste.Model
{
    class Pagamento
    {
        public int IdPagamento { get; set; }

        // Data do pagamento
        public DateTime DataPagamento { get; set; }

        // Status do pagamento
        // true = pago
        // false = pendente
        public bool Pago { get; set; }

        // Construtor opcional
        public Pagamento()
        {
            DataPagamento = DateTime.Now;
            Pago = false;
        }
    }
}
