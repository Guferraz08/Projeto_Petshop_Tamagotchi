using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Projeto_do_semestre.Data;
using Projeto_do_semestre.Models;

namespace Projeto_do_semestre.Controllers
{
    public class AreaRestritaController : Controller
    {
        private readonly IWebHostEnvironment _ambiente;

        public AreaRestritaController(IWebHostEnvironment ambiente)
        {
            _ambiente = ambiente;
        }

        // MÉTODO PARA SALVAR A IMAGEM

        private string SalvarImagem(IFormFile? arquivoImagem)
        {
            if (arquivoImagem == null || arquivoImagem.Length == 0)
            {
                return string.Empty;
            }

            string extensao = Path.GetExtension(
                arquivoImagem.FileName).ToLowerInvariant();

            string[] extensoesPermitidas =
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            if (!extensoesPermitidas.Contains(extensao))
            {
                throw new InvalidOperationException(
                    "Formato de imagem não permitido.");
            }

            if (arquivoImagem.Length > 5 * 1024 * 1024)
            {
                throw new InvalidOperationException(
                    "A imagem deve ter no máximo 5 MB.");
            }

            string pasta = Path.Combine(
                _ambiente.WebRootPath, "imagens", "produtos");

            Directory.CreateDirectory(pasta);

            string nomeArquivo = Guid.NewGuid().ToString() + extensao;

            string caminhoCompleto = Path.Combine(pasta, nomeArquivo);

            using (var stream = new FileStream(
                caminhoCompleto, FileMode.CreateNew))
            {
                arquivoImagem.CopyTo(stream);
            }

            return "/imagens/produtos/" + nomeArquivo;
        }

        public IActionResult Index()
        {
            return View();
        }

        // LISTAR PRODUTOS

        public IActionResult AdmVeiculos()
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

        public IActionResult AdmVendas()
        {
            return View();
        }

        // CADASTRAR PRODUTO

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SalvarProduto(
            Produto produto, IFormFile? arquivoImagem)
        {
            if (!ModelState.IsValid ||
                produto.Preco < 0 ||
                produto.Quantidade < 0)
            {
                TempData["mensagem"] = "Verifique os dados do produto.";

                return RedirectToAction("AdmVeiculos");
            }

            try
            {
                produto.Imagem = SalvarImagem(arquivoImagem);
            }
            catch (InvalidOperationException ex)
            {
                TempData["mensagem"] = ex.Message;

                return RedirectToAction("AdmVeiculos");
            }

            using var conexao = Banco.obterConexao();

            string sql = """
                INSERT INTO Produto
                (nome, descricao, categoria, quantidade, preco, imagem)
                VALUES
                (@nome, @descricao, @categoria, @quantidade, @preco, @imagem)
            """;

            using var cmd = conexao.CreateCommand();

            cmd.CommandText = sql;

            cmd.Parameters.AddWithValue("@nome", produto.Nome);
            cmd.Parameters.AddWithValue("@descricao", produto.Descricao);
            cmd.Parameters.AddWithValue("@categoria", produto.Categoria);
            cmd.Parameters.AddWithValue("@quantidade", produto.Quantidade);
            cmd.Parameters.AddWithValue("@preco", (double)produto.Preco);
            cmd.Parameters.AddWithValue("@imagem", produto.Imagem);

            cmd.ExecuteNonQuery();

            TempData["mensagem"] = "Produto cadastrado com sucesso!";

            return RedirectToAction("AdmVeiculos");
        }

        // EXCLUIR PRODUTO

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ExcluirProduto(int id)
        {
            using var conexao = Banco.obterConexao();

            string sql = "DELETE FROM Produto WHERE id = @id";

            using var cmd = conexao.CreateCommand();

            cmd.CommandText = sql;

            cmd.Parameters.AddWithValue("@id", id);

            int registros = cmd.ExecuteNonQuery();

            TempData["mensagem"] = registros > 0
                ? "Produto excluído com sucesso!"
                : "Produto não encontrado.";

            return RedirectToAction("AdmVeiculos");
        }

        // ABRIR TELA DE EDIÇÃO

        [HttpGet]
        public IActionResult EditarProduto(int id)
        {
            using var conexao = Banco.obterConexao();

            string sql = "SELECT * FROM Produto WHERE id = @id";

            using var cmd = conexao.CreateCommand();

            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                Produto produto = new Produto
                {
                    Id = reader.GetInt32(0),
                    Nome = reader.GetString(1),
                    Descricao = reader.GetString(2),
                    Categoria = reader.GetString(3),
                    Quantidade = reader.GetInt32(4),
                    Preco = reader.GetDecimal(5),
                    Imagem = reader.GetString(6)
                };

                return View(produto);
            }

            return NotFound();
        }

        // ATUALIZAR PRODUTO

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AtualizarProduto(
            Produto produto, IFormFile? arquivoImagem)
        {
            using var conexao = Banco.obterConexao();

            // Buscar a imagem atual no banco

            using var buscar = conexao.CreateCommand();

            buscar.CommandText =
                "SELECT imagem FROM Produto WHERE id = @id";

            buscar.Parameters.AddWithValue("@id", produto.Id);

            var imagemAtual = buscar.ExecuteScalar();

            if (imagemAtual == null)
            {
                return NotFound();
            }

            produto.Imagem = imagemAtual.ToString() ?? string.Empty;

            if (!ModelState.IsValid ||
                produto.Preco < 0 ||
                produto.Quantidade < 0)
            {
                ModelState.AddModelError(
                    "", "Verifique os dados do produto.");

                return View("EditarProduto", produto);
            }

            // Se selecionar uma nova foto, substituir o caminho

            if (arquivoImagem != null && arquivoImagem.Length > 0)
            {
                try
                {
                    produto.Imagem = SalvarImagem(arquivoImagem);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);

                    return View("EditarProduto", produto);
                }
            }

            string sql = """
                UPDATE Produto SET
                    nome = @nome,
                    descricao = @descricao,
                    categoria = @categoria,
                    quantidade = @quantidade,
                    preco = @preco,
                    imagem = @imagem
                WHERE id = @id
            """;

            using var cmd = conexao.CreateCommand();

            cmd.CommandText = sql;

            cmd.Parameters.AddWithValue("@id", produto.Id);
            cmd.Parameters.AddWithValue("@nome", produto.Nome);
            cmd.Parameters.AddWithValue("@descricao", produto.Descricao);
            cmd.Parameters.AddWithValue("@categoria", produto.Categoria);
            cmd.Parameters.AddWithValue("@quantidade", produto.Quantidade);
            cmd.Parameters.AddWithValue("@preco", (double)produto.Preco);
            cmd.Parameters.AddWithValue("@imagem", produto.Imagem);

            int registros = cmd.ExecuteNonQuery();

            if (registros == 0)
            {
                return NotFound();
            }

            TempData["mensagem"] = "Produto atualizado com sucesso!";

            return RedirectToAction("AdmVeiculos");
        }
    }
}