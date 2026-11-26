using Microsoft.AspNetCore.Mvc;
using Projeto_do_semestre.Models;
using Projeto_do_semestre.Data;

namespace Projeto_do_semestre.Controllers
{
    public class AreaPublicaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult VeiculosImportados()
        {
            return View();
        }

        public IActionResult VeiculosNacionais()
        {
            List<Produto> produtos = new List<Produto>();

            using var conexao = Banco.obterConexao();

            string sql = "SELECT * FROM Produto";

            using var cmd = conexao.CreateCommand();
            cmd.CommandText = sql;

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                produtos.Add(new Produto
                {
                    Id = reader.GetInt32(0),
                    Nome = reader.GetString(1),
                    Descricao = reader.GetString(2),
                    Categoria = reader.GetString(3),
                    Quantidade = reader.GetInt32(4),
                    Preco = reader.GetDecimal(5),
                    Imagem = reader.GetString(6)
                });
            }

            return View(produtos);
        }
    }
}