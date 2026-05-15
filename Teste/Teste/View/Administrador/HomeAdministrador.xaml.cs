using System.Windows.Controls;
using Teste.Model;

namespace Teste.View
{
    public partial class HomeAdministrador : UserControl
    {
        private User usuarioAdmin;

        public HomeAdministrador(User user)
        {
            InitializeComponent();

            usuarioAdmin = user;

            BoasVindasTexto.Text =
                $"Bem-vindo de volta, {user.Nome}! Aqui está o resumo de hoje.";
        }
    }
}