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
        private static int contador = 1;
        public int IdPedido { get; set; }

        public Pedido()
        {
            IdPedido = contador++;
        }

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

        public List<Produto> ProdutosOriginaisCesta
        {
            get
            {
                string? nomeCesta = CestaComprada?.Nome?.Trim().ToUpper();
                if (string.IsNullOrEmpty(nomeCesta)) return new List<Produto>();

                var original = MemoriaCestas.Lista.FirstOrDefault(c =>
                    c.Nome != null && c.Nome.Trim().ToUpper() == nomeCesta);

                return original?.Itens ?? new List<Produto>();
            }
        }

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

        // 🔥 BACKING FIELD: Variável interna para salvar o status de controle forçado pelo administrador
        private string? _tipoComposicaoForçado;

        // 🔥 PROPRIEDADE ATUALIZADA: Permite leitura computada E alteração direta via código
        public string TipoComposicao
        {
            get
            {
                // Se o administrador alterou o estado manualmente (ex: setou para "Preparada"), prioriza esse valor
                if (!string.IsNullOrEmpty(_tipoComposicaoForçado))
                    return _tipoComposicaoForçado;

                var originais = ProdutosOriginaisCesta;
                if (originais == null || !originais.Any())
                    return "Padrão";

                var mapaOriginal = originais.GroupBy(p => p.Nome?.Trim().ToUpper())
                                            .ToDictionary(g => g.Key ?? "", g => g.Count());

                var modificados = ProdutosModificadosCliente;
                var mapaCliente = modificados.GroupBy(i => i.Nome?.Trim().ToUpper())
                                             .ToDictionary(g => g.Key ?? "", g => g.Sum(i => i.Quantidade));

                if (mapaOriginal.Count != mapaCliente.Count)
                    return "Modificada";

                foreach (var par in mapaOriginal)
                {
                    if (!mapaCliente.TryGetValue(par.Key, out int qtdCliente) || par.Value != qtdCliente)
                    {
                        return "Modificada";
                    }
                }

                return "Completa";
            }
            set
            {
                _tipoComposicaoForçado = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsModificada)); // Atualiza também o bool auxiliar
            }
        }

        // 🔥 ATUALIZADO: Bate com o novo comportamento
        public bool IsModificada => TipoComposicao == "Modificada";

        private DateTime? _dataEntrega;
        public DateTime? DataEntrega
        {
            get => _dataEntrega;
            set { _dataEntrega = value; OnPropertyChanged(); }
        }
        private string _statusMontagem = "";
        public string StatusMontagem
        {
            get => _statusMontagem;
            set { _statusMontagem = value; OnPropertyChanged(); }
        }
        public int IdUsuario { get; set; }
        public string NomePedido { get; set; } = "";
        public string Recebedor { get; set; } = "";
        public string Endereco { get; set; } = "";
        public string FormaPagamento { get; set; } = "";

        private bool _pago;
        public bool Pago
        {
            get => _pago;
            set
            {
                _pago = value;
                OnPropertyChanged();
            }
        }
        public string Status { get; set; } = "Pendente";
        public decimal Total { get; set; }
        public DateTime Dia { get; set; } = DateTime.Now;
        public string DataDoPedido { get; set; } = DateTime.Now.ToString("dd/MM/yyyy");
        public string Produto { get; set; } = "";
        public int Quantidade { get; set; }
        public decimal Valor { get; set; }

        public List<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
        public string Observacoes { get; set; } = "";

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
        public string Observacoes { get; set; } = "";
        public string EnderecoEntrega { get; set; } = "";
    }

    public static class MemoriaPedidos
    {
        public static List<Pedido> Lista { get; set; } = new List<Pedido>();
    }
}