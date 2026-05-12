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

        // Comando para o botão "Ver Mais"
        public ICommand VerMaisCommand { get; }

        public PedidosViewModel(string usuarioLogado)
        {
            _usuarioLogado = usuarioLogado;
            _repository = new PedidoRepository();
            Pedidos = new ObservableCollection<Pedido>();

            // Comando que define o pedido selecionado para exibi-lo na direita
            VerMaisCommand = new RelayCommand<Pedido>(pedido => PedidoSelecionado = pedido);

            CarregarPedidosDoCliente();
        }

        private void CarregarPedidosDoCliente()
        {
            // 1. Carrega todos os pedidos do txt para a MemoriaPedidos.Lista
            _repository.CarregarDoArquivo();

            // 2. Filtra de forma robusta (Ignora espaços nas pontas e ignora Maiúsculas/Minúsculas)
            var pedidosFiltrados = MemoriaPedidos.Lista
                .Where(p => !string.IsNullOrEmpty(p.Recebedor) &&
                            !string.IsNullOrEmpty(_usuarioLogado) &&
                            p.Recebedor.Trim().Equals(_usuarioLogado.Trim(), System.StringComparison.OrdinalIgnoreCase))
                .ToList();

            Pedidos.Clear();
            foreach (var pedido in pedidosFiltrados)
            {
                Pedidos.Add(pedido);
            }

            // Seleciona o primeiro pedido por padrão para a tela não ficar vazia na direita
            if (Pedidos.Any())
            {
                PedidoSelecionado = Pedidos.First();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // Classe auxiliar simples para o comando do botão "Ver Mais"
    public class RelayCommand<T> : ICommand
    {
        private readonly System.Action<T> _execute;
        public RelayCommand(System.Action<T> execute) => _execute = execute;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute((T)parameter);
        public event System.EventHandler CanExecuteChanged { add { } remove { } }
    }
}