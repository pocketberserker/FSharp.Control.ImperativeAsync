namespace FSharp.Control

open System

[<Sealed>]
[<NoEquality; NoComparison>]
type ImperativeAsync<'T>

module ImperativeAsync =

  type ImperativeAsyncBuilder =

    new: unit -> ImperativeAsyncBuilder

    member Bind : source:ImperativeAsync<'T> * body:('T -> ImperativeAsync<'U>) -> ImperativeAsync<'U>

    member Combine : source1:ImperativeAsync<'T> * source2:ImperativeAsync<'T> -> ImperativeAsync<'T>

    member Delay : f:(unit -> ImperativeAsync<'T>) -> ImperativeAsync<'T>

    member For : source:#seq<'T> * action:('T -> ImperativeAsync<'TResult>) -> ImperativeAsync<'TResult>

    member Return : 'T -> ImperativeAsync<'T> 

    member ReturnFrom : ImperativeAsync<'T> -> ImperativeAsync<'T> 

    member TryFinally : body:ImperativeAsync<'T> * compensation:(unit -> unit) -> ImperativeAsync<'T>

    member TryWith : body:ImperativeAsync<'T> * handler:(exn -> ImperativeAsync<'T>) -> ImperativeAsync<'T>

    member Using : resource:'T * binder:('T -> ImperativeAsync<'U>) -> ImperativeAsync<'U> when 'T :> IDisposable

    member While : guard:(unit -> bool) * body:ImperativeAsync<'T> -> ImperativeAsync<'T>

    member Zero : unit -> ImperativeAsync<'T>

    member Source<'T, 'U when 'T :> seq<'U>> : source:'T -> 'T

    member Source: source:Async<'T> -> ImperativeAsync<'T>

    member Source: source:ImperativeAsync<'T> -> ImperativeAsync<'T>

    member Run : source: ImperativeAsync<'T> -> Async<'T option>

  [<AutoOpen>]
  module Syntax =

    val imperativeAsync: ImperativeAsyncBuilder

  module Unsafe =

    type UnsafeImperativeAsyncBuilder =
    
      new: unit -> UnsafeImperativeAsyncBuilder

      member Bind : source:ImperativeAsync<'T> * body:('T -> ImperativeAsync<'U>) -> ImperativeAsync<'U>

      member Combine : soure1:ImperativeAsync<'T> * source2:ImperativeAsync<'T> -> ImperativeAsync<'T>

      member Delay : f:(unit -> ImperativeAsync<'T>) -> ImperativeAsync<'T>

      member For : source:#seq<'T> * action:('T -> ImperativeAsync<'TResult>) -> ImperativeAsync<'TResult>

      member Return : 'T -> ImperativeAsync<'T> 

      member ReturnFrom : ImperativeAsync<'T> -> ImperativeAsync<'T> 

      member TryFinally : body:ImperativeAsync<'T> * compensation:(unit -> unit) -> ImperativeAsync<'T>

      member TryWith : body:ImperativeAsync<'T> * handler:(exn -> ImperativeAsync<'T>) -> ImperativeAsync<'T>

      member Using : resource:'T * binder:('T -> ImperativeAsync<'U>) -> ImperativeAsync<'U> when 'T :> IDisposable

      member While : guard:(unit -> bool) * body:ImperativeAsync<'T> -> ImperativeAsync<'T>

      member Zero : unit -> ImperativeAsync<'T>

      member Source<'T, 'U when 'T :> seq<'U>> : source:'T -> 'T

      member Source: source:Async<'T> -> ImperativeAsync<'T>

      member Source: source:ImperativeAsync<'T> -> ImperativeAsync<'T>

      member Run : source: ImperativeAsync<'T> -> Async<'T>

    val imperativeAsync: UnsafeImperativeAsyncBuilder
