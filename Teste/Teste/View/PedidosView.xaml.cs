using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Teste.Model;
using Teste.ViewModel;

namespace Teste.View
{
    public partial class PedidosView : UserControl
    {
        public PedidosView()
        {
            InitializeComponent();

            // 🚀 VÍNCULO POR ID: Envia o ID numérico único do usuário logado para a ViewModel
            if (Sessao.UsuarioLogado != null)
            {
                this.DataContext = new PedidosViewModel(Sessao.UsuarioLogado.Id);
            }
        }

        // 🟢 ABA 1: PEDIDOS PENDENTES
        private void AbaPendentes_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is PedidosViewModel vm)
            {
                TabelaDePedidos.ItemsSource = vm.ListaPedidosPendentes;
                vm.PedidoSelecionado = vm.ListaPedidosPendentes?.FirstOrDefault();
            }

            AlternarVisualAbas(AbaPendentesTexto, AbaCaminhoTexto, AbaHistoricoTexto);
        }

        // 🔵 ABA 2: PEDIDOS A CAMINHO (Nova Rota Logística)
        private void AbaCaminho_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is PedidosViewModel vm)
            {
                TabelaDePedidos.ItemsSource = vm.ListaPedidosACaminho;
                vm.PedidoSelecionado = vm.ListaPedidosACaminho?.FirstOrDefault();
            }

            AlternarVisualAbas(AbaCaminhoTexto, AbaPendentesTexto, AbaHistoricoTexto);
        }

        // 🔴 ABA 3: HISTÓRICO / ENTREGUES
        private void AbaHistorico_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is PedidosViewModel vm)
            {
                TabelaDePedidos.ItemsSource = vm.ListaPedidosEntregues;
                vm.PedidoSelecionado = vm.ListaPedidosEntregues?.FirstOrDefault();
            }

            AlternarVisualAbas(AbaHistoricoTexto, AbaPendentesTexto, AbaCaminhoTexto);
        }

        // 🎨 GERENCIADOR VISUAL DE ABAS (Padrão SaaS Moderno)
        private void AlternarVisualAbas(TextBlock ativa, TextBlock inativa1, TextBlock inativa2)
        {
            // Aba ativa ganha destaque em negrito e cor escura (#0F172A)
            ativa.FontWeight = FontWeights.Bold;
            ativa.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F172A"));

            // Abas inativas ficam com fonte normal e cor cinza suave (#94A3B8)
            inativa1.FontWeight = FontWeights.Normal;
            inativa1.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));

            inativa2.FontWeight = FontWeights.Normal;
            inativa2.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94A3B8"));
        }

        private void VerDetalhes_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button botao && botao.DataContext is Pedido pedidoClicado)
            {
                var janelaDetalhes = new DetalhesPedidoCliente(pedidoClicado);
                janelaDetalhes.ShowDialog();
            }
        }
    }
}