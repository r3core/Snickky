namespace Snickky.Domain.Models
{
    public class MachineInformation
    {
        public string LoadedValue { get; set; }
        public string ChangeTray { get; set; }
        public string SnickersInMachine { get; set; }
        public string CurrentStatus { get; set; }
        public bool IsIdle { get; set; }
    }
}
