using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stateless;

namespace Snickky.Domain
{
    public enum MachineTrigger
    {
        Insert,
        Reject,
        Cancel,
        Dispense,
        Idle
    }

    public enum MachineState
    {
        Idle,
        Insertion,
        Suspension,
        Termination
    }

    public class Machine
    {
        private const int ItemPrice = 160;
        public StateMachine<MachineState, MachineTrigger> StateMachine { get; private set; }

        private readonly StateMachine<MachineState, MachineTrigger>.TriggerWithParameters<int> _insertionTrigger =
            new StateMachine<MachineState, MachineTrigger>.TriggerWithParameters<int>(MachineTrigger.Insert);
        private readonly StateMachine<MachineState, MachineTrigger>.TriggerWithParameters<int> _rejectionTrigger =
            new StateMachine<MachineState, MachineTrigger>.TriggerWithParameters<int>(MachineTrigger.Reject);

        private readonly Dictionary<int, Bank> _banks;
        public int LoadedAmount { get; private set; }
        public int ChangeAmount { get; private set; }
        public int Snickers { get; set; }
        public List<string> MachineLogs { get; set; }
        public List<int> ChangeCoins { get; set; }

        public Machine()
        {
            _banks = new Dictionary<int, Bank>();
            ConfigureDefaults();
        }

        public async Task InsertCoin(int coin)
        {
            await StateMachine.FireAsync(_insertionTrigger, coin);
        }

        public async Task Dispense()
        {
            await StateMachine.FireAsync(MachineTrigger.Dispense);
        }

        public async Task Cancel()
        {
            await StateMachine.FireAsync(MachineTrigger.Cancel);
        }

        public bool IsIdle()
        {
            return StateMachine.IsInState(MachineState.Idle);
        }

        private void ConfigureDefaults()
        {
            MachineLogs = new List<string>();
            MachineLogs.Add("The machine is idle and running.");
            ConfigureSnickers();
            ConfigureBanks();
            ConfigureStateMachine();
        }

        private void ConfigureSnickers()
        {
            Snickers = 2;
        }

        private void ConfigureBanks()
        {
            _banks.Add(10, new Bank(8, 10));
            _banks.Add(20, new Bank(25, 20));
            _banks.Add(50, new Bank(5, 50));
            _banks.Add(100, new Bank(11, 100));
            _banks.Add(200, new Bank(15, 200));
        }

        private void ConfigureStateMachine()
        {
            StateMachine = new StateMachine<MachineState, MachineTrigger>(MachineState.Idle);
            StateMachine.Configure(MachineState.Idle)
                .PermitReentry(MachineTrigger.Dispense)
                .PermitReentry(MachineTrigger.Cancel)
                .Permit(MachineTrigger.Insert, MachineState.Insertion)
                .Permit(MachineTrigger.Reject, MachineState.Suspension);

            StateMachine.Configure(MachineState.Insertion)
                .PermitReentry(MachineTrigger.Insert)
                .Permit(MachineTrigger.Reject, MachineState.Suspension)
                .Permit(MachineTrigger.Cancel, MachineState.Termination)
                .Permit(MachineTrigger.Dispense, MachineState.Termination)
                .OnEntryFromAsync(_insertionTrigger, OnInsert);

            StateMachine.Configure(MachineState.Suspension)
                .Permit(MachineTrigger.Cancel, MachineState.Termination)
                .Permit(MachineTrigger.Insert, MachineState.Insertion)
                .OnEntryFrom(_rejectionTrigger, OnReject);

            StateMachine.Configure(MachineState.Termination)
                .PermitReentry(MachineTrigger.Cancel)
                .Permit(MachineTrigger.Idle, MachineState.Idle)
                .OnEntryFromAsync(MachineTrigger.Cancel, OnCancel)
                .OnEntryFromAsync(MachineTrigger.Dispense, OnDispense);
        }

        private async Task OnDispense()
        {
            var change = LoadedAmount - ItemPrice;
            if (Snickers == 0)
            {
                MachineLogs.Add("Snickers are out of stock.");
                await StateMachine.FireAsync(MachineTrigger.Cancel);
                return;
            }

            if (change == 0)
            {
                Snickers--;
                LoadedAmount = 0;
                MachineLogs.Add("Snicker issued.");
                await StateMachine.FireAsync(MachineTrigger.Idle);
            }
            else if (change < 0)
            {
                MachineLogs.Add("Insufficient funds.");
                await StateMachine.FireAsync(MachineTrigger.Cancel);
            }
            else
            {
                
                ChangeAmount = change;
                LoadedAmount = 0;
                DeductChange();

                if (ChangeCoins != null)
                {
                    Snickers--;
                    MachineLogs.Add("Snicker issued.");
                    await StateMachine.FireAsync(MachineTrigger.Idle);
                }
                else
                {
                    MachineLogs.Add("Unable to return change.");
                    await StateMachine.FireAsync(MachineTrigger.Cancel);
                }
                
            }
        }

        private async Task OnCancel()
        {
            MachineLogs.Add("Cancelling Transaction.");
            ChangeAmount = LoadedAmount;
            DeductChange();
            LoadedAmount = 0;
            await StateMachine.FireAsync(MachineTrigger.Idle);
        }

        private void OnReject(int rejectedCoinValue)
        {
            MachineLogs.Add("Rejecting coin.");
            ChangeCoins = new List<int>{rejectedCoinValue};
        }

        private async Task OnInsert(int coinValue)
        {
            var isValidCoinValue = _banks.TryGetValue(coinValue, out var bank);

            if (isValidCoinValue && bank.CanInsertCoins())
            {
                MachineLogs.Add("Inserted coin.");
                bank.Count++;
                LoadedAmount += coinValue;
            }
            else
            {
                if (!isValidCoinValue)
                {
                    MachineLogs.Add("Warning. Unsupported Coin inserted.");
                }

                if (bank != null && !bank.CanInsertCoins())
                {
                    MachineLogs.Add("Coin bank full.");
                }
                await StateMachine.FireAsync(_rejectionTrigger, coinValue);
            }
        }

        private void DeductChange()
        {
            var validCombinationCount = 0;
            var banks = _banks.Values.ToList();
            banks.Reverse();
            var changeCombination = FindChangeCombination(ChangeAmount, 0, banks, new List<int>(), ref validCombinationCount);
            if (changeCombination?.Any() == true)
            {
                foreach (var coinValue in changeCombination)
                {
                    _banks[coinValue].Count--;
                }
            }

            ChangeCoins = changeCombination;
        }

        /// <summary>
        /// This algorithm behaves slightly different than the standard change permutation algorithm as it terminates after finding the first successful permutation.
        /// Providing a reversed list of banks/denominations will yield better results.
        /// </summary>
        private static List<int> FindChangeCombination(int remainder, int index, List<Bank> banks, List<int> combination, ref int validCombinationCount)
        {
            if (remainder == 0)
            {
                validCombinationCount++;
                return combination;
            }

            if (remainder < 0)
            {
                return null;
            }

            for(var i = index; i < banks.Count; i++)
            {
                var currentCoinBank = banks[i];
                if (currentCoinBank.Count > 0)
                {
                    var coinValue = banks[i].CoinValue;
                    combination.Add(coinValue);
                    FindChangeCombination(remainder - coinValue, i, banks, combination, ref validCombinationCount);
                    if (validCombinationCount > 0)
                    {
                        return combination;
                    }
                    combination.RemoveAt(combination.Count - 1);
                }
            }

            return null;
        }
    }
}
