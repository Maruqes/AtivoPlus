using System;
using System.Threading.Tasks;
using Xunit;
using AtivoPlus.Data;
using AtivoPlus.Logic;
using Microsoft.EntityFrameworkCore;
using AtivoPlus.Models;

namespace AtivoPlus.Tests
{
    public partial class UnitTests
    {
        // Use an in-memory database to isolate tests.


        [Fact]
        public async Task AtivoFinanceiroTeste()
        {
            FinnhubLogic.StartFinnhubLogic();

            Decimal? spy = await FinnhubLogic.GetETF("SPY");
            Decimal? aapl = await FinnhubLogic.GetStock("AAPL");
            Decimal? btc = await FinnhubLogic.GetCrypto("BTC");
            if (spy == null || aapl == null || btc == null)
            {
                Assert.True(false);
            }

            Assert.True(spy.Value > 0);
            Assert.True(aapl.Value > 0);
            Assert.True(btc.Value > 0);
        }
    }
}