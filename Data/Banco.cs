using Microsoft.Data.Sqlite;

namespace Projeto_do_semestre.Data
{
    public class Banco
    {
        private static string caminho = "Data Source=agencia.db";

        public static SqliteConnection obterConexao()
        {
            SqliteConnection conexao = new SqliteConnection(caminho);
            conexao.Open();

            return conexao;
        }

        public static void CriarBanco()
        {
            using SqliteConnection conexao = obterConexao();

            string sql = """
                CREATE TABLE IF NOT EXISTS Produto(
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    nome TEXT NOT NULL,
                    descricao TEXT NOT NULL,
                    categoria TEXT NOT NULL,
                    quantidade INTEGER NOT NULL,
                    preco REAL NOT NULL,
                    imagem TEXT NOT NULL DEFAULT ''
                )
            """;

            using SqliteCommand cmd = new SqliteCommand(sql, conexao);

            cmd.ExecuteNonQuery();

            // Verificar se a coluna imagem já existe

            bool colunaExiste = false;

            using (var verificar = conexao.CreateCommand())
            {
                verificar.CommandText = "PRAGMA table_info(Produto)";

                using var reader = verificar.ExecuteReader();

                while (reader.Read())
                {
                    if (reader.GetString(1) == "imagem")
                    {
                        colunaExiste = true;
                        break;
                    }
                }
            }

            // Adicionar a coluna caso ainda não exista

            if (!colunaExiste)
            {
                using var alterar = conexao.CreateCommand();

                alterar.CommandText = """
                    ALTER TABLE Produto
                    ADD COLUMN imagem TEXT NOT NULL DEFAULT ''
                    """;

                alterar.ExecuteNonQuery();
            }
        }
    }
}