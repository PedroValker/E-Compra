namespace Teste.Model
{
    public class Produto
    {
        public string Nome { get; set; } = "";
        public string Marca { get; set; } = "";
        public string Categoria { get; set; } = "";
        public decimal Preco { get; set; }

        public int QuantidadeFixa { get; set; }
        public int QuantidadeSelecionada { get; set; } = 1;

        public string Peso { get; set; } = "";
    }
}