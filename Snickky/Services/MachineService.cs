using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Snickky.Domain;
using Snickky.Domain.Models;

namespace Snickky.Services
{
    public class MachineService : IMachineService
    {
        private Machine _machine;

        public MachineService()
        {
            _machine = new Machine();
        }

        public MachineInformation GetInfo()
        {
            return GenerateMachineInformation();
        }

        public async Task<MachineInformation> Insert(int coin)
        {
            await _machine.InsertCoin(coin);
            return GenerateMachineInformation();
        }

        public async Task<MachineInformation> Dispense()
        {
            await _machine.Dispense();
            return GenerateMachineInformation();
        }

        public async Task<MachineInformation> Cancel()
        {
            await _machine.Cancel();
            return GenerateMachineInformation();
        }

        public MachineInformation Reset()
        {
            _machine = new Machine();
            return GenerateMachineInformation();
        }

        public int GetStock()
        {
            return _machine.Snickers;
        }

        public void UpdateStock(int updatedStockValue)
        {
            _machine.Snickers = updatedStockValue;
        }

        private MachineInformation GenerateMachineInformation()
        {
            var information = new MachineInformation
            {
                LoadedValue = _machine.LoadedAmount > 0 ? $"{(decimal)_machine.LoadedAmount / 100:C}" : "$0",
                ChangeTray = _machine.ChangeCoins?.Any() == true ? string.Join(", ", _machine.ChangeCoins.Select(CentsToCoins)) : "Tray is empty.",
                SnickersInMachine = _machine.Snickers.ToString(),
                CurrentStatus = string.Join(" ", _machine.MachineLogs),
                IsIdle = _machine.IsIdle()
            };
            _machine.MachineLogs = new List<string>();
            _machine.ChangeCoins = new List<int>();

            return information;
        }

        private string CentsToCoins(int value)
        {
            switch (value)
            {
                case 5:
                    return "5c";
                case 10:
                    return "10c";
                case 20:
                    return "20c";
                case 50:
                    return "50c";
                case 100:
                    return "$1";
                case 200:
                    return "$2";
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
