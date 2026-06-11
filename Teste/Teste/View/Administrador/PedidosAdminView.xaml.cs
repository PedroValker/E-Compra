using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Teste.Model;
using Teste.Repository;

namespace Teste.View
{
    public partial class PedidosAdminView : UserControl
    {
        public ObservableCollection<Pedido> ListaPedidosPendentes { get; set; } = new ObservableCollection<Pedido>();
        public ObservableCollection<Pedido> ListaPedidosACaminho { get; set; } = new ObservableCollection<Pedido>();
        public ObservableCollection<Pedido> ListaPedidosEntregues { get; set; } = new ObservableCollection<Pedido>();

        public PedidosAdminView()
        {
            InitializeComponent();
            CarregarPedidos();
            this.DataContext = this;
        }

        private void CarregarPedidos()
        {
        

            ListaPedidosPendentes.Clear();
            ListaPedidosACaminho.Clear();
            ListaPedidosEntregues.Clear();

            foreach (var pedido in MemoriaPedidos.Lista)
            {
                if (string.IsNullOrEmpty(pedido.Status)) pedido.Status = "Pendente";

                if (pedido.Status.Equals("Entregue", StringComparison.OrdinalIgnoreCase))
                {
                    ListaPedidosEntregues.Add(pedido);
                }
                else if (pedido.Status.Equals("A Caminho", StringComparison.OrdinalIgnoreCase))
                {
                    ListaPedidosACaminho.Add(pedido);
                }
                else
                {
                    ListaPedidosPendentes.Add(pedido);
                }
            }

            if (GridPedidos != null)
            {
                GridPedidos.ItemsSource = ListaPedidosPendentes;
            }
        }

  
        private void AbaPendentes_Click(object sender, MouseButtonEventArgs e)
        {
            GridPedidos.ItemsSource = ListaPedidosPendentes;
            AlternarEstiloAbas(AbaPendentesTexto, AbaCaminhoTexto, AbaHistoricoTexto);
            PainelFiltroData.Visibility = Visibility.Collapsed;
        }

        private void AbaCaminho_Click(object sender, MouseButtonEventArgs e)
        {
            GridPedidos.ItemsSource = ListaPedidosACaminho;
            AlternarEstiloAbas(AbaCaminhoTexto, AbaPendentesTexto, AbaHistoricoTexto);
            PainelFiltroData.Visibility = Visibility.Visible;
            FiltroCalendario.SelectedDate = null;
        }

        private void AbaHistorico_Click(object sender, MouseButtonEventArgs e)
        {
            GridPedidos.ItemsSource = ListaPedidosEntregues;
            AlternarEstiloAbas(AbaHistoricoTexto, AbaPendentesTexto, AbaCaminhoTexto);
            PainelFiltroData.Visibility = Visibility.Collapsed;
        }

        private void AlternarEstiloAbas(TextBlock ativa, TextBlock inativa1, TextBlock inativa2)
        {
            ativa.FontWeight = FontWeights.Bold;
            ativa.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A"));

            inativa1.FontWeight = FontWeights.Normal;
            inativa1.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));

            inativa2.FontWeight = FontWeights.Normal;
            inativa2.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));
        }

        private void FiltroCalendario_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FiltroCalendario.SelectedDate.HasValue)
            {
                DateTime dataFiltrada = FiltroCalendario.SelectedDate.Value.Date;

                var pedidosFiltrados = ListaPedidosACaminho
                    .Where(p => p.DataEntrega.HasValue && p.DataEntrega.Value.Date == dataFiltrada)
                    .ToList();

                GridPedidos.ItemsSource = pedidosFiltrados;
            }
        }

        private void LimparFiltro_Click(object sender, RoutedEventArgs e)
        {
            FiltroCalendario.SelectedDate = null;
            GridPedidos.ItemsSource = ListaPedidosACaminho;
        }

 
        private void AgendarRota_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button botao && botao.DataContext is Pedido pedidoClicado)
            {
                Window janelaData = new Window
                {
                    Title = "Selecionar Data de Entrega",
                    Width = 320,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize
                };

                StackPanel painel = new StackPanel { Margin = new Thickness(20) };

                TextBlock label = new TextBlock
                {
                    Text = $"Escolha a data para entregar a {pedidoClicado.Recebedor}:",
                    Margin = new Thickness(0, 0, 0, 12),
                    TextWrapping = TextWrapping.Wrap,
                    FontWeight = FontWeights.Medium,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155"))
                };

                DatePicker seletorData = new DatePicker
                {
                    SelectedDate = DateTime.Today,
                    Margin = new Thickness(0, 0, 0, 20),
                    Height = 32
                };

                Button btnConfirmar = new Button
                {
                    Content = "Confirmar Agendamento",
                    Height = 36,
                    Style = (Style)FindResource("BtnAcaoStyle")
                };

                painel.Children.Add(label);
                painel.Children.Add(seletorData);
                painel.Children.Add(btnConfirmar);
                janelaData.Content = painel;

                btnConfirmar.Click += (s, args) =>
                {
                    if (seletorData.SelectedDate == null)
                    {
                        MessageBox.Show("Por favor, selecione uma data válida.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    DateTime dataEscolhida = seletorData.SelectedDate.Value;

                    // Validação baseada na lista unificada em memória
                    int totalNoDia = MemoriaPedidos.Lista.Count(p => p.DataEntrega.HasValue && p.DataEntrega.Value.Date == dataEscolhida.Date);

                    if (totalNoDia >= 8)
                    {
                        MessageBox.Show($"Limite Atingido! Já existem {totalNoDia} cestas agendadas para o dia {dataEscolhida:dd/MM/yyyy}.\nEscolha outro dia.",
                                        "Capacidade Máxima", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    pedidoClicado.DataEntrega = dataEscolhida;
                    pedidoClicado.Status = "A Caminho";

                    ListaPedidosPendentes.Remove(pedidoClicado);
                    ListaPedidosACaminho.Add(pedidoClicado);

                    janelaData.Close();
                    MessageBox.Show($"Pedido enviado para a rota do dia {dataEscolhida:dd/MM/yyyy}! Alteração salva em memória.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                };

                janelaData.ShowDialog();
            }
        }

      
        private void MarcarEntregue_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button botao && botao.DataContext is Pedido pedidoClicado)
            {
                var resp = MessageBox.Show($"Confirmar a entrega do pedido de {pedidoClicado.Recebedor}?",
                                           "Confirmação de Entrega", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (resp == MessageBoxResult.Yes)
                {
                 
                    pedidoClicado.Status = "Entregue";

                    ListaPedidosACaminho.Remove(pedidoClicado);
                    ListaPedidosEntregues.Add(pedidoClicado);

                    MessageBox.Show("Pedido concluído na memória! Ele será arquivado permanentemente ao fechar o sistema.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void VerItens_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button botao && botao.DataContext is Pedido pedidoClicado)
            {
                DetalhesPedidoWindow modal = new DetalhesPedidoWindow(pedidoClicado);
                modal.ShowDialog();
            }
        }
    }
}