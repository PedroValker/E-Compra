using System;
using System.Collections.Generic;
using System.Linq;

namespace Teste.Model
{
    public class ResumoPedidos
    {
        // Soma total dos pedidos
        public decimal Faturamento { get; set; }

        // Quantidade total de pedidos
        public int TotalPedidos { get; set; }

        // Pedidos que ainda serão entregues
        public int PedidosAEntregar { get; set; }

        // Pagamentos pendentes
        public int PagPendentes { get; set; }
    }

    public static class EstatisticasPedidos
    {
        public static ResumoPedidos GerarResumo()
        {
            var pedidos = MemoriaPedidos.Lista;

            return new ResumoPedidos
            {
                // Soma todos os valores
                Faturamento = pedidos.Sum(p => p.Total),

                // Conta total de pedidos
                TotalPedidos = pedidos.Count,

                // Conta pedidos com status "A Entregar"
                PedidosAEntregar = pedidos.Count(p =>
                    p.Status != null &&
                    p.Status.ToLower().Contains("entregar")),

                // Conta pagamentos pendentes
                PagPendentes = pedidos.Count(p =>
                    p.FormaPagamento != null &&
                    p.FormaPagamento.ToLower().Contains("pendente"))
            };
        }
    }
}