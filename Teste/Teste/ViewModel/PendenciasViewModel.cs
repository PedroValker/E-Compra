using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Teste.Model;
using Teste.Repository;

namespace Teste.ViewModel
{


    public class PendenciasViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Pedido> PedidosPendentes { get; set; }

        public ICommand ConfirmarPagamentoCommand { get; }

        public int TotalPendencias => PedidosPendentes.Count;

        public decimal ValorTotalPendente =>
            PedidosPendentes.Sum(p => p.Total);

        public PendenciasViewModel()
        {
            PedidosPendentes = new ObservableCollection<Pedido>();

            CarregarPendencias();

            ConfirmarPagamentoCommand =
                new RelayCommand(obj =>
                {
                    System.Diagnostics.Debug.WriteLine("CLICOU");

                    if (obj is Pedido pedido)
                    {
                        ConfirmarPagamento(pedido);
                    }
                });
        }
        private void CarregarPendencias()
        {
            PedidosPendentes.Clear();

            var pendentes = MemoriaPedidos.Lista
                .Where(p => !p.Pago)
                .ToList();

            foreach (var pedido in pendentes)
            {
                PedidosPendentes.Add(pedido);
            }

            OnPropertyChanged(nameof(TotalPendencias));
            OnPropertyChanged(nameof(ValorTotalPendente));
        }

        private void ConfirmarPagamento(Pedido pedido)
        {
            pedido.Pago = true;

            var repository = new PedidoRepository();
            repository.AtualizarArquivoTxt();

            PedidosPendentes.Remove(pedido);

            OnPropertyChanged(nameof(TotalPendencias));
            OnPropertyChanged(nameof(ValorTotalPendente));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(
            [CallerMemberName] string? nome = null)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(nome));
        }
    }
}