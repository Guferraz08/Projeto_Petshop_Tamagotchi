namespace Projeto_do_semestre.Models
{
    public class Produto
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public int Quantidade { get; set; }

        public decimal Preco { get; set; }

        public string Imagem { get; set; } = string.Empty;
    }
}