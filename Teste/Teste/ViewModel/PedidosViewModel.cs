using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Teste.Model;
using Teste.Repository;

namespace Teste.ViewModel
{
    public class PedidosViewModel : INotifyPropertyChanged
    {
        private PedidoRepository _repository;
        private string _usuarioLogado;
        public ObservableCollection<Pedido> Pedidos { get; set; }
        public ObservableCollection<Pedido> ListaPedidosEntregues { get; set; } = new ObservableCollection<Pedido>();
        public ObservableCollection<Pedido> ListaPedidosPendentes { get; set; } = new ObservableCollection<Pedido>();

        private Pedido _pedidoSelecionado;
        public Pedido PedidoSelecionado
        {
            get => _pedidoSelecionado;
            set
            {
                _pedidoSelecionado = value;
                OnPropertyChanged();
            }
        }

        public ICommand VerMaisCommand { get; }

        public PedidosViewModel(string usuarioLogado)
        {
            _usuarioLogado = usuarioLogado;
            _repository = new PedidoRepository();
            Pedidos = new ObservableCollection<Pedido>();

            VerMaisCommand = new RelayCommand<Pedido>(pedido => PedidoSelecionado = pedido);

            CarregarPedidosDoCliente();
        }

        private void CarregarPedidosDoCliente()
        {
            _repository.CarregarDoArquivo();

            var pedidosFiltrados = MemoriaPedidos.Lista
                .Where(p => !string.IsNullOrEmpty(p.Recebedor) &&
                            !string.IsNullOrEmpty(_usuarioLogado) &&
                            p.Recebedor.Trim().Equals(_usuarioLogado.Trim(), System.StringComparison.OrdinalIgnoreCase))
                .ToList();

            Pedidos.Clear();
            ListaPedidosEntregues.Clear();
            ListaPedidosPendentes.Clear();

            foreach (var pedido in pedidosFiltrados)
            {
                Pedidos.Add(pedido);

                // Separação por Status
                if (pedido.Status != null && pedido.Status.Trim().Equals("Entregue", System.StringComparison.OrdinalIgnoreCase))
                {
                    ListaPedidosEntregues.Add(pedido);
                }
                else
                {
                    ListaPedidosPendentes.Add(pedido);
                }
            }

            if (Pedidos.Any())
            {
                PedidoSelecionado = Pedidos.First();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly System.Action<T> _execute;
        public RelayCommand(System.Action<T> execute) => _execute = execute;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute((T)parameter);
        public event System.EventHandler CanExecuteChanged { add { } remove { } }
    }
}