namespace TestSerialCommunication
{
    internal class MacroModel
    {
        public string Name { get; set; }
        public string Macro { get; set; }
        public char Type { get; set; }
        public string GetSerialString() => (char)0x01 + "XXXXX" + (char)0x02 + "" + Type + Macro + (char)0x03;
    }
}