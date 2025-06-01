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
        public int UserId { get; set; }
        public int AtivoFinaceiroId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal MontanteInvestido { get; set; }
        public string AtivoSigla { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
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


        [HttpPost("adicionar")]
        public async Task<ActionResult> AdicionarFundoInvestimento([FromBody] FundoInvestimentoRequest fundoInvestimento)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await FundoInvestimentoLogic.AdicionarFundoInvestimento(db, fundoInvestimento, username);
        }

        [HttpDelete("remover")]
        public async Task<ActionResult> RemoverFundoInvestimento(int fundoInvestimentoID)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await FundoInvestimentoLogic.RemoverFundoInvestimento(db, fundoInvestimentoID, username);
        }

        [HttpGet("getAllByUser")]
        public async Task<ActionResult<List<FundoInvestimento>>> GetAllFundoInvestimentos()
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await FundoInvestimentoLogic.GetAllFundoInvestimentos(db, username);
        }

        [HttpGet("getLucroById")]
        public async Task<ActionResult<LucroReturn>> GetLucroById(int fundoInvestimentoId)
        {
            string username = UserLogic.CheckUserLoggedRequest(Request);
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }
            return await FundoInvestimentoLogic.GetLucroById(db, fundoInvestimentoId, username);
        }

        // [HttpGet("commodities")]
        // public ActionResult GetCommodities()
        // {
        //     var jsonFiles = TwelveDataLogic.GetCachedJsonFile("commodities.json");
        //     if (jsonFiles == null)
        //     {
        //         return NotFound("Ficheiro commodities.json não encontrado");
        //     }
        //     string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(jsonFiles);
        //     return Content(jsonContent, "application/json");
        // }

        // [HttpGet("cryptocurrencies")]
        // public ActionResult GetCryptocurrencies()
        // {
        //     var jsonFiles = TwelveDataLogic.GetCachedJsonFile("cryptocurrencies.json");
        //     if (jsonFiles == null)
        //     {
        //         return NotFound("Ficheiro cryptocurrencies.json não encontrado");
        //     }
        //     string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(jsonFiles);
        //     return Content(jsonContent, "application/json");
        // }

        // [HttpGet("etfs")]
        // public ActionResult GetEtfs()
        // {
        //     var jsonFiles = TwelveDataLogic.GetCachedJsonFile("etfs.json");
        //     if (jsonFiles == null)
        //     {
        //         return NotFound("Ficheiro etfs.json não encontrado");
        //     }
        //     string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(jsonFiles);
        //     return Content(jsonContent, "application/json");
        // }

        // [HttpGet("forex")]
        // public ActionResult GetForexPairs()
        // {
        //     var jsonFiles = TwelveDataLogic.GetCachedJsonFile("forex_pairs.json");
        //     if (jsonFiles == null)
        //     {
        //         return NotFound("Ficheiro forex_pairs.json não encontrado");
        //     }
        //     string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(jsonFiles);
        //     return Content(jsonContent, "application/json");
        // }

        // [HttpGet("indices")]
        // public ActionResult GetIndices()
        // {
        //     var jsonFiles = TwelveDataLogic.GetCachedJsonFile("indices.json");
        //     if (jsonFiles == null)
        //     {
        //         return NotFound("Ficheiro indices.json não encontrado");
        //     }
        //     string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(jsonFiles);
        //     return Content(jsonContent, "application/json");
        // }

        // [HttpGet("stocks")]
        // public ActionResult GetStocks()
        // {
        //     var jsonFiles = TwelveDataLogic.GetCachedJsonFile("stocks.json");
        //     if (jsonFiles == null)
        //     {
        //         return NotFound("Ficheiro stocks.json não encontrado");
        //     }
        //     string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(jsonFiles);
        //     return Content(jsonContent, "application/json");
        // }

        // [HttpGet("search")]
        // public ActionResult SearchTerm(String term, int length = 10)
        // {
        //     try
        //     {
        //         List<JObject> ret = TwelveDataLogic.SearchJsonFiles(term, length);
        //         if (ret == null)
        //         {
        //             return NotFound("Commodities data file not found.");
        //         }
        //         string jsonContent = Newtonsoft.Json.JsonConvert.SerializeObject(ret);
        //         return Content(jsonContent, "application/json");
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, $"Error reading commodities data: {ex.Message}");
        //     }
        // }

    }
}