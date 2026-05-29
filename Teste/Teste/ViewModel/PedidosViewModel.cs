using System;
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
        private int _idUsuarioLogado;

        public ObservableCollection<Pedido> Pedidos { get; set; }
        public ObservableCollection<Pedido> ListaPedidosEntregues { get; set; } = new ObservableCollection<Pedido>();
        public ObservableCollection<Pedido> ListaPedidosPendentes { get; set; } = new ObservableCollection<Pedido>();

        // 🚀 NOVO: Lista para o cliente acompanhar o que já está na rua
        public ObservableCollection<Pedido> ListaPedidosACaminho { get; set; } = new ObservableCollection<Pedido>();

        // 🛠️ CORREÇÃO: Declarado como anulável (?) para evitar aviso no construtor
        private Pedido? _pedidoSelecionado;
        public Pedido? PedidoSelecionado
        {
            get => _pedidoSelecionado;
            set
            {
                _pedidoSelecionado = value;
                OnPropertyChanged();
            }
        }

        public ICommand VerMaisCommand { get; }

        public PedidosViewModel(int idUsuario)
        {
            _idUsuarioLogado = idUsuario;
            _repository = new PedidoRepository();
            Pedidos = new ObservableCollection<Pedido>();

            // 🛠️ CORREÇÃO: Cast preventivo para evitar quebras se o comando receber algo inesperado
            VerMaisCommand = new RelayCommand<Pedido>(pedido => { if (pedido != null) PedidoSelecionado = pedido; });

            CarregarPedidosDoCliente();
        }

        private void CarregarPedidosDoCliente()
        {
            // APAGADO: _repository.CarregarDoArquivo(); (Isso agora é feito uma vez no App.xaml.cs)

            // Puxa direto da memória atualizada em tempo de execução
            var pedidosFiltrados = MemoriaPedidos.Lista
                .Where(p => p.IdUsuario == _idUsuarioLogado)
                .ToList();

            Pedidos.Clear();
            ListaPedidosEntregues.Clear();
            ListaPedidosPendentes.Clear();
            ListaPedidosACaminho.Clear();

            foreach (var pedido in pedidosFiltrados)
            {
                Pedidos.Add(pedido);

                if (pedido.Status != null && pedido.Status.Trim().Equals("Entregue", StringComparison.OrdinalIgnoreCase))
                {
                    ListaPedidosEntregues.Add(pedido);
                }
                else if (pedido.Status != null && pedido.Status.Trim().Equals("A Caminho", StringComparison.OrdinalIgnoreCase))
                {
                    ListaPedidosACaminho.Add(pedido);
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

        // 🛠️ CORREÇÃO: Evento alterado para aceitar nulo (bater com a assinatura do .NET Core)
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // 🛠️ CORREÇÃO: Implementação limpa do RelayCommand sem warnings de Nullable
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        public RelayCommand(Action<T> execute) => _execute = execute;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (parameter is T valorValido)
            {
                _execute(valorValido);
            }
        }

        public event EventHandler? CanExecuteChanged { add { } remove { } }
    }
}