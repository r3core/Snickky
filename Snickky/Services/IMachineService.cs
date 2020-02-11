using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Snickky.Services
{
    public interface IMachineService
    {
        Task<bool> Insert();
        Task<bool> Dispense();
        Task Cancel();
    }
}
