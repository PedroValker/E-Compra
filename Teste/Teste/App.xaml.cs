using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using Teste.Repository;

namespace Teste
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Carrega usuários
            UserRepository repoUsers = new UserRepository();
            repoUsers.CarregarDoArquivo();

            // 2. Carrega produtos (⚠️ TEM QUE SER ANTES DAS CESTAS)
            ProdutoRepository repoProdutos = new ProdutoRepository();
            repoProdutos.CarregarDoArquivo();

            // 3. 🔥 Carrega Cestas
            CestaRepository repoCestas = new CestaRepository();
            repoCestas.CarregarDoArquivo();
            //4. 🔥 Carrega Carrinhos
            CarrinhoRepository repoCarrinho = new CarrinhoRepository();
            repoCarrinho.CarregarDoArquivo();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                // 🔥 Usamos os repositórios que já sabem salvar no formato certinho!
                ProdutoRepository repoProdutos = new ProdutoRepository();
                repoProdutos.AtualizarArquivoTxt();

                CestaRepository repoCestas = new CestaRepository();
                repoCestas.AtualizarArquivoTxt();

                CarrinhoRepository repoCarrinho = new CarrinhoRepository();
                repoCarrinho.AtualizarArquivoTxt();

                // Mantenho a sua lógica de salvar usuários do jeito que você fez
                string pastaProjeto = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
                SalvarUsuarios(pastaProjeto);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar dados ao sair: " + ex.Message);
            }

            base.OnExit(e);
        }

        // Mantive apenas o de Usuários, pois Produtos e Cestas agora se salvam sozinhos com os Repositórios
        private void SalvarUsuarios(string pastaProjeto)
        {
            string pasta = Path.Combine(pastaProjeto, "cadastroUsers");

            if (!Directory.Exists(pasta))
                Directory.CreateDirectory(pasta);

            string arquivo = Path.Combine(pasta, "cadastroUsers.txt");

            List<string> linhas = new List<string>();

            foreach (var user in MemoriaUsuarios.Lista)
            {
                // Se futuramente o UserRepository der erro ao carregar, lembre-se de 
                // verificar se ele está esperando esse formato cheio de textos "Id:", "Nome:"
                linhas.Add($"Id:{user.Id} | Nome:{user.Nome} | Email:{user.Email} | Telefone:{user.Telefone} | Senha:{user.Senha} | Data:{user.DataCriacao}");
            }

            File.WriteAllLines(arquivo, linhas, Encoding.UTF8);
        }
    }
}