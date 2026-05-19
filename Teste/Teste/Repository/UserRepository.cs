using System;
using System.Linq;
using Teste.Model;
using System.IO;
using System.Collections.Generic;

namespace Teste.Repository
{
    class UserRepository
    {
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

        // 🚀 CARREGAR DO ARQUIVO (Versão Atualizada com FotoPerfil e Endereço)
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

                var partes = linha.Split('|');

                if (partes.Length < 5)
                    continue;

                var dadosUsuario = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var parte in partes)
                {
                    var divisaoChaveValor = parte.Split(new[] { ':' }, 2);
                    if (divisaoChaveValor.Length == 2)
                    {
                        // 🚀 A CORREÇÃO ESTÁ AQUI: Trim() na chave remove os espaços extras criados pelo ' | '
                        string chave = divisaoChaveValor[0].Trim();
                        string valor = divisaoChaveValor[1].Trim();
                        dadosUsuario[chave] = valor;
                    }
                }

                if (!dadosUsuario.TryGetValue("Id", out string idTexto) || !int.TryParse(idTexto, out int id))
                    continue;

                var user = new User(id)
                {
                    Nome = dadosUsuario.TryGetValue("Nome", out string nome) ? nome : "",
                    Email = dadosUsuario.TryGetValue("Email", out string email) ? email : "",
                    Telefone = dadosUsuario.TryGetValue("Telefone", out string telefone) ? telefone : "",
                    Senha = dadosUsuario.TryGetValue("Senha", out string senha) ? senha : ""
                };

                if (dadosUsuario.TryGetValue("FotoPerfil", out string foto) && foto != "null")
                {
                    user.FotoPerfil = foto;
                }
                else
                {
                    user.FotoPerfil = "";
                }

                // 🚀 AGORA VAI FUNCIONAR: Como as chaves estão limpas com Trim(), o dicionário vai encontrar os campos
                if (dadosUsuario.ContainsKey("CEP") || dadosUsuario.ContainsKey("Rua") ||
                    dadosUsuario.ContainsKey("Numero") || dadosUsuario.ContainsKey("Bairro"))
                {
                    // Adicionalmente, verificamos se o endereço salvo não é apenas um monte de strings vazias
                    string cep = dadosUsuario.TryGetValue("CEP", out string c) ? c : "";
                    string rua = dadosUsuario.TryGetValue("Rua", out string r) ? r : "";
                    string numero = dadosUsuario.TryGetValue("Numero", out string n) ? n : "";
                    string bairro = dadosUsuario.TryGetValue("Bairro", out string b) ? b : "";

                    // Só cria o objeto se o usuário realmente tiver preenchido algo
                    if (!string.IsNullOrWhiteSpace(cep) || !string.IsNullOrWhiteSpace(rua))
                    {
                        user.Endereco = new Endereco
                        {
                            CEP = cep,
                            Rua = rua,
                            Numero = numero,
                            Bairro = bairro
                        };
                    }
                }

                MemoriaUsuarios.Lista.Add(user);
            }
        }
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

            user.FotoPerfil = SalvarFotoNoCaminhoAbsoluto(user.FotoPerfil);

            MemoriaUsuarios.Lista.Add(user);
            SalvarArquivo();

            return true;
        }

        // 🚀 CORREÇÃO: Corrigido o nome de "Atuallizar" para "Atualizar"
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
                usuarioExistente.FotoPerfil = SalvarFotoNoCaminhoAbsoluto(user.FotoPerfil);

                // 🚀 NOVO: Atualiza a referência do Endereço
                if (user.Endereco != null)
                {
                    if (usuarioExistente.Endereco == null)
                    {
                        usuarioExistente.Endereco = new Endereco();
                    }

                    usuarioExistente.Endereco.CEP = user.Endereco.CEP;
                    usuarioExistente.Endereco.Rua = user.Endereco.Rua;
                    usuarioExistente.Endereco.Numero = user.Endereco.Numero;
                    usuarioExistente.Endereco.Bairro = user.Endereco.Bairro;
                }
            }

            SalvarArquivo();
        }

        // 🚀 SALVAR NO TXT (Incluindo as chaves do Endereço no final da linha)
        public void SalvarArquivo()
        {
            string caminho = ObterCaminhoTxt();
            Directory.CreateDirectory(Path.GetDirectoryName(caminho));

            List<string> linhas = new List<string>();

            foreach (var u in MemoriaUsuarios.Lista)
            {
                string foto = string.IsNullOrEmpty(u.FotoPerfil) ? "null" : u.FotoPerfil;

                // 🚀 NOVO: Trata campos vazios do subobjeto de endereço antes de salvar
                string cep = u.Endereco != null ? u.Endereco.CEP : "";
                string rua = u.Endereco != null ? u.Endereco.Rua : "";
                string numero = u.Endereco != null ? u.Endereco.Numero : "";
                string bairro = u.Endereco != null ? u.Endereco.Bairro : "";

                // Linha estendida dinamicamente preservando a estrutura de Chave:Valor |
                string linha = $"Id:{u.Id} |Nome:{u.Nome} |Email:{u.Email} |Telefone:{u.Telefone} |Senha:{u.Senha} |FotoPerfil:{foto} |CEP:{cep} |Rua:{rua} |Numero:{numero} |Bairro:{bairro}";

                linhas.Add(linha);
            }

            File.WriteAllLines(caminho, linhas);
        }

        private string SalvarFotoNoCaminhoAbsoluto(string caminhoOrigem)
        {
            if (string.IsNullOrEmpty(caminhoOrigem) || !File.Exists(caminhoOrigem))
            {
                if (!string.IsNullOrEmpty(caminhoOrigem) && caminhoOrigem.StartsWith(@"C:\TesteSistema"))
                    return caminhoOrigem;

                return "";
            }

            try
            {
                string pastaDestino = ObterPastaImagensPerfil();
                string extensao = Path.GetExtension(caminhoOrigem);

                string nomeArquivo = $"{Guid.NewGuid()}{extensao}";
                string caminhoDestinoCompleto = Path.Combine(pastaDestino, nomeArquivo);

                string origemAbsoluta = Path.GetFullPath(caminhoOrigem);

                if (!origemAbsoluta.Equals(caminhoDestinoCompleto, StringComparison.OrdinalIgnoreCase))
                {
                    using (var streamOrigem = new FileStream(origemAbsoluta, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var streamDestino = new FileStream(caminhoDestinoCompleto, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        streamOrigem.CopyTo(streamDestino);
                    }
                }

                return caminhoDestinoCompleto;
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