namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string startKeyHex = "0000000000000000000000000000000000000000000000040000000000000000";
            string stopKeyHex = "000000000000000000000000000000000000000000000007ffffffffffffffff";

            PartitionCalculator calculator = new PartitionCalculator(startKeyHex, stopKeyHex);
            List<Section> sections = calculator.GenerateSections();

            // Exibir um exemplo do primeiro section, area, e bloco
            Console.WriteLine("Section 0: \nStart = " + sections[0].StartKey + ", \nEnd = " + sections[0].EndKey);
            Console.WriteLine("  Area 0: \nStart = " + sections[0].Areas[0].StartKey + ", \nEnd = " + sections[0].Areas[0].EndKey);
            Console.WriteLine("    Block 0: \nStart = " + sections[0].Areas[0].Blocks[0].StartKey + ", \nEnd = " + sections[0].Areas[0].Blocks[0].EndKey);
        }
    }
}
