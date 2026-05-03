namespace Teste.Model
{
    // Uma classe estática para guardar informações enquanto o programa estiver aberto
    public static class Sessao
    {
        // Aqui vai ficar salvo o nome (ou Login/ID) do cliente atual
        public static string UsuarioLogado { get; set; } = "";
    }
}