namespace CoopQueue.Api.Entities
{
    /// <summary>
    /// Represents a specific vote record.
    /// Acts as a link between a User and a Game to ensure a one-vote-per-user logic.
    /// </summary>
    public class GameVote
    {
        public int Id { get; set; }

        /// <summary>
        /// Foreign Key pointing to the Game being voted on.
        /// </summary>
        public int GameId { get; set; }

        /// <summary>
        /// Foreign Key pointing to the User who cast the vote.
        /// </summary>
        public int UserId { get; set; }
    }
}