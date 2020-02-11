using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Snickky.Services;

namespace Snickky
{
    public class StocksModel : PageModel
    {
        private readonly IMachineService _machineService;

        [Range(1, 10)]
        [BindProperty]
        public int Stock { get; set; }
        public int CurrentStock { get; set; }

        public StocksModel(IMachineService machineService)
        {
            _machineService = machineService;
        }

        public void OnGet()
        {
            CurrentStock = _machineService.GetStock();
        }

        public IActionResult OnPostUpdate()
        {
            if (!ModelState.IsValid)
            {
                CurrentStock = _machineService.GetStock();
                return Page();
            }

            _machineService.UpdateStock(Stock);

            return RedirectToPage("./Stocks");
        }
    }
}