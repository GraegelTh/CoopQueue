namespace CoopQueue.Shared.Enums
{
    public enum GameStatus
    {
        Suggestion,     // Proposed by a user, open for voting
        Playing,        // Currently active / being played
        Completed,      // Finished / Beaten
        WaitForSale     // Interested, but waiting for a price drop
    }
}