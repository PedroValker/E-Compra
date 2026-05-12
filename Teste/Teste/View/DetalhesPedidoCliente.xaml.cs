using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Teste.Model;
using Teste.Repository;

namespace Teste.View
{
    public partial class DetalhesPedidoCliente : Window
    {
        public DetalhesPedidoCliente(Pedido pedido)
        {
            InitializeComponent();

            // 🔥 MÁGICA: Define o pedido como fonte de dados para a janela inteira!
            // Isso permite usar {Binding Total}, {Binding Endereco}, etc, direto no XAML
            DataContext = pedido;

            var listaParaTela = new List<object>();

            foreach (var itemPedido in pedido.Itens)
            {
                var cestaOriginal = MemoriaCestas.Lista.FirstOrDefault(c => c.Nome == itemPedido.Nome);

                string produtos = cestaOriginal != null ? cestaOriginal.ResumoItens : "Produtos indisponíveis (Cesta excluída).";
                decimal precoUnitario = cestaOriginal != null ? cestaOriginal.Preco : 0m;

                listaParaTela.Add(new
                {
                    Quantidade = itemPedido.Quantidade,
                    Nome = itemPedido.Nome,
                    Produtos = produtos,
                  
                    Subtotal = precoUnitario * itemPedido.Quantidade
                });
            }

            GridItens.ItemsSource = listaParaTela;
        }

        private void Fechar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}