using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Data;
using AtivoPlus.Models;
using AtivoPlus.Logic;
using Newtonsoft.Json.Linq;

//ou app a usar api ou da par por razor em cima desta merda

namespace AtivoPlus.Controllers
{

    public class FundoInvestimentoRequest
    {
        /// <summary>
        /// ID do utilizador a quem o ativo será atribuído.
        /// Utiliza -1 para indicar o utilizador atualmente autenticado.  
        /// Qualquer outro ID só pode ser usado por administradores.
        /// </summary>
        public int UserId { get; set; }
        public int AtivoFinaceiroId { get; set; }
        public int TipoAtivoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal MontanteInvestido { get; set; }
        public float TaxaJuro { get; set; }
        public Boolean TaxaFixa { get; set; }
        public string AtivoSigla { get; set; } = string.Empty;
        public float TaxaImposto { get; set; }
    }

    public class FundoInvestimentoRequestEdit
    {
        public int UserId { get; set; }
        public int FundoInvestimentoID { get; set; }
        public int TipoAtivoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal MontanteInvestido { get; set; }
        public float TaxaJuro { get; set; }
        public Boolean TaxaFixa { get; set; }
        public string AtivoSigla { get; set; } = string.Empty;
        public float TaxaImposto { get; set; }
    }

    [Route("api/fundoinvestimento")]
    [ApiController] // Indica que este é um Controller de API
    public class FundoInvestimentoController : ControllerBase
    {
        private readonly AppDbContext db;

        public FundoInvestimentoController(AppDbContext context)
        {
            db = context;
        }

        [HttpPut("adicionar")]
        public async Task<ActionResult> AdicionarFundoInvestimento([FromBody] FundoInvestimentoRequest fundo)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await FundoInvestimentoLogic.AdicionarFundoInvestimento(username, fundo, db);
        }

        [HttpGet("editar")]
        public async Task<ActionResult> EditarFundoInvestimento([FromBody] FundoInvestimentoRequestEdit fundo)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await FundoInvestimentoLogic.EditarFundoInvestimento(username, fundo, db);
        }

        [HttpGet("get")]
        public async Task<ActionResult> GetFundoInvestimento(int ativoFinanceiroId)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            FundoInvestimento? fundo = await db.GetFundoInvestimento(ativoFinanceiroId);
            if (fundo == null)
            {
                return NotFound();
            }

            AtivoFinanceiro? ativoFinanceiro = await db.GetAtivoFinanceiroById(ativoFinanceiroId);
            if (ativoFinanceiro == null)
            {
                return NotFound();
            }

            if (ativoFinanceiro.UserId != (await db.GetUserByUsername(username))!.Id)
            {
                return Unauthorized();
            }
            return Ok(fundo);
        }

        [HttpGet("getall")]
        public async Task<ActionResult> GetAllFundoInvestimento()
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            List<FundoInvestimento> fundos = await db.fundoInvestimentos.ToListAsync();
            if (fundos == null)
            {
                return NotFound();
            }

            List<FundoInvestimento> returnFundos = new List<FundoInvestimento>();

            foreach (FundoInvestimento fundo in fundos)
            {
                AtivoFinanceiro? ativoFinanceiro = await db.GetAtivoFinanceiroById(fundo.AtivoFinaceiroId);
                if (ativoFinanceiro == null)
                {
                    return NotFound();
                }

                if (ativoFinanceiro.UserId != (await db.GetUserByUsername(username))!.Id)
                {
                    return Unauthorized();
                }
                returnFundos.Add(fundo);
            }

            return Ok(fundos);
        }

        [HttpDelete("delete")]
        public async Task<ActionResult> DeleteFundoInvestimento(int ativoFinanceiroId)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await FundoInvestimentoLogic.DeleteFundoInvestimento(username, ativoFinanceiroId, db);
        }

        [HttpGet("getCommodities")]
        public async Task<ActionResult> GetCommodities()
        {
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "TwelveJson", "commodities.json");
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Commodities data file not found.");
                }

                string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
                return Content(jsonContent, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error reading commodities data: {ex.Message}");
            }
        }

        [HttpGet("getCryptos")]
        public async Task<ActionResult> GetCrypto()
        {
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "TwelveJson", "cryptocurrencies.json");
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Commodities data file not found.");
                }

                string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
                return Content(jsonContent, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error reading commodities data: {ex.Message}");
            }
        }

        [HttpGet("getEtfs")]
        public async Task<ActionResult> GetEtfs()
        {
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "TwelveJson", "etfs.json");
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Commodities data file not found.");
                }

                string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
                return Content(jsonContent, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error reading commodities data: {ex.Message}");
            }
        }

        [HttpGet("getForex")]
        public async Task<ActionResult> GetForex()
        {
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "TwelveJson", "forex_pairs.json");
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Commodities data file not found.");
                }

                string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
                return Content(jsonContent, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error reading commodities data: {ex.Message}");
            }
        }

        [HttpGet("getIndices")]
        public async Task<ActionResult> GetIndices()
        {
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "TwelveJson", "indices.json");
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Commodities data file not found.");
                }

                string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
                return Content(jsonContent, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error reading commodities data: {ex.Message}");
            }
        }

        [HttpGet("getStocks")]
        public async Task<ActionResult> GetStocks()
        {
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "TwelveJson", "stocks.json");
                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound("Commodities data file not found.");
                }

                string jsonContent = await System.IO.File.ReadAllTextAsync(filePath);
                return Content(jsonContent, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error reading commodities data: {ex.Message}");
            }
        }

        [HttpGet("searchTerm")]
        public ActionResult SearchTerm(String term, int length = 50)
        {
            try
            {
                List<JObject> ret = TwelveDataLogic.SearchJsonFiles(term, length);
                if (ret == null)
                {
                    return NotFound("Commodities data file not found.");
                }

                string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(ret);
                return Content(jsonContent, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error reading commodities data: {ex.Message}");
            }
        }
    }
}