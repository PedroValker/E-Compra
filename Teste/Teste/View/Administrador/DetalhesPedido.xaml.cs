using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Teste.Model;
using Teste.Repository;

namespace Teste.View
{
    public partial class DetalhesPedidoWindow : Window
    {
        private Pedido _pedidoAtual;

        public DetalhesPedidoWindow(Pedido pedido)
        {
            InitializeComponent();

            _pedidoAtual = pedido;

            // 1. Vincula o Pedido ao DataContext para alimentar todos os {Binding ...} do XAML automaticamente
            this.DataContext = pedido;

            // 2. Monta a lista da Composição Final da Cesta baseando-se nos itens do pedido
            var listaItensFinais = new List<object>();

            foreach (var itemPedido in pedido.Itens)
            {
                // Busca na memória a cesta correspondente para rastrear os produtos originais
                var cestaOriginal = MemoriaCestas.Lista.FirstOrDefault(c => c.Nome == itemPedido.Nome);

                // Se a cesta não for encontrada (excluída ou alterada), exibe uma mensagem de aviso
                string nomeProduto = cestaOriginal != null ? cestaOriginal.ResumoItens : "Produtos indisponíveis (Cesta excluída/alterada).";

                listaItensFinais.Add(new
                {
                    Quantidade = itemPedido.Quantidade,
                    Nome = itemPedido.Nome
                });
            }

            // 3. Alimenta o novo DataGrid principal da sua interface
            GridItensFinais.ItemsSource = listaItensFinais;

            // 4. Configuração dos Badges Visuais com base no tipo do pedido
            ConfigurarBadges(pedido);

            // 🔥 5. CORREÇÃO CRÍTICA: Agora verifica o status OU se a string da composição é "Modificada"
            if ((pedido.Status != null && pedido.Status.Equals("Modificado", StringComparison.OrdinalIgnoreCase)) ||
                (pedido.TipoComposicao != null && pedido.TipoComposicao.ToLower().Contains("modificad")))
            {
                BtnPreparada.Visibility = Visibility.Visible;
            }
        }

        private void ConfigurarBadges(Pedido pedido)
        {
            if (string.IsNullOrEmpty(pedido.TipoComposicao))
            {
                BadgeComposicao.Visibility = Visibility.Collapsed;
            }
            else
            {
                BadgeComposicao.Visibility = Visibility.Visible;
                TxtBadge.Text = pedido.TipoComposicao;

                string composicaoLower = pedido.TipoComposicao.ToLower();

                // 🔥 NOVA VALIDAÇÃO: Se virar "Preparada", aplica o azul de sucesso/completa
                if (composicaoLower.Contains("preparada"))
                {
                    BadgeComposicao.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E0F2FE")); // Azul suave (Sky-100)
                    TxtBadge.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#0369A1")); // Azul escuro executivo (Sky-700)
                }
                else if (composicaoLower.Contains("personalizad") || composicaoLower.Contains("completa"))
                {
                    // Mantém ou adapta o tom azul clássico do sistema
                    BadgeComposicao.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#DBEAFE")); // Azul claro
                    TxtBadge.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1E40AF")); // Azul escuro
                }
                else
                {
                    // Amarelo para quando ainda estiver como "Modificada"
                    BadgeComposicao.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FEF3C7")); // Amarelo claro
                    TxtBadge.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#92400E")); // Amarelo escuro
                }
            }
        }

        private void MarcarPreparada_Click(object sender, RoutedEventArgs e)
        {
            if (_pedidoAtual != null)
            {
                var resultado = MessageBox.Show("Deseja confirmar que esta cesta modificada já foi montada e está PRONTAMENTE PREPARADA?",
                                                "Confirmar Preparo",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    // 🛠️ CORREÇÃO LOGÍSTICA: O Status continua "Pendente", mas registramos que a montagem foi feita!
                    _pedidoAtual.StatusMontagem = "Pronta";

                    MessageBox.Show("Cesta marcada como Montada/Pronta com sucesso!",
                                    "Sucesso",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Information);

                    BtnPreparada.Visibility = Visibility.Collapsed;
                    this.Close();
                }
            }
        }

        private void GerarPdf_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Gerando relatório PDF do resumo logístico...", "PDF", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Fechar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}