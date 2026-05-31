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

            // 2. Carrega produtos
            ProdutoRepository repoProdutos = new ProdutoRepository();
            repoProdutos.CarregarDoArquivo();

            // 3. Carrega Cestas
            CestaRepository repoCestas = new CestaRepository();
            repoCestas.CarregarDoArquivo();

            // 4. Carrega Carrinhos
            CarrinhoRepository repoCarrinho = new CarrinhoRepository();
            repoCarrinho.CarregarDoArquivo();

            // 🚀 O QUE FALTAVA: Carrega os pedidos existentes do TXT para a memória ao iniciar
            PedidoRepository repoPedidos = new PedidoRepository();
            repoPedidos.CarregarDoArquivo();
        }
        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                // ✔ Agora usamos o UserRepository para salvar os usuários! 
                // Ele vai usar o formato idêntico ao que ele mesmo lê.
                UserRepository repoUsers = new UserRepository();
                repoUsers.SalvarArquivo();
                PedidoRepository repoPedido = new PedidoRepository();
                repoPedido.AtualizarArquivoTxt();
                ProdutoRepository repoProdutos = new ProdutoRepository();
                repoProdutos.AtualizarArquivoTxt();

                CestaRepository repoCestas = new CestaRepository();
                repoCestas.AtualizarArquivoTxt();

                CarrinhoRepository repoCarrinho = new CarrinhoRepository();
                repoCarrinho.AtualizarArquivoTxt();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar dados ao sair: " + ex.Message);
            }

            base.OnExit(e);
        }
     
    }
}