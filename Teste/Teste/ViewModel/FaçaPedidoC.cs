using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teste.Model;
using System.Windows.Input;

public class CestaViewModel : BaseViewModel
{
    public ObservableCollection<Produto> Produtos { get; set; }

    public string Observacoes { get; set; }

    public decimal Total => Produtos.Sum(p => p.Preco * p.QuantidadeSelecionada);

    public ICommand AumentarCommand { get; }
    public ICommand DiminuirCommand { get; }

    public CestaViewModel()
    {
        Produtos = new ObservableCollection<Produto>
        {
            new Produto { Nome = "Azeite de Oliva", Preco = 25, QuantidadeFixa = 2, QuantidadeSelecionada = 2 },
            new Produto { Nome = "Arroz Arbóreo", Preco = 15, QuantidadeFixa = 3, QuantidadeSelecionada = 3 },
            new Produto { Nome = "Café em Grãos", Preco = 30, QuantidadeFixa = 3, QuantidadeSelecionada = 3 },
            new Produto { Nome = "Vinho Tinto", Preco = 80, QuantidadeFixa = 1, QuantidadeSelecionada = 1 }
        };

        AumentarCommand = new RelayCommand(p =>
        {
            if (p is Produto prod)
            {
                prod.QuantidadeSelecionada++;
                OnPropertyChanged(nameof(Total));
            }
        });

        DiminuirCommand = new RelayCommand(p =>
        {
            if (p is Produto prod && prod.QuantidadeSelecionada > 0)
            {
                prod.QuantidadeSelecionada--;
                OnPropertyChanged(nameof(Total));
            }
        });
    }
}