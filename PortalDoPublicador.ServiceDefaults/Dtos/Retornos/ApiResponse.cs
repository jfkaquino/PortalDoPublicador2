namespace PortalDoPublicador.ServiceDefaults.Dtos.Retornos;

public class ApiResponse<T>
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public T? Dados { get; set; }

    public static ApiResponse<T> Ok(T dados, string mensagem = "") 
        => new() { Sucesso = true, Dados = dados, Mensagem = mensagem };

    public static ApiResponse<T> Erro(string mensagem) 
        => new() { Sucesso = false, Mensagem = mensagem };
}