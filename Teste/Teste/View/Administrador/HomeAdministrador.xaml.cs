using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Teste.Model;
using Teste.Repository;

namespace Teste.View
{
    public partial class HomeAdministrador : UserControl
    {
        public HomeAdministrador(string nomeAdmin)
        {
            InitializeComponent();

            BoasVindasTexto.Text =
                $"Bem-vindo de volta, {nomeAdmin}! Aqui está o resumo de hoje.";

            // CARREGA O TXT
            PedidoRepository repo = new PedidoRepository();
            repo.CarregarDoArquivo();

            // AGORA MONTA O DASHBOARD
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
              p.Status.Trim().ToLower() != "entregue")
              .ToString();

            // Pedidos entregues
            //TxtPedidosEntregues.Text = pedidos.Count(p =>
            //  p.Status != null &&
            //p.Status.ToLower() == "entregue")
            //.ToString();

            // Pagamentos pendentes
            TxtPagPendentes.Text = pedidos.Count(p =>
                p.FormaPagamento != null &&
                p.FormaPagamento.ToLower() == "pendente")
                .ToString();

            // Pedidos não pagos
            // Pagamentos pendentes
            TxtPagPendentes.Text = pedidos.Count(p => !p.Pago).ToString();

            // Faturamento total
            decimal total = pedidos
              .Where(p => p.Pago)
              .Sum(p => p.Total);

            TxtFaturamento.Text = $"R$ {total:N2}";

            // Quantidade de vendas por cesta


            var vendasPorItem = pedidos
     .Where(p => p.CestaComprada != null)
     .GroupBy(p => p.CestaComprada.Nome)
     .Select(g => new
     {
         Nome = g.Key,
         Quantidade = g.Count()
     })
     .OrderByDescending(x => x.Quantidade)
     .ToList();

            ListaVendas.ItemsSource = vendasPorItem;
        }
    }
}