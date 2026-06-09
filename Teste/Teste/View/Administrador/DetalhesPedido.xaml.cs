using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Teste.Model;
using Teste.Repository;

namespace Teste.View
{
    public partial class DetalhesPedidoWindow : Window
    {
        public DetalhesPedidoWindow(Pedido pedido)
        {
            InitializeComponent();

            // Define o título com o código do pedido e o cliente
            TxtTitulo.Text = $"Itens do Pedido ({pedido.Recebedor})";

            // 🔥 Cria uma lista nova para a tela, combinando o Item do Pedido + Produtos da Cesta
            var listaParaTela = new List<object>();

            foreach (var itemPedido in pedido.Itens)
            {
                // Vai na memória e tenta achar a cesta com o mesmo nome para pegar os produtos
                var cestaOriginal = MemoriaCestas.Lista.FirstOrDefault(c => c.Nome == itemPedido.Nome);

                // Se achar a cesta, pega os produtos, se não achar, avisa.
                string produtos = cestaOriginal != null ? cestaOriginal.ResumoItens : "Produtos indisponíveis (Cesta excluída ou alterada).";

                // Adiciona na lista temporária para o DataGrid ler
                listaParaTela.Add(new
                {
                    Quantidade = itemPedido.Quantidade,
                    Nome = itemPedido.Nome,
                    Produtos = produtos
                });
            }

            // Joga essa nova lista combinada no DataGrid
            GridItens.ItemsSource = listaParaTela;
        }

        private void Fechar_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // Fecha o pop-up
        }


    }
}