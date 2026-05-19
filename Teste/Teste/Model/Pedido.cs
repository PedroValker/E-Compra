using System;
using System.Collections.Generic;
using System.Linq;
using Teste.Repository;

namespace Teste.Model
{
    public class ItemPedido
    {
        public string Nome { get; set; }
        public int Quantidade { get; set; }
    }

    public class Pedido
    {
        private Cesta _cestaComprada;

        public Cesta CestaComprada
        {
            get
            {
                if (_cestaComprada == null && Itens != null && Itens.Any())
                {
                    string nomeDaCestaNoPedido = Itens.First().Nome;

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
            set
            {
                _cestaComprada = value;
            }
        }

        // 🔥 PROPRIEDADE COMPUTADA: Retorna a receita original fixa da fábrica para esta cesta
        public List<Produto> ProdutosOriginaisCesta
        {
            get
            {
                // Busca a receita estruturada direto da memória global indexada
                var original = MemoriaCestas.Lista.FirstOrDefault(c =>
                    c.Nome != null && c.Nome.Trim().ToUpper() == CestaComprada.Nome.Trim().ToUpper());

                return original?.Itens ?? new List<Produto>();
            }
        }

        // 🔥 PROPRIEDADE COMPUTADA: Identifica de forma segura a composição final do carrinho do cliente
        public List<ItemPedido> ProdutosModificadosCliente
        {
            get
            {
                // Evita referências nulas caso o pedido não possua itens processados
                if (Itens == null || !Itens.Any())
                    return new List<ItemPedido>();

                // Se a primeira linha for a Cesta Master e o pedido contiver mais nós (os produtos modificados),
                // nós pulamos o primeiro registro (a cesta em si) e retornamos os itens reais da composição.
                if (Itens.Count > 1)
                {
                    return Itens.Skip(1).ToList();
                }

                // Caso o seu carrinho salve a Cesta modificada direto na propriedade CestaComprada,
                // adaptamos o mapeamento para ler os itens da instância local do pedido.
                if (CestaComprada != null && CestaComprada.Itens != null && CestaComprada.Itens.Any())
                {
                    return CestaComprada.Itens.Select(p => new ItemPedido
                    {
                        Nome = p.Nome,
                        Quantidade = p.QuantidadeSelecionada > 0 ? p.QuantidadeSelecionada : 1
                    }).ToList();
                }

                return new List<ItemPedido>();
            }
        }

        // Propriedades nativas de Persistência e Controle Cadastral
        // Adicione esta propriedade dentro da sua classe Pedido
        public int IdUsuario { get; set; }
        public string NomePedido { get; set; }

        public string Recebedor { get; set; }
        public string Endereco { get; set; }
        public string FormaPagamento { get; set; }
        public string Status { get; set; }
        public decimal Total { get; set; }
        public DateTime Dia { get; set; }
        public string DataDoPedido { get; set; } = DateTime.Now.ToString("dd/MM/yyyy");
        public string Produto { get; set; }
        public int Quantidade { get; set; }
        public decimal Valor { get; set; }

        public List<ItemPedido> Itens { get; set; }
        public string Observacoes { get; set; }
    }

    public class ItemResumo
    {
        public int Quantidade { get; set; }
        public string Nome { get; set; }
    }

    public static class MemoriaPedidos
    {
        public static List<Pedido> Lista { get; set; } = new List<Pedido>();
    }
}