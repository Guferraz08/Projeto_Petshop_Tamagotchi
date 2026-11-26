namespace Projeto_do_semestre.Models
{
    public class Carro
    {
        public String Nome { get; set; } = String.Empty;
        public String Descricao { get; set; } = String.Empty;
        public int Ano { get; set; }
        public int Quilometragem { get; set; }

        public string Cambio { get; set; } = String.Empty;

        public double Preco { get; set; }


    }
}
