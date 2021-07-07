namespace Notification.Contract

open System
open System.Collections.Generic

type Identifier = Identifier of Guid
type RelatedId = Id of string
type AdditionalData = AdditionalData of string

type Message = {
    Id: Identifier
    RelatedId: RelatedId
    AdditionalData: AdditionalData
}
