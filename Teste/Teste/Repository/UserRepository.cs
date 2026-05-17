using System;
using System.Linq;
using Teste.Model;
using System.IO;
using System.Collections.Generic;

namespace Teste.Repository
{
    class UserRepository
    {
        // 🔥 Caminho absoluto fixo no Windows para evitar problemas entre usuários
        private string ObterPastaImagensPerfil()
        {
            string caminhoRaiz = @"C:\TesteSistema\Dados\imagemUser";
            if (!Directory.Exists(caminhoRaiz))
            {
                Directory.CreateDirectory(caminhoRaiz);
            }
            return caminhoRaiz;
        }

        private string ObterCaminhoTxt()
        {
            string pastaProjeto = Path.GetFullPath(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\")
            );
            return Path.Combine(pastaProjeto, "cadastroUsers", "cadastroUsers.txt");
        }

        // 🔥 CARREGAR DO ARQUIVO (Versão Atualizada com FotoPerfil)
        // 🔥 CARREGAR DO ARQUIVO (Versão Corrigida para Carregar a Foto de Perfil)
        public void CarregarDoArquivo()
        {
            MemoriaUsuarios.Lista.Clear();
            string caminho = ObterCaminhoTxt();

            if (!File.Exists(caminho))
                return;

            var linhas = File.ReadAllLines(caminho);

            foreach (var linha in linhas)
            {
                if (string.IsNullOrWhiteSpace(linha)) continue;

                // Separa as colunas por '|'
                var partes = linha.Split('|');

                if (partes.Length < 5)
                    continue;

                // Dicionário para guardar as chaves (Id, Nome, Email, etc.) e seus respectivos valores
                var dadosUsuario = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var parte in partes)
                {
                    var divisaoChaveValor = parte.Split(new[] { ':' }, 2);
                    if (divisaoChaveValor.Length == 2)
                    {
                        string chave = divisaoChaveValor[0].Trim();
                        string valor = divisaoChaveValor[1].Trim();
                        dadosUsuario[chave] = valor;
                    }
                }

                // Tenta ler o ID da estrutura mapeada
                if (!dadosUsuario.TryGetValue("Id", out string idTexto) || !int.TryParse(idTexto, out int id))
                    continue;

                // Cria o usuário com os dados extraídos com total segurança contra nulos ou falta de colunas
                var user = new User(id)
                {
                    Nome = dadosUsuario.TryGetValue("Nome", out string nome) ? nome : "",
                    Email = dadosUsuario.TryGetValue("Email", out string email) ? email : "",
                    Telefone = dadosUsuario.TryGetValue("Telefone", out string telefone) ? telefone : "",
                    Senha = dadosUsuario.TryGetValue("Senha", out string senha) ? senha : ""
                };

                // 🔥 AQUI ESTÁ A CORREÇÃO DA FOTO:
                if (dadosUsuario.TryGetValue("FotoPerfil", out string foto) && foto != "null")
                {
                    user.FotoPerfil = foto;
                }
                else
                {
                    user.FotoPerfil = "";
                }

                MemoriaUsuarios.Lista.Add(user);
            }
        }
        // ✔ SALVAR NOVO USUÁRIO (Com processamento da imagem)
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

            // 🔥 PROCESSAR FOTO DE PERFIL ANTES DE SALVAR
            user.FotoPerfil = SalvarFotoNoCaminhoAbsoluto(user.FotoPerfil);

            MemoriaUsuarios.Lista.Add(user);
            SalvarArquivo();

            return true;
        }

        // 🔥 ATUALIZAR USUÁRIO
        public void Atuallizar(User user)
        {
            var usuarioExistente = MemoriaUsuarios.Lista
                .FirstOrDefault(u => u.Id == user.Id);

            if (usuarioExistente != null)
            {
                usuarioExistente.Nome = user.Nome;
                usuarioExistente.Email = user.Email;
                usuarioExistente.Telefone = user.Telefone;
                usuarioExistente.Senha = user.Senha;

                // 🔥 Atualiza a foto tratando o caminho absoluto
                usuarioExistente.FotoPerfil = SalvarFotoNoCaminhoAbsoluto(user.FotoPerfil);
            }

            SalvarArquivo();
        }

        // 🔥 SALVAR NO TXT (Incluindo a propriedade FotoPerfil)
        public void SalvarArquivo()
        {
            string caminho = ObterCaminhoTxt();
            Directory.CreateDirectory(Path.GetDirectoryName(caminho));

            List<string> linhas = new List<string>();

            foreach (var u in MemoriaUsuarios.Lista)
            {
                string foto = string.IsNullOrEmpty(u.FotoPerfil) ? "null" : u.FotoPerfil;

                // Adicionado "|FotoPerfil:" no final da linha do TXT
                string linha = $"Id:{u.Id} |Nome:{u.Nome} |Email:{u.Email} |Telefone:{u.Telefone} |Senha:{u.Senha} |FotoPerfil:{foto}";

                linhas.Add(linha);
            }

            File.WriteAllLines(caminho, linhas);
        }

        // 🔥 MÉTODO AUXILIAR: Faz a cópia segura da foto para a pasta absoluta C:\
        private string SalvarFotoNoCaminhoAbsoluto(string caminhoOrigem)
        {
            if (string.IsNullOrEmpty(caminhoOrigem) || !File.Exists(caminhoOrigem))
            {
                // Se o arquivo não existe ou já está salvo no caminho absoluto, mantém como está
                if (!string.IsNullOrEmpty(caminhoOrigem) && caminhoOrigem.StartsWith(@"C:\TesteSistema"))
                    return caminhoOrigem;

                return "";
            }

            try
            {
                string pastaDestino = ObterPastaImagensPerfil();
                string extensao = Path.GetExtension(caminhoOrigem);

                // Nome único baseado em Guid para evitar sobrescrever fotos de usuários diferentes
                string nomeArquivo = $"{Guid.NewGuid()}{extensao}";
                string caminhoDestinoCompleto = Path.Combine(pastaDestino, nomeArquivo);

                string origemAbsoluta = Path.GetFullPath(caminhoOrigem);

                if (!origemAbsoluta.Equals(caminhoDestinoCompleto, StringComparison.OrdinalIgnoreCase))
                {
                    // Stream seguro que não trava o arquivo original aberto na tela
                    using (var streamOrigem = new FileStream(origemAbsoluta, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var streamDestino = new FileStream(caminhoDestinoCompleto, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        streamOrigem.CopyTo(streamDestino);
                    }
                }

                return caminhoDestinoCompleto; // Retorna ex: "C:\TesteSistema\Dados\imagemUser\guid.jpg"
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao processar foto de perfil: " + ex.Message);
                return "";
            }
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