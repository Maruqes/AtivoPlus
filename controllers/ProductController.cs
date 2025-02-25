using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;

//ou app a usar api ou da par por razor em cima desta merda

namespace AtivoPlus.Controllers
{
    [Route("api/produto")] // A API está definida em "api/produto"
    [ApiController] // Indica que este é um Controller de API
    public class ProdutoController : ControllerBase
    {
        private readonly AppDbContext db;

        public ProdutoController(AppDbContext context)
        {
            db = context;
        }



        // 📌 GET: api/produto → Retorna todos os produtos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> ObterProdutos()
        {
            return await db.Produtos.ToListAsync();
        }

        // 📌 GET: api/produto/{id} → Retorna um único produto por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> ObterProduto(int id)
        {
            var produto = await db.Produtos.FindAsync(id);
            if (produto == null)
                return NotFound("Produto não encontrado.");
            return produto;
        }

        // 📌 POST: api/produto → Cria um novo produto
        [HttpPost]
        public async Task<ActionResult<Produto>> CriarProduto([FromBody] Produto produtoDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var produto = new Produto
            {
                Nome = produtoDto.Nome,
                Preco = produtoDto.Preco,
            };

            db.Produtos.Add(produto);
            await db.SaveChangesAsync();

            return CreatedAtAction(nameof(ObterProduto), new { id = produto.Id }, produto);
        }

        // 📌 PUT: api/produto/{id} → Atualiza um produto existente
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarProduto(int id, [FromBody] Produto produtoDto)
        {
            var produto = await db.Produtos.FindAsync(id);
            if (produto == null)
                return NotFound("Produto não encontrado.");

            produto.Nome = produtoDto.Nome;
            produto.Preco = produtoDto.Preco;

            await db.SaveChangesAsync();
            return NoContent(); // Retorna 204 No Content se for bem-sucedido
        }

        // 📌 DELETE: api/produto/{id} → Apaga um produto pelo ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> ApagarProduto(int id)
        {
            var produto = await db.Produtos.FindAsync(id);
            if (produto == null)
                return NotFound("Produto não encontrado.");

            db.Produtos.Remove(produto);
            await db.SaveChangesAsync();

            return NoContent(); // Retorna 204 No Content se for bem-sucedido
        }
    }
}
