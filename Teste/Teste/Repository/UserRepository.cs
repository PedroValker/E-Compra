using System;
using System.Linq;
using Teste.Model;
using System.IO;
using System.Collections.Generic;

namespace Teste.Repository
{
    class UserRepository
    {
        // 🔥 CARREGAR DO ARQUIVO
        public void CarregarDoArquivo()
        {
            MemoriaUsuarios.Lista.Clear();

            string pastaProjeto = Path.GetFullPath(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\")
            );

            string caminho = Path.Combine(pastaProjeto, "cadastroUsers", "cadastroUsers.txt");

            if (!File.Exists(caminho))
                return;

            var linhas = File.ReadAllLines(caminho);

            foreach (var linha in linhas)
            {
                var partes = linha.Split('|');

                if (partes.Length < 5)
                    continue;

                if (!int.TryParse(partes[0].Replace("Id:", "").Trim(), out int id))
                    continue;

                var user = new User(id)
                {
                    Nome = partes[1].Replace("Nome:", "").Trim(),
                    Email = partes[2].Replace("Email:", "").Trim(),
                    Telefone = partes[3].Replace("Telefone:", "").Trim(),
                    Senha = partes[4].Replace("Senha:", "").Trim()
                };

                MemoriaUsuarios.Lista.Add(user);
            }
        }

        // ✔ SALVAR NOVO USUÁRIO
        public bool Salvar(User user, out string mensagemErro)
        {
            mensagemErro = "";

            if (string.IsNullOrWhiteSpace(user.Nome) ||
                string.IsNullOrWhiteSpace(user.Email) ||
                string.IsNullOrWhiteSpace(user.Senha))
            {
                mensagemErro = "Nome, Email e Senha são obrigatórios.";
                return false;
            }

            if (BuscarPorEmail(user.Email) != null)
            {
                mensagemErro = "Este email já está cadastrado.";
                return false;
            }

            if (SenhaExiste(user.Senha))
            {
                mensagemErro = "Esta senha já está em uso.";
                return false;
            }

            MemoriaUsuarios.Lista.Add(user);

            SalvarArquivo(); // 🔥 grava no txt

            return true;
        }

        // 🔥 ATUALIZAR USUÁRIO (NOVO)
        public void Atualizar(User user)
        {
            var usuarioExistente = MemoriaUsuarios.Lista
                .FirstOrDefault(u => u.Id == user.Id);

            if (usuarioExistente != null)
            {
                usuarioExistente.Nome = user.Nome;
                usuarioExistente.Email = user.Email;
                usuarioExistente.Telefone = user.Telefone;
                usuarioExistente.Senha = user.Senha;
            }

            SalvarArquivo();
        }

        // 🔥 SALVAR NO TXT
        public void SalvarArquivo()
        {
            string pastaProjeto = Path.GetFullPath(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\")
            );

            string caminho = Path.Combine(pastaProjeto, "cadastroUsers", "cadastroUsers.txt");

            Directory.CreateDirectory(Path.GetDirectoryName(caminho));

            List<string> linhas = new List<string>();

            foreach (var u in MemoriaUsuarios.Lista)
            {
                string linha =
                    $"Id:{u.Id} |Nome:{u.Nome} |Email:{u.Email} |Telefone:{u.Telefone} |Senha:{u.Senha}";

                linhas.Add(linha);
            }

            File.WriteAllLines(caminho, linhas);
        }

        public bool SenhaExiste(string senha)
        {
            return MemoriaUsuarios.Lista.Any(u => u.Senha == senha);
        }

        public User BuscarPorEmail(string email)
        {
            return MemoriaUsuarios.Lista
                .FirstOrDefault(u => u.Email == email);
        }
    }
}