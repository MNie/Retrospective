namespace Topic.Contract

type Name = Name of string
type Identifier = Id of string
type Creator = Creator of string
type Description = Description of string
type Done = Done of bool

type Topic = {
    Name: Name
    Id: Identifier
    Creator: Creator
    Description: Description
    Done: Done
}
