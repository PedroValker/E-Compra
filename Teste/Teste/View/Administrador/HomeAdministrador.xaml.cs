using System.Linq;
using System.Windows.Controls;
using Teste.Model;

namespace Teste.View
{
    public partial class HomeAdministrador : UserControl
    {
        public HomeAdministrador(string nomeAdmin)
        {
            InitializeComponent();

            BoasVindasTexto.Text =
                $"Bem-vindo de volta, {nomeAdmin}! Aqui está o resumo de hoje.";

            CarregarDashboard();
        }

        private void CarregarDashboard()
        {
            var pedidos = MemoriaPedidos.Lista;

            // Total de pedidos
            TxtPedidos.Text = pedidos.Count.ToString();

            // Pedidos a entregar
            TxtPedidosAEntregar.Text = pedidos.Count(p =>
                p.Status != null &&
                p.Status.ToLower().Contains("entregar"))
                .ToString();

            // Pagamentos pendentes
            TxtPagPendentes.Text = pedidos.Count(p =>
                p.FormaPagamento != null &&
                p.FormaPagamento.ToLower().Contains("pendente"))
                .ToString();

            // Faturamento total
            decimal total = pedidos.Sum(p => p.Total);

            TxtFaturamento.Text = $"R$ {total:N2}";

            // Quantidade de vendas por cesta
            TxtVendas1.Text = pedidos.Count(p =>
                p.NomePedido == "Cesta Econômica")
                .ToString();

            TxtVendas2.Text = pedidos.Count(p =>
                p.NomePedido == "Cesta Família")
                .ToString();

            TxtVendas3.Text = pedidos.Count(p =>
                p.NomePedido == "Cesta Especial")
                .ToString();

            TxtVendas4.Text = pedidos.Count(p =>
                p.NomePedido == "Cesta Premium")
                .ToString();
        }
    }
}