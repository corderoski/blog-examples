using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Globalization;
using WebApiAutofacTest.Services;

namespace WebApiAutofacTest.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly IRandomService _randomService;

        public ValuesController(IRandomService randomService)
        {
            _randomService = randomService;
        }

        // GET api/values
        [HttpGet("{format?}")]
        public IEnumerable<string> Get(string format = null)
        {
            var culture = GetCultureCurrency(format);
            return new string[] {
                _randomService.GetSingle().ToString("C", culture),
                _randomService.GetSingle().ToString("C", culture) };
        }

        // GET api/values/5
        [HttpGet("{count}/{format?}")]
        public IEnumerable<string> Get(int count, string format = null)
        {
            var culture = GetCultureCurrency(format);
            for (int i = 0; i < count; i++)
            {
                yield return _randomService.GetSingle().ToString("C", culture);
            }
        }

        private static CultureInfo GetCultureCurrency(string format)
        {
            CultureInfo culture;
            try
            {
                culture = CultureInfo.GetCultureInfo(format);
            }
            catch
            {
                culture = new CultureInfo("jp");
            }

            return culture;
        }
    }
}
