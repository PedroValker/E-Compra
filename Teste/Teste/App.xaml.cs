using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using Teste.Model;
using Teste.Repository;

namespace Teste
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 1. Carrega usuários do TXT para a memória
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

            // 5. Carrega os pedidos existentes do TXT para a memória ao iniciar
            PedidoRepository repoPedidos = new PedidoRepository();
            repoPedidos.CarregarDoArquivo();

            // 🛡️ CORREÇÃO: A linha que chamava repo.Atualizar(Sessao.UsuarioLogado) foi removida daqui!
            // Não faz sentido atualizar o usuário logado antes mesmo de a tela de Login aparecer.
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                // Antes de fechar o arquivo físico, garantimos que o estado atual do usuário logado na tela
                // seja atualizado na lista estática/memória do repositório
                if (Sessao.UsuarioLogado != null)
                {
                    UserRepository repoUsers = new UserRepository();
                    repoUsers.Atualizar(Sessao.UsuarioLogado);
                }

                UserRepository repoFinal = new UserRepository();
                repoFinal.SalvarArquivo();

                PedidoRepository repoPedido = new PedidoRepository();
                repoPedido.AtualizarArquivoTxt();

                ProdutoRepository repoProdutos = new ProdutoRepository();
                repoProdutos.AtualizarArquivoTxt();

                CestaRepository repoCestas = new CestaRepository();
                repoCestas.AtualizarArquivoTxt();

                PedidoRepository repoPedidos = new PedidoRepository();
                repoPedidos.AtualizarArquivoTxt();
                UserRepository repo = new UserRepository();
                repo.Atualizar(Sessao.UsuarioLogado);
                CarrinhoRepository repoCarrinho = new CarrinhoRepository();
                repoCarrinho.AtualizarArquivoTxt();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao salvar dados ao fechar o aplicativo: " + ex.Message, "Erro no Fechamento", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            base.OnExit(e);
        }
    }
}