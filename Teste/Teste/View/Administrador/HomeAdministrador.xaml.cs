using System.Windows.Controls;

namespace Teste.View
{
    public partial class HomeAdministrador : UserControl
    {
        public class DashboardVM
        {
            public string Vendas1 { get; set; } = "120 vendas";
            public string Vendas2 { get; set; } = "250 vendas";
            public string Vendas3 { get; set; } = "320 vendas";
            public string Vendas4 { get; set; } = "450 vendas";

            public string Faturamento { get; set; } = "R$ 45.900,00";

            public string Crescimento { get; set; } = "+12% este mês";
        }

        public HomeAdministrador(string nomeAdmin)
        {
            InitializeComponent();

            BoasVindasTexto.Text =
                $"Bem-vindo de volta, {nomeAdmin}! Aqui está o resumo de hoje.";

            // IMPORTANTE
            this.DataContext = new DashboardVM();
        }
    }
}