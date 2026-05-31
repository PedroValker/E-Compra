using System;
// Se a sua classe de endereço estiver em outra pasta (ex: Teste.Models), 
// adicione o using dela aqui em cima, ex: using Teste.SuaPasta;

namespace Teste.Model
{
    public class User
    {
        private static int contador = 1;

        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string Email { get; set; } = "";
        public string Telefone { get; set; } = "";
        public string Senha { get; set; } = "";
        public string FotoPerfil { get; set; } = "";
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        // 🚀 CORREÇÃO: Ajustado para "Endereco" (ou o nome exato que você deu para a classe)
        // O "?" permite que o campo comece nulo no cadastro.
        public Endereco? Endereco { get; set; }

        // Construtor padrão (novo usuário)
        public User()
        {
            Id = contador++;
        }

        // Construtor para carregar do arquivo
        public User(int id)
        {
            Id = id;

            if (id >= contador)
                contador = id + 1;
        }

        public bool IsAdmin => Id == 1;
    }
}