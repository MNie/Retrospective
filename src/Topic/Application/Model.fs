module internal Model
    open System
    open MongoDB.Bson
    open MongoDB.Bson.Serialization.Attributes

    type Name = string
    type PointOfTime = DateTimeOffset
    type Identifier = ObjectId
    type Description = string
    type Done = bool
    
    [<BsonIgnoreExtraElements>]
    type Topic =
        {
            _id: Identifier
            name: Name
            createdDate: PointOfTime
            updatedDate: PointOfTime
            createdBy: Name
            description: Description
            isDone: Done
        }

