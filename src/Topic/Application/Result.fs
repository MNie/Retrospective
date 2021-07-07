namespace Topic.Application.Result

open Model
open MongoDB.Bson

type internal QueryResult =
    | Create of Topic
    | Update of Topic
    | View of Topic list
    | Single of Result<Topic, string>
    | Delete of Result<bool, string>

type TopicRequest =
    | Create of Topic.Contract.Topic
    | Update of Topic.Contract.Topic
    | View of string
    | Single of id: ObjectId
    | Delete of id: ObjectId

type TopicOperation =
    | Create
    | Update
    | View
    | Single of id: ObjectId
    | Delete of id: ObjectId

type DTOResult =
    | Create of Topic.Contract.Topic
    | Update of Topic.Contract.Topic
    | View of Topic.Contract.Topic list
    | Single of Result<Topic.Contract.Topic, string>
    | Delete of Result<bool, string>