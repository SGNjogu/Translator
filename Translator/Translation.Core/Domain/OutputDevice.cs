namespace Translation.Core.Domain
{
    public class OutputDevice
    {
        public string DeviceId { get; set; }
        public string Address { get; set; }
        public string ProductName { get; set; }

        public bool IsBluetooth { get; set; } = false;
        public string Type { get; set; }
    }
}
