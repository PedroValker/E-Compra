using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Teste.Model
{
    public class Produto : INotifyPropertyChanged
    {
        public string Nome { get; set; } = "";
        public string Marca { get; set; } = "";
        public string Categoria { get; set; } = "";
        public decimal Preco { get; set; }

        public int QuantidadeFixa { get; set; }

        private int _quantidadeSelecionada = 1;
        public int QuantidadeSelecionada
        {
            get => _quantidadeSelecionada;
            set
            {
                if (_quantidadeSelecionada != value)
                {
                    _quantidadeSelecionada = value;
                    OnPropertyChanged();
                    // 🔥 Notifica a tela que o subtotal desse item mudou
                    OnPropertyChanged(nameof(SubtotalItem));
                }
            }
        }

        public string Peso { get; set; } = "";

        // 🔥 Propriedade calculada do item (Preço x Qtd Selecionada)
        public decimal SubtotalItem => Preco * QuantidadeSelecionada;

        // Implementação da atualização em tempo real para o WPF
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}