namespace MechanicsAssistantServerTests{

    class TestingJoinRequest {
        public int UserId;
        public int CompanyId;
    
        public static TestingJoinRequest ValidRequest1 = new TestingJoinRequest() {
            UserId = 1,
            CompanyId = 2
        };

        public static TestingJoinRequest ValidRequest2 = new TestingJoinRequest() {
            UserId = 2,
            CompanyId = 3
        };
    }


}