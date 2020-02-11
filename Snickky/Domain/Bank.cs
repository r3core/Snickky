namespace Snickky.Domain
{
    public class Bank
    {
        private const int BankSize = 25;
        public int CoinValue { get; }
        public int Count { get; set; }

        public Bank(int count, int coinValue)
        {
            Count = count;
            CoinValue = coinValue;
        }

        public bool CanInsertCoins() => Count < BankSize;
    }
}
