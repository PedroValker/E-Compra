using System;
using System.Collections.Generic;
using System.Linq;
using Teste.Repository;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Teste.Model
{
    public class ItemPedido
    {
        public string Nome { get; set; } = "";
        public int Quantidade { get; set; }
    }

    public class Pedido : INotifyPropertyChanged
    {
        private Cesta? _cestaComprada;

        public Cesta CestaComprada
        {
            get
            {
                if (_cestaComprada == null && Itens != null && Itens.Any())
                {
                    string? nomeDaCestaNoPedido = Itens.First().Nome;

                    if (!string.IsNullOrEmpty(nomeDaCestaNoPedido))
                    {
                        string nomeBusca = nomeDaCestaNoPedido.Trim().ToUpper();

                        _cestaComprada = MemoriaCestas.Lista.FirstOrDefault(c =>
                            c.Nome != null && c.Nome.Trim().ToUpper() == nomeBusca);
                    }
                }

                if (_cestaComprada == null)
                {
                    string nomeFallback = (Itens != null && Itens.Any()) ? Itens.First().Nome : "Cesta não identificada";
                    return new Cesta { Nome = nomeFallback };
                }

                return _cestaComprada;
            }
            set => _cestaComprada = value;
        }

        // 🔥 PROPRIEDADE COMPUTADA SEGURO: Retorna a receita original fixa da fábrica
        public List<Produto> ProdutosOriginaisCesta
        {
            get
            {
                // 🛠️ CORREÇÃO: Uso de ?.Trim() para evitar quebras caso o nome do fallback seja nulo
                string? nomeCesta = CestaComprada?.Nome?.Trim().ToUpper();
                if (string.IsNullOrEmpty(nomeCesta)) return new List<Produto>();

                var original = MemoriaCestas.Lista.FirstOrDefault(c =>
                    c.Nome != null && c.Nome.Trim().ToUpper() == nomeCesta);

                return original?.Itens ?? new List<Produto>();
            }
        }

        // 🔥 PROPRIEDADE COMPUTADA: Identifica de forma segura a composição final do carrinho do cliente
        public List<ItemPedido> ProdutosModificadosCliente
        {
            get
            {
                if (Itens == null || !Itens.Any())
                    return new List<ItemPedido>();

                if (Itens.Count > 1)
                {
                    return Itens.Skip(1).ToList();
                }

                if (CestaComprada != null && CestaComprada.Itens != null && CestaComprada.Itens.Any())
                {
                    return CestaComprada.Itens.Select(p => new ItemPedido
                    {
                        Nome = p.Nome ?? "Produto Sem Nome",
                        Quantidade = p.QuantidadeSelecionada > 0 ? p.QuantidadeSelecionada : 1
                    }).ToList();
                }

                return new List<ItemPedido>();
            }
        }

        private DateTime? _dataEntrega;
        public DateTime? DataEntrega
        {
            get => _dataEntrega;
            set { _dataEntrega = value; OnPropertyChanged(); }
        }

        // Propriedades nativas de Persistência e Controle Cadastral com strings inicializadas
        public int IdUsuario { get; set; }
        public string NomePedido { get; set; } = "";
        public string Recebedor { get; set; } = "";
        public string Endereco { get; set; } = "";
        public string FormaPagamento { get; set; } = "";
        public string Status { get; set; } = "Pendente"; // Valor padrão seguro
        public decimal Total { get; set; }
        public DateTime Dia { get; set; } = DateTime.Now;
        public string DataDoPedido { get; set; } = DateTime.Now.ToString("dd/MM/yyyy");
        public string Produto { get; set; } = "";
        public int Quantidade { get; set; }
        public decimal Valor { get; set; }

        // 🛠️ CORREÇÃO: Garantindo que a lista nasça instanciada para evitar NullReferenceException externos
        public List<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
        public string Observacoes { get; set; } = "";

        // 🛠️ CORREÇÃO: Assinatura do Evento PropertyChanged adequada ao .NET Core / Core 6+
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ItemResumo
    {
        public int Quantidade { get; set; }
        public string Nome { get; set; } = "";
    }

    public static class MemoriaPedidos
    {
        public static List<Pedido> Lista { get; set; } = new List<Pedido>();
    }
}