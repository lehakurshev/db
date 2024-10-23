using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MongoDB.Driver;

namespace Game.Domain
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserEntity> userCollection;
        public const string CollectionName = "users";

        public static readonly Object Lock = new();

        public MongoUserRepository(IMongoDatabase database)
        {
            userCollection = database.GetCollection<UserEntity>(CollectionName);

            // Создание уникального индекса для логинов
            userCollection.Indexes.CreateOne(new CreateIndexModel<UserEntity>(
                Builders<UserEntity>.IndexKeys.Ascending(u => u.Login),
                new CreateIndexOptions { Unique = true }
            ));
        }

        public UserEntity Insert(UserEntity user)
        {
            // Используем метод InsertOne, чтобы избежать необходимости вызывать Find
            userCollection.InsertOne(user);
            return user;
        }

        public UserEntity FindById(Guid id)
        {
            return userCollection.Find(user => user.Id == id).FirstOrDefault();
        }

        public UserEntity GetOrCreateByLogin(string login)
        {
            lock(Lock)
            {
                return userCollection.Find(user => user.Login == login).FirstOrDefault()
                       ?? Insert(new UserEntity(Guid.NewGuid()) { Login = login });
            }

        }

        public void Update(UserEntity user)
        {
            userCollection.ReplaceOne(u => u.Id == user.Id, user);
        }

        public void Delete(Guid id)
        {
            userCollection.DeleteOne(u => u.Id == id);
        }

        public PageList<UserEntity> GetPage(int pageNumber, int pageSize)
        {
            var users = userCollection
                .Find(u => true)
                .SortBy(u => u.Login)
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToList();

            var totalUsers = userCollection.CountDocuments(u => true);

            return new PageList<UserEntity>(users, totalUsers, pageNumber, pageSize);
        }

        public void UpdateOrInsert(UserEntity user, out bool isInserted)
        {
            var existingUser = FindById(user.Id);
            if (existingUser == null)
            {
                Insert(user);
                isInserted = true;
            }
            else
            {
                Update(user);
                isInserted = false;
            }
        }
    }
}
