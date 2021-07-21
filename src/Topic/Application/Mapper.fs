namespace Mapper

open Topic.Application.Result
open Topic.Contract

module internal ToDomain =
    open Model
    open MongoDB.Bson
    open System

    let private mapCreate (dto: Topic.Contract.Topic): Async<Topic> =
        async {
            let (Id id) = dto.Id
            let (Description todo) = dto.Description
            let (Creator creator) = dto.Creator
            let (Name name) = dto.Name
            let (Done isDone) = dto.Done
            return {
                _id = if String.IsNullOrWhiteSpace id then ObjectId.GenerateNewId () else ObjectId.Parse (id)
                createdDate = DateTimeOffset.UtcNow
                updatedDate = DateTimeOffset.UtcNow
                description = todo
                createdBy = creator
                name = name
                isDone = isDone
            }
        }

    let map data =
        async {
            match data with
            | Some d ->
                let! mapped = mapCreate d
                return Some mapped
            | None -> return None
        }

module internal ToDTO =
    open Model

    let private mapSingle (dbRepr: Topic): Topic.Contract.Topic =
        {
            Id = dbRepr._id.ToString() |> Id
            Name = Topic.Contract.Name dbRepr.name
            Creator = Creator dbRepr.createdBy
            Description = Topic.Contract.Description dbRepr.description
            Done = Topic.Contract.Done dbRepr.isDone
        }

    let map =
        function
        | QueryResult.Create single -> mapSingle single |> DTOResult.Create
        | QueryResult.View multiple -> multiple |> List.map mapSingle |> DTOResult.View
        | QueryResult.Single single -> single |> Result.bind (mapSingle >> Ok) |> DTOResult.Single
        | QueryResult.Delete single -> single |> DTOResult.Delete
        | QueryResult.Update single -> mapSingle single |> DTOResult.Update