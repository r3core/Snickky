using System.Collections.Generic;
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
        Reload
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
        private StateMachine<MachineState, MachineTrigger> _stateMachine;
        private readonly StateMachine<MachineState, MachineTrigger>.TriggerWithParameters<int> _insertionTrigger =
            new StateMachine<MachineState, MachineTrigger>.TriggerWithParameters<int>(MachineTrigger.Insert);
        private readonly StateMachine<MachineState, MachineTrigger>.TriggerWithParameters<int> _rejectionTrigger =
            new StateMachine<MachineState, MachineTrigger>.TriggerWithParameters<int>(MachineTrigger.Reject);
        private readonly StateMachine<MachineState, MachineTrigger>.TriggerWithParameters<int> _cancellationTrigger =
            new StateMachine<MachineState, MachineTrigger>.TriggerWithParameters<int>(MachineTrigger.Cancel);
        private readonly StateMachine<MachineState, MachineTrigger>.TriggerWithParameters<int> _dispensationTrigger =
            new StateMachine<MachineState, MachineTrigger>.TriggerWithParameters<int>(MachineTrigger.Dispense);

        private readonly Dictionary<int, Bank> _banks;
        private int LoadedAmount;
        public int RejectedAmount { get; private set; }
        public int Change { get; private set; }
        private int Snickers;

        public Machine()
        {
            _banks = new Dictionary<int, Bank>();
            ConfigureDefaults();
        }

        public async Task<bool> InsertCoin()
        {
            await _stateMachine.FireAsync(MachineTrigger.Insert);
            return _stateMachine.IsInState(MachineState.Insertion);
        }

        public async Task<int> Dispense()
        {
            await _stateMachine.FireAsync(MachineTrigger.Dispense);
            return Change;
        }

        public async Task<int> Cancel()
        {
            await _stateMachine.FireAsync(MachineTrigger.Cancel);
            return Change;
        }

        private void ConfigureDefaults()
        {
            ConfigureBanks();
            ConfigureStateMachine();
        }

        private void ConfigureBanks()
        {
            _banks.Add(10, new Bank(8));
            _banks.Add(20, new Bank(25));
            _banks.Add(50, new Bank(5));
            _banks.Add(100, new Bank(11));
            _banks.Add(200, new Bank(15));
        }

        private void ConfigureStateMachine()
        {
            _stateMachine = new StateMachine<MachineState, MachineTrigger>(MachineState.Idle);
            _stateMachine.Configure(MachineState.Idle)
                .PermitReentry(MachineTrigger.Dispense)
                .PermitReentry(MachineTrigger.Cancel)
                .Permit(MachineTrigger.Insert, MachineState.Insertion)
                .Permit(MachineTrigger.Reject, MachineState.Suspension);

            _stateMachine.Configure(MachineState.Insertion)
                .PermitReentry(MachineTrigger.Insert)
                .Permit(MachineTrigger.Reject, MachineState.Suspension)
                .Permit(MachineTrigger.Cancel, MachineState.Idle)
                .Permit(MachineTrigger.Dispense, MachineState.Termination)
                .OnEntryFrom(_insertionTrigger, OnInsert);

            _stateMachine.Configure(MachineState.Suspension)
                .PermitReentry(MachineTrigger.Insert)
                .Permit(MachineTrigger.Cancel, MachineState.Idle)
                .Permit(MachineTrigger.Insert, MachineState.Insertion)
                .OnEntryFrom(_rejectionTrigger, OnReject);

            _stateMachine.Configure(MachineState.Termination)
                .Permit(MachineTrigger.Dispense, MachineState.Idle)
                .Permit(MachineTrigger.Cancel, MachineState.Idle)
                .OnEntryFrom(_cancellationTrigger, OnCancel)
                .OnEntryFrom(_dispensationTrigger, OnDispense);
        }

        private void OnDispense(int obj)
        {
            var change = LoadedAmount - ItemPrice;
            if (change == 0)
            {
                Snickers--;
            }
            else if (change < 0)
            {
                _stateMachine.FireAsync(MachineTrigger.Cancel);
            }
            else
            {
                Snickers--;
                Change = change;
            }
        }

        private void OnCancel(int obj)
        {
            Change = LoadedAmount + Change;
            LoadedAmount = 0;
        }

        private void OnReject(int rejectedCoinValue)
        {
            Change = rejectedCoinValue;
        }

        private void OnInsert(int coinValue)
        {
            var success = _banks[coinValue].InsertCoin();
            if (!success)
            {
                _stateMachine.FireAsync(_rejectionTrigger, coinValue);
            }
            else
            {
                LoadedAmount += coinValue;
            }
        }
    }
}
