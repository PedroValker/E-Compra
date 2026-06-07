using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Teste.Model;
using Teste.Repository;

namespace Teste.ViewModel
{
    public class ClientesViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<User> Clientes { get; set; }

        public ObservableCollection<Pedido> PedidosCliente { get; set; }

        private User _clienteSelecionado;

        public User ClienteSelecionado
        {
            get => _clienteSelecionado;
            set
            {
                _clienteSelecionado = value;
                OnPropertyChanged();

                CarregarPedidosDoCliente();
            }
        }

        public ClientesViewModel()
        {
            Clientes = new ObservableCollection<User>(
                MemoriaUsuarios.Lista);

            PedidosCliente = new ObservableCollection<Pedido>();
        }

        private void CarregarPedidosDoCliente()
        {
            PedidosCliente.Clear();

            if (ClienteSelecionado == null)
                return;

            var pedidos = MemoriaPedidos.Lista
                .Where(p => p.IdUsuario == ClienteSelecionado.Id)
                .OrderByDescending(p => p.IdPedido);

            foreach (var pedido in pedidos)
            {
                PedidosCliente.Add(pedido);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(
            [CallerMemberName] string nome = "")
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(nome));
        }
    }
}