using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls; // Importante: Necessário para UserControl
using Teste.Model;
using Teste.Repository;

namespace Teste.View
{
    // 1. Mudamos de ": Window" para ": UserControl"
    public partial class FacaSeuPedidoView : UserControl
    {
        public ObservableCollection<Cesta> ListaCestas { get; set; }

        // 2. Removi o parâmetro "string nome" do construtor para bater com a chamada 
        // que você fez na TelaPrincipalCliente: ConteudoPrincipal.Content = new FacaSeuPedidoView();
        public FacaSeuPedidoView()
        {
            InitializeComponent();
            // Inicializa a lista
            ListaCestas = new ObservableCollection<Cesta>();

            // Define o DataContext para o Binding das Cestas funcionar
            this.DataContext = this;

            // Carrega os dados
            CarregarCestasDoBanco();
        }

        private void CarregarCestasDoBanco()
        {
            CestaRepository repo = new CestaRepository();
            repo.CarregarDoArquivo();

            ListaCestas.Clear();

            foreach (Cesta cesta in MemoriaCestas.Lista)
            {
                ListaCestas.Add(cesta);
            }
        }
    }
}