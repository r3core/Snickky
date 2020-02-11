using System.Threading.Tasks;
using Snickky.Domain.Models;

namespace Snickky.Services
{
    public interface IMachineService
    {
        MachineInformation GetInfo();
        Task<MachineInformation> Insert(int coin);
        Task<MachineInformation> Dispense();
        Task<MachineInformation> Cancel();
        MachineInformation Reset();

        int GetStock();
        void UpdateStock(int updatedStockValue);
    }
}
