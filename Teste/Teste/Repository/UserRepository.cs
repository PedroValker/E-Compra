using System;
using System.Linq;
using Teste.Model;
using System.IO;
using System.Collections.Generic;

namespace Teste.Repository
{
    class UserRepository
    {
        private static int _ultimoId = 0;

        // =========================================
        // CARREGAR DO ARQUIVO
        // =========================================
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

                var user = new User
                {
                    Id = id,
                    Nome = partes[1].Replace("Nome:", "").Trim(),
                    Email = partes[2].Replace("Email:", "").Trim(),
                    Telefone = partes[3].Replace("Telefone:", "").Trim(),
                    Senha = partes[4].Replace("Senha:", "").Trim()
                };

                MemoriaUsuarios.Lista.Add(user);

                // 🔥 controla o maior ID
                if (id > _ultimoId)
                    _ultimoId = id;
            }
        }

        // =========================================
        // SALVAR (SÓ MEMÓRIA)
        // =========================================
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

            // 🔥 AQUI GARANTE ID CORRETO
            user.Id = ++_ultimoId;

            MemoriaUsuarios.Lista.Add(user);

            return true;
        }

        // =========================================
        // ATUALIZAR (SÓ MEMÓRIA)
        // =========================================
        public void Atualizar(User user)
        {
            var usuarioExistente = MemoriaUsuarios.Lista
                .FirstOrDefault(u => u.Id == user.Id);

            if (usuarioExistente == null)
                return;

            usuarioExistente.Nome = user.Nome;
            usuarioExistente.Email = user.Email;
            usuarioExistente.Telefone = user.Telefone;
            usuarioExistente.Senha = user.Senha;
            usuarioExistente.FotoPerfil = user.FotoPerfil;
        }

        // =========================================
        // UTILITÁRIOS
        // =========================================
        public bool SenhaExiste(string senha)
        {
            return MemoriaUsuarios.Lista.Any(u => u.Senha == senha);
        }

        public User BuscarPorEmail(string email)
        {
            return MemoriaUsuarios.Lista.FirstOrDefault(u => u.Email == email);
        }
    }
}