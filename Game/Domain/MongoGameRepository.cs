using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Game.Domain
{
    public class MongoGameRepository : IGameRepository
    {
        public const string CollectionName = "games";
        private readonly IMongoCollection<GameEntity> _gamesCollection;

        public MongoGameRepository(IMongoDatabase db)
        {
            _gamesCollection = db.GetCollection<GameEntity>(CollectionName);
        }

        public GameEntity Insert(GameEntity game)
        {
            _gamesCollection.InsertOne(game);
            return game; 
        }

        public GameEntity FindById(Guid gameId)
        {
            return _gamesCollection.Find(game => game.Id == gameId).FirstOrDefault();
        }

        public void Update(GameEntity game)
        {
            var result = _gamesCollection.ReplaceOne(g => g.Id == game.Id, game);
            if (!result.IsAcknowledged)
            {
                throw new Exception("Update failed.");
            }
        }

        public IList<GameEntity> FindWaitingToStart(int limit)
        {
            return _gamesCollection
                .Find(game => game.Status == GameStatus.WaitingToStart)
                .Limit(limit)
                .ToList();
        }

        public bool TryUpdateWaitingToStart(GameEntity game)
        {
            var result = _gamesCollection.ReplaceOne(
                g => g.Id == game.Id && g.Status == GameStatus.WaitingToStart,
                game);

            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
    }
}
