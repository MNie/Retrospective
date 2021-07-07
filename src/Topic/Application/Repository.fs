module Topic.Application.Repository
    open BagnoDB
    open BagnoDB.Serializator
    open Topic.Application.Configuration
    open Model
    open MongoDB.Driver
    open Mapper
    open Topic.Application.Result

    [<Literal>]
    let private collectionName = "topic"

    let connection (config: BagnoDB.Config * CollectionConfig) =
        let cnf, collectionCnf = config
        let connection =
            Connection.host cnf
            |> Connection.database collectionCnf.db
            |> Connection.collection collectionName

        Conventions.create
        |> Conventions.add (RecordConvention ())
        |> Conventions.add (OptionConvention ())
        |> Conventions.build "F# Type Conventions"

        Serialization.bson (BagnoSerializationProvider ())

        connection

    let private queryDb token config ``type`` (model: Option<Topic>) =
        match ``type`` with
        | TopicOperation.Create ->
            async {
                let fOpt = InsertOneOptions ()

                let! _ =
                    connection config
                    |> Query.insertOne token fOpt model.Value
                return QueryResult.Create model.Value
            }
        | TopicOperation.Update ->
            async {
                let fOpt = FindOneAndReplaceOptions<Topic> ()
                fOpt.ReturnDocument <- ReturnDocument.After
                let filter =
                    Filter.eq (fun (x: Topic) -> x._id) model.Value._id

                let! result =
                    connection config
                    |> Query.updateWhole token fOpt model.Value filter
                return QueryResult.Update result
            }
        | TopicOperation.View ->
            async {
                let fOpt = FindOptions<Topic> ()

                let! result =
                    connection config
                    |> Query.getAll token fOpt
                return result |> Seq.toList |> QueryResult.View
            }
        | TopicOperation.Single id ->
            async {
                let fOpt = FindOptions<Topic> ()
                let filter =
                    Filter.eq (fun (x: Topic) -> x._id) id

                let! result =
                    connection config
                    |> Query.filter token fOpt filter

                return
                    match result |> Seq.toList with
                    | [ s ] -> Ok s |> QueryResult.Single
                    | _ -> Error (sprintf "problem when getting record with id: %A" id) |> QueryResult.Single
            }
        | TopicOperation.Delete id ->
            async {
                let fOpt = FindOneAndDeleteOptions<Topic> ()
                let filter =
                    Filter.eq (fun (x: Topic) -> x._id) id

                let! result =
                    connection config
                    |> Query.delete token fOpt filter
                return Ok (result._id = id) |> QueryResult.Delete
            }

    let private query token config data ``type`` =
        async {
            let! mapped = ToDomain.map data
            return! mapped |> (queryDb token config ``type``)
        }

    let create token config data =
        async {
            let! result = query token config (Some data) TopicOperation.Create
            return result |> ToDTO.map
        } |> Async.Catch

    let update token config data =
        async {
            let! result = query token config (Some data) TopicOperation.Update
            return result |> ToDTO.map
        } |> Async.Catch

    let view token config =
        async {
            let! result = query token config None TopicOperation.View
            return result |> ToDTO.map
        } |> Async.Catch

    let single token config id =
        async {
            let! result = query token config None (TopicOperation.Single id)
            return result |> ToDTO.map
        } |> Async.Catch

    let delete token config id =
        async {
            let! result = query token config None (TopicOperation.Delete id)
            return result |> ToDTO.map
        } |> Async.Catch