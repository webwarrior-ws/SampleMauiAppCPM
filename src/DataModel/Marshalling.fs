namespace DataModel

open System
open System.Reflection
open System.Text.Json

module VersionHelper =
    let CURRENT_VERSION =
        Assembly
            .GetExecutingAssembly()
            .GetName()
            .Version.ToString()

type IMarshallingWrapper =
    abstract member Value: obj

type MarshallingWrapper<'T> =
    {
        Version: string
        TypeName: string
        Value: 'T
    }

    static member New(value: 'T) =
        {
            Value = value
            Version = VersionHelper.CURRENT_VERSION
            TypeName = typeof<'T>.FullName
        }

    interface IMarshallingWrapper with
        member this.Value = this.Value :> obj

module Marshaller =

    let ExtractMetadata(json: string) : Type * Version =
        let wrapper = JsonSerializer.Deserialize<MarshallingWrapper<obj>> json
        let typ = Type.GetType wrapper.TypeName
        let version = Version wrapper.Version
        typ, version

    let Serialize<'T>(object: 'T) : string =
        let wrapper = MarshallingWrapper.New object
        JsonSerializer.Serialize wrapper

    let Deserialize<'T>(json: string) : 'T =
        if isNull json then
            raise <| ArgumentNullException "json"

        let wrapper = JsonSerializer.Deserialize<MarshallingWrapper<'T>> json
        wrapper.Value

    let DeserializeAbstract (json: string) (targetType: Type) : obj =
        if isNull json then
            raise <| ArgumentNullException "json"

        let wrapperGenericType = typedefof<MarshallingWrapper<_>>

        let wrapperType =
            wrapperGenericType.MakeGenericType(Array.singleton targetType)

        let wrapperObj = JsonSerializer.Deserialize(json, wrapperType)

        if isNull wrapperObj then
            failwith "Deserialization failed: result is null"
        elif wrapperObj.GetType() <> wrapperType then
            failwithf
                "Deserialization failed, resulting type: %s"
                (wrapperObj.GetType().FullName)

        let wrapper = wrapperObj :?> IMarshallingWrapper
        wrapper.Value
