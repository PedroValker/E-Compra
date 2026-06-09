using CestaApp.Views;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Teste.Model;
using Teste.Repository;
using Teste.View.Administrador;

namespace Teste.View
{
    public partial class PrincipalAdministrador : Window
    {
        private string _nomeDoAdmin;

        public PrincipalAdministrador(string nomeAdmin)
        {
            InitializeComponent();

            _nomeDoAdmin = nomeAdmin;

            NomeAdminMenu.Text = $"Olá, {_nomeDoAdmin}";

            // Sincroniza a foto salva na memória do usuário logado ao abrir a tela
            if (Sessao.UsuarioLogado != null && !string.IsNullOrEmpty(Sessao.UsuarioLogado.FotoPerfil) && File.Exists(Sessao.UsuarioLogado.FotoPerfil))
            {
                try
                {
                    BitmapImage imgSalva = new BitmapImage();
                    imgSalva.BeginInit();
                    imgSalva.CacheOption = BitmapCacheOption.OnLoad;
                    imgSalva.UriSource = new Uri(Sessao.UsuarioLogado.FotoPerfil);
                    imgSalva.EndInit();
                    ImagemPerfilBrush.ImageSource = imgSalva;
                }
                catch { /* Fallback automático se falhar */ }
            }

            ConteudoPrincipal.Content = new HomeAdministrador(_nomeDoAdmin);
        }

        // 🚀 NOVO: Abre a tela de Edição de Perfil quando clicar no botão de engrenagem
        private void Perfil_Click(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new PerfilAdminView();
        }

        // 🚀 NOVO: Abre a tela de Edição de Perfil quando clicar em cima da foto ou do nome
        private void AbrirPerfil_Click(object sender, MouseButtonEventArgs e)
        {
            ConteudoPrincipal.Content = new PerfilAdminView();
        }

        private void AlterarFoto_Click(object sender, MouseButtonEventArgs e)
        {
            // Substituído para direcionar ao fluxo centralizado de Perfil
            ConteudoPrincipal.Content = new PerfilAdminView();
        }

        private void Inicio_Click(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new HomeAdministrador(_nomeDoAdmin);
        }

        private void Pedidos_Click(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new PedidosAdminView();
        }

        private void Cadastrar_Click(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new CadastroProduto();
        }

        private void Logoff_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Deseja realmente sair do sistema?", "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MainWindow login = new MainWindow();
                login.Show();
                this.Close();
            }
        }

        private void Sandy_Click(object sender, RoutedEventArgs e)
        {
            // Espaço reservado
        }

        private void Pendencias_Click(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new PendenciasView();
        }

        private void Cestas_Click(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new CadastroCesta();
        }

        private void Estatisticas_Click(object sender, RoutedEventArgs e)
        {
            // Espaço reservado para View de Relatórios/Estatísticas
        }

        private void Clientes_Click(object sender, RoutedEventArgs e)
        {
            ConteudoPrincipal.Content = new ClientesView();
        }
    }
}