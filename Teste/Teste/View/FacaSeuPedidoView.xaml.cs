using CestaApp.Views;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Teste.Model;
using Teste.Repository;

namespace Teste.View
{
    public partial class FacaSeuPedidoView : UserControl
    {
        // Retorna ao evento simples passando o objeto Cesta diretamente
        public event Action<Cesta> CestaSelecionada;
        public ObservableCollection<Cesta> ListaCestas { get; set; }

        public FacaSeuPedidoView()
        {
            InitializeComponent();

            ListaCestas = new ObservableCollection<Cesta>();
            this.DataContext = this;

            CarregarCestasDoBanco();
        }

        private void ComprarCesta_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button botao && botao.DataContext is Cesta cesta)
            {
                // Dispara o evento apenas enviando a cesta que foi clicada
                CestaSelecionada?.Invoke(cesta);
            }
        }

        private void CarregarCestasDoBanco()
        {
            CestaRepository repo = new CestaRepository();
            repo.CarregarDoArquivo();

            ListaCestas.Clear();

            foreach (Cesta cesta in MemoriaCestas.Lista)
            {
                ListaCestas.Add(cesta);
            }
        }
    }
}