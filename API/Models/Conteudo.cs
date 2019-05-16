using System.ComponentModel.DataAnnotations;
namespace API.Models
{
    public class Conteudo
    {
        [Required]
        public string Mensagem { get; set; }
    }
}
