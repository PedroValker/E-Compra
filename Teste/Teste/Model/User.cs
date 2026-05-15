using System;

namespace Teste.Model
{
    public class User
    {
        public int Id { get; set; }

        public string Nome { get; set; } = "";
        public string Email { get; set; } = "";
        public string Telefone { get; set; } = "";
        public string Senha { get; set; } = "";
        public string FotoPerfil { get; set; } = "";
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public bool IsAdmin => Id == 1;

        // ✔ novo usuário
        public User() { }

        // ✔ usuário vindo do arquivo
        public User(int id)
        {
            Id = id;
        }
    }
}