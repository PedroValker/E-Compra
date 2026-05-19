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

        // 🚀 ALTERAÇÃO: Mudou de string para int para armazenar o ID fixo do usuário
        private int _idUsuarioLogado;

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

        // 🚀 ALTERAÇÃO: O construtor agora aceita formalmente o 'int idUsuario' vindo da View
        public PedidosViewModel(int idUsuario)
        {
            _idUsuarioLogado = idUsuario;
            _repository = new PedidoRepository();
            Pedidos = new ObservableCollection<Pedido>();

            VerMaisCommand = new RelayCommand<Pedido>(pedido => PedidoSelecionado = pedido);

            CarregarPedidosDoCliente();
        }

        private void CarregarPedidosDoCliente()
        {
            // Força o repositório a ler as linhas atualizadas do arquivo pedidos.txt
            _repository.CarregarDoArquivo();

            // 🚀 ALTERAÇÃO CRÍTICA: Agora filtramos diretamente pelo ID numérico amarrado ao pedido
            var pedidosFiltrados = MemoriaPedidos.Lista
                .Where(p => p.IdUsuario == _idUsuarioLogado)
                .ToList();

            Pedidos.Clear();
            ListaPedidosEntregues.Clear();
            ListaPedidosPendentes.Clear();

            foreach (var pedido in pedidosFiltrados)
            {
                Pedidos.Add(pedido);

                // Separação por Status de forma segura
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