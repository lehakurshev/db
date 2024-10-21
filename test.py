from pymongo import MongoClient

client = MongoClient('mongodb://localhost:27017/')

db = client['game-tests']

collection1 = db['users']

documents = collection1.find()

for doc in documents:
    print(doc)

collection2 = db['games']

documents = collection2.find()

for doc in documents:
    print(doc)

client.close()

