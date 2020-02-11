using System.Threading.Tasks;
using Stateless;

namespace Snickky.Domain
{
    public enum BankTrigger
    {
        InsertCoin,
        LockBank
    }

    public enum BankState
    {
        Open,
        Closed
    }

    public class Bank
    {
        private const int BankSize = 25;
        private StateMachine<BankState, BankTrigger> _stateMachine;
        public int CoinValue { get; }
        public int Count { get; private set; }

        public Bank(int count)
        {
            Count = count;
            ConfigureStateMachine();
        }

        public bool InsertCoin()
        {
            _stateMachine.FireAsync(BankTrigger.InsertCoin);
            return _stateMachine.IsInState(BankState.Open);
        }

        private void ConfigureStateMachine()
        {
            _stateMachine = new StateMachine<BankState, BankTrigger>(BankState.Open);
            _stateMachine.Configure(BankState.Open)
                .OnEntryFromAsync(BankTrigger.InsertCoin, OnInsertCoin)
                .PermitReentry(BankTrigger.InsertCoin)
                .Permit(BankTrigger.LockBank, BankState.Closed);
        }

        private async Task OnInsertCoin()
        {
            if (CanInsertCoins())
            {
                Count++;
            }
            else
            {
                await _stateMachine.FireAsync(BankTrigger.LockBank);
            }
        }

        private bool CanInsertCoins() => Count < BankSize;

    }
}
