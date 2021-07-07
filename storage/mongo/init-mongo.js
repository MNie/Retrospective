db = db.getSiblingDB('admin')
db.createUser(
    {
        user: "admin",
        pwd: "123",
        roles: [
            { role: "readWrite", db: "admin" }
        ]
    });

db = db.getSiblingDB('retrospective')
db.createUser(
    {
        user: "admin",
        pwd: "123",
        roles: [
            { role: "readWrite", db: "retrospective" }
        ]
    });