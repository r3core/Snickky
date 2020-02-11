using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Snickky.Domain.Models;
using Snickky.Services;

namespace Snickky.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IMachineService _machineService;
        public MachineInformation MachineInformation { get; set; }
        public SelectList Coins { get; set; }
        
        [BindProperty]
        public Coin Coin { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IMachineService machineService)
        {
            _logger = logger;
            _machineService = machineService;
        }

        public void OnGet()
        {
            LoadCoins();
            MachineInformation = _machineService.GetInfo();
        }

        public async Task OnPostInsertAsync()
        {
            LoadCoins();
            MachineInformation = await _machineService.Insert(Coin.Value);
        }

        public async Task OnPostCancelAsync()
        {
            LoadCoins();
            MachineInformation = await _machineService.Cancel();
        }

        public void OnPostReset()
        {
            LoadCoins();
            MachineInformation = _machineService.Reset();
        }

        public async Task OnPostDispenseAsync()
        {
            LoadCoins();
            MachineInformation = await _machineService.Dispense();
        }

        private void LoadCoins()
        {
            Coins = new SelectList(new List<Coin>
            {
                new Coin { Name = "5c", Value = 5},
                new Coin { Name = "10c", Value = 10},
                new Coin { Name = "20c", Value = 20},
                new Coin { Name = "50c", Value = 50},
                new Coin { Name = "$1", Value = 100},
                new Coin { Name = "$2", Value = 200},
            }, "Value", "Name");
        }
    }
}
