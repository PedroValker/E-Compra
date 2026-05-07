using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using Teste.Model;


namespace Teste.ViewModel
{
    public class PedidosViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Pedido> Pedidos { get; set; }

        public ICollectionView PedidosView { get; set; }

        private string _statusSelecionado;
        public string StatusSelecionado
        {
            get => _statusSelecionado;
            set
            {
                _statusSelecionado = value;
                OnPropertyChanged();
                PedidosView.Refresh(); // 🔥 aplica filtro
            }
        }

        public List<string> StatusDisponiveis { get; set; }

        public PedidosViewModel()
        {
            Pedidos = new ObservableCollection<Pedido>
        {
            new Pedido { Produto = "Cesta Premium", Status = "Pendente", Valor = 574.50m },
            new Pedido { Produto = "Cesta Limpeza", Status = "Entregue", Valor = 256.89m }
        };

            StatusDisponiveis = new List<string>
        {
            "Todos",
            "Pendente",
            "Entregue"
        };

            StatusSelecionado = "Todos";

            PedidosView = CollectionViewSource.GetDefaultView(Pedidos);
            PedidosView.Filter = FiltrarPedidos;
        }

        private bool FiltrarPedidos(object obj)
        {
            if (obj is not Pedido pedido)
                return false;

            if (StatusSelecionado == "Todos")
                return true;

            return pedido.Status == StatusSelecionado;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}