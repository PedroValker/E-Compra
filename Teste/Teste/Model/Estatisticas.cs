using System;
using System.Collections.Generic;
using System.Linq;

namespace Teste.Model
{
    public class ResumoPedidos
    {
     
        public decimal Faturamento { get; set; }
        public int TotalPedidos { get; set; }
        public int PedidosAEntregar { get; set; }
        public int PagPendentes { get; set; }
    }

    public static class EstatisticasPedidos
    {
        public static ResumoPedidos GerarResumo()
        {
            var pedidos = MemoriaPedidos.Lista;

            return new ResumoPedidos
            {
                Faturamento = pedidos.Sum(p => p.Total),

                TotalPedidos = pedidos.Count,

                PedidosAEntregar = pedidos.Count(p =>
                    p.Status != null &&
                    p.Status.ToLower() == "a entregar"),

                 PagPendentes = pedidos.Count(p =>
                    p.FormaPagamento != null &&
                    p.FormaPagamento.ToLower().Contains("pendente"))
            };
        }
    }
}