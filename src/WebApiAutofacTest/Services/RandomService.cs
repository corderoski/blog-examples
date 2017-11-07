using System;

namespace WebApiAutofacTest.Services
{

    public interface IRandomService
    {
        double GetSingle();
    }

    public class RandomService : IRandomService
    {
        public double GetSingle()
        {
            System.Threading.Thread.Sleep(50);
            return new Random().Next(MIN_VALUE, MAX_VALUE);
        }

        private const int MIN_VALUE = 1;
        private const int MAX_VALUE = 9999;
    }

}
