using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using AtivoPlus.Logic;

namespace AtivoPlus.Data
{
    public partial class AppDbContext : DbContext
    {
        public DbSet<ImovelArrendado> ImovelArrendados { get; set; }

        public async Task<bool> CreateImovelArrendado(int ativoFinanceiroId, string morada, string designacao, string localizacao, decimal valorImovel, decimal valorRenda, decimal valorMensalCondominio, decimal valorAnualDespesasEstimadas, DateTime? dataCriacao = null)
        {
            if (dataCriacao == null)
            {
                dataCriacao = DateTime.UtcNow;
            }
            ImovelArrendado imovel = new ImovelArrendado
            {
                AtivoFinaceiroId = ativoFinanceiroId,
                MoradaId = morada,
                Designacao = designacao,
                Localizacao = localizacao,
                ValorImovel = valorImovel,
                ValorRenda = valorRenda,
                ValorMensalCondominio = valorMensalCondominio,
                ValorAnualDespesasEstimadas = valorAnualDespesasEstimadas,
                DataCriacao = dataCriacao.Value
            };

            await ImovelArrendados.AddAsync(imovel);
            await SaveChangesAsync();
            return true;
        }

        public async Task<ImovelArrendado?> GetImovelArrendadoById(int imovelArrendadoId)
        {
            return await ImovelArrendados.FirstOrDefaultAsync(i => i.Id == imovelArrendadoId);
        }

        public async Task<List<ImovelArrendado>> GetImovelArrendadosByAtivoFinanceiroId(int ativoFinanceiroId)
        {
            return await ImovelArrendados.Where(i => i.AtivoFinaceiroId == ativoFinanceiroId).ToListAsync();
        }


        public async Task<bool> UpdateImovelArrendado(int imovelArrendadoId, string? morada = null, string? designacao = null, string? localizacao = null, decimal? valorImovel = null, decimal? valorRenda = null, decimal? valorMensalCondominio = null, decimal? valorAnualDespesasEstimadas = null)
        {
            var imovel = await ImovelArrendados.FirstOrDefaultAsync(i => i.Id == imovelArrendadoId);
            if (imovel == null)
            {
                return false;
            }
            if (morada != null)
            {
                imovel.MoradaId = morada;
            }
            if (designacao != null)
            {
                imovel.Designacao = designacao;
            }
            if (localizacao != null)
            {
                imovel.Localizacao = localizacao;
            }
            if (valorImovel.HasValue)
            {
                imovel.ValorImovel = valorImovel.Value;
            }
            if (valorRenda.HasValue)
            {
                imovel.ValorRenda = valorRenda.Value;
            }
            if (valorMensalCondominio.HasValue)
            {
                imovel.ValorMensalCondominio = valorMensalCondominio.Value;
            }
            if (valorAnualDespesasEstimadas.HasValue)
            {
                imovel.ValorAnualDespesasEstimadas = valorAnualDespesasEstimadas.Value;
            }
            await SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteImovelArrendado(int imovelArrendadoId)
        {
            var imovel = await ImovelArrendados.FirstOrDefaultAsync(i => i.Id == imovelArrendadoId);
            if (imovel == null)
            {
                return false;
            }
            ImovelArrendados.Remove(imovel);
            await SaveChangesAsync();
            return true;
        }

        public async Task<List<ImovelArrendado>> GetAllImovelArrendados()
        {
            return await ImovelArrendados.ToListAsync();
        }

        public async Task<int?> GetAtivoFinanceiroIdFromImovelArrendado(int imovelArrendadoId)
        {
            var imovel = await ImovelArrendados.FirstOrDefaultAsync(i => i.Id == imovelArrendadoId);
            return imovel?.AtivoFinaceiroId;
        }

        public async Task<int?> GetUserIdFromImovelArrendado(int imovelArrendadoId)
        {
            var imovel = await ImovelArrendados.FirstOrDefaultAsync(i => i.Id == imovelArrendadoId);
            if (imovel == null) return null;

            var ativo = await AtivoFinanceiros.FirstOrDefaultAsync(a => a.Id == imovel.AtivoFinaceiroId);
            return ativo?.UserId;
        }

        public async Task<List<ImovelArrendado>> GetImovelArrendadosByUserId(int userId)
        {
            var ativos = await AtivoFinanceiros.Where(a => a.UserId == userId).ToListAsync();
            var resultado = new List<ImovelArrendado>();
            foreach (var ativo in ativos)
            {
                var imoveis = await ImovelArrendados.Where(i => i.AtivoFinaceiroId == ativo.Id).ToListAsync();
                resultado.AddRange(imoveis);
            }
            return resultado;
        }

        public async Task<List<ImovelArrendado>> GetImovelArrendadoByAtivoFinanceiroId(int ativoFinanceiroId)
        {
            return await ImovelArrendados.Where(i => i.AtivoFinaceiroId == ativoFinanceiroId).ToListAsync();
        }
    }
}
