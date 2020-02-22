using OldManInTheShopServer.Util;

namespace MechanicsAssistantServerTests {

    class TestingPartEntry {
        public string Make;
        public string Model;
        public int Year;
        public string PartId;
        public string PartName;

        public static TestingPartEntry ValidPartEntry1 = new TestingPartEntry() {
            Make = "Autocar",
            Model = "Xpeditor",
            Year = 1993,
            PartId = "AUXP93-TBT01",
            PartName = "Timing Belt Tensioner #1"
        };

        public static TestingPartEntry ValidPartEntry2 = new TestingPartEntry() {
            Make = "Autocar",
            Model = "Xpeditor",
            Year = 1993,
            PartId = "AUXP93-TBT02",
            PartName = "Timing Belt Tensioner #2"
        };

        public static TestingPartEntry ValidPartEntry3 = new TestingPartEntry() {
            Make = "Genie",
            Model = "Manlift",
            Year = 1982,
            PartId = "GELI-OR-HP",
            PartName = "Hydraulic Pump O-Ring"
        };
    }




}