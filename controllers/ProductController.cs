using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;

namespace AtivoPlus.Controllers
{
    [Route("api/[controller]")] // A API está definida em "api/produto"
    [ApiController] // Indica que este é um Controller de API
    public class ProdutoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProdutoController(AppDbContext context)
        {
            _context = context;
        }

        // 📌 GET: api/produto → Retorna todos os produtos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> ObterProdutos()
        {
            return await _context.Produtos.ToListAsync();
        }

        // 📌 GET: api/produto/{id} → Retorna um único produto por ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> ObterProduto(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
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

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObterProduto), new { id = produto.Id }, produto);
        }

        // 📌 PUT: api/produto/{id} → Atualiza um produto existente
        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarProduto(int id, [FromBody] Produto produtoDto)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
                return NotFound("Produto não encontrado.");

            produto.Nome = produtoDto.Nome;
            produto.Preco = produtoDto.Preco;

            await _context.SaveChangesAsync();
            return NoContent(); // Retorna 204 No Content se for bem-sucedido
        }

        // 📌 DELETE: api/produto/{id} → Apaga um produto pelo ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> ApagarProduto(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
                return NotFound("Produto não encontrado.");

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();

            return NoContent(); // Retorna 204 No Content se for bem-sucedido
        }
    }
}
