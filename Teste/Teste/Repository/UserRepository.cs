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
        // 🔥 CARREGAR DO ARQUIVO (Versão Corrigida e Protegida)
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
                if (string.IsNullOrWhiteSpace(linha)) continue;

                var partes = linha.Split('|');

                if (partes.Length < 5)
                    continue;

                // Limpa a string de forma mais segura contra espaços extras
                string idTexto = partes[0].ToLower().Replace("id:", "").Trim();

                if (!int.TryParse(idTexto, out int id))
                    continue;

                // Usa o construtor correto passando o ID lido!
                var user = new User(id)
                {
                    Nome = partes[1].ToLower().Contains("nome:") ? partes[1].Substring(partes[1].IndexOf(':') + 1).Trim() : partes[1].Trim(),
                    Email = partes[2].ToLower().Contains("email:") ? partes[2].Substring(partes[2].IndexOf(':') + 1).Trim() : partes[2].Trim(),
                    Telefone = partes[3].ToLower().Contains("telefone:") ? partes[3].Substring(partes[3].IndexOf(':') + 1).Trim() : partes[3].Trim(),
                    Senha = partes[4].ToLower().Contains("senha:") ? partes[4].Substring(partes[4].IndexOf(':') + 1).Trim() : partes[4].Trim()
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