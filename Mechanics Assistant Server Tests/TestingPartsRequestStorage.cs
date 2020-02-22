namespace MechanicsAssistantServerTests {

    class TestingPartsRequest {

        public string ReferencedParts;
        public string JobId;
        public int UserId;


        public static TestingPartsRequest ValidRequest1 = new TestingPartsRequest() {
            ReferencedParts = "[1, 2]",
            JobId = "HMOH-TB00343",
            UserId = 1
        };

        public static TestingPartsRequest ValidRequest2 = new TestingPartsRequest() {
            ReferencedParts = "[2]",
            JobId = "HMOH-TB00344",
            UserId = 2
        };

        public static TestingPartsRequest ValidRequest3 = new TestingPartsRequest() {
            ReferencedParts = "[3]",
            JobId = "HMOH-HP001",
            UserId = 2
        };
    }
}