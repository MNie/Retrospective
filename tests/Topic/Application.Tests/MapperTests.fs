module MapperTests

open Expecto
open Mapper
open MongoDB.Bson
open Model
open System
open Topic.Application.Result

let internal singleToDTOCases =
    let id = ObjectId.GenerateNewId()
    let input = {
        _id = id
        createdDate = DateTimeOffset.UtcNow
        updatedDate = DateTimeOffset.UtcNow
        description = "todo"
        createdBy = "creator"
        name = "name"
        isDone = false
    }
    
    let result: Topic.Contract.Topic = {
        Id = Topic.Contract.Id (id.ToString())
        Description = Topic.Contract.Description "todo"
        Creator = Topic.Contract.Creator "creator"
        Name = Topic.Contract.Name "name"
        Done = Topic.Contract.Done false
    }
    
    [
        "create", QueryResult.Create input, DTOResult.Create result
        "single", QueryResult.Single (Ok input), DTOResult.Single (Ok result)
        "delete", QueryResult.Delete (Ok true), DTOResult.Delete (Ok true)
        "update", QueryResult.Update input, DTOResult.Update result
    ]

let tests = testList "Mapper" [
    testList "toDomain" [
        testCase "none" <| fun _ ->
            let result = Mapper.ToDomain.map None |> Async.RunSynchronously
            Expect.equal result None ""
            
        testCase "valid" <| fun _ ->
            let id = ObjectId.GenerateNewId()
            let input: Topic.Contract.Topic = {
                Id = Topic.Contract.Id (id.ToString())
                Description = Topic.Contract.Description "desc"
                Creator = Topic.Contract.Creator "creator"
                Name = Topic.Contract.Name "name"
                Done = Topic.Contract.Done true
            }
            
            let result =
                Mapper.ToDomain.map (Some input)
                |> Async.RunSynchronously
                |> Option.defaultWith (fun () -> raise (Exception "Map returns none"))
            
            Expect.equal result._id id ""
            Expect.equal result.description "desc" ""
            Expect.equal result.createdBy "creator" ""
            Expect.equal result.name "name" ""
            Expect.equal result.isDone true ""
    ]
    
    testList "toDTO" [
        testList "single" [
            for (name, inTest, outTest) in singleToDTOCases do
                testCase name <| fun _ ->
                    let result = ToDTO.map inTest
                    Expect.equal result outTest ""
        ]
    ]
]