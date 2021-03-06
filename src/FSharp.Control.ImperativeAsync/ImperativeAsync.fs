﻿namespace FSharp.Control

open System

[<NoEquality; NoComparison>]
type ImperativeAsync<'T> = C of Async<'T option>

module ImperativeAsync =

  let unwrap (C body) = body

  type ImperativeAsyncBuilder() =
    member __.ReturnFrom(x: ImperativeAsync<_>) = x
    member __.Return(x) = C <| async { return Some x }
    member __.Source(xs: #seq<_>) = xs
    member __.Source(x: ImperativeAsync<_>) = x
    member __.Source(x: Async<_>) = C <| async {
      let! x = x
      return Some x
    }
    member __.Zero() = C <| async { return None }
    member this.Bind(C x, f) = C <| async {
      let! x = x
      return!
        match x with
        | Some x -> f x
        | None -> this.Zero()
        |> unwrap
    }
    member __.Using(x: #IDisposable, f) = C <| async.Using(x, f >> unwrap)
    member __.Combine(C x, C y) = C <| async {
      let! x = x
      match x with
      | Some r -> return Some r
      | _ -> return! y
    }
    member this.While(guard, y) =
      if guard () then
        this.Combine(y, this.Delay(fun () -> this.While(guard, y)))
      else this.Zero()
    member this.For(xs: #seq<_>, f) =
      this.Using(
        xs.GetEnumerator(),
        fun itor -> this.While(itor.MoveNext, this.Delay(fun () -> f itor.Current))
      )
    member __.TryFinally(C x, cf) = C <| async.TryFinally(x, cf)
    member __.TryWith(C p, cf) = C <| async.TryWith(p, cf >> unwrap)
    member __.Delay(f) = C <| async.Delay(f >> unwrap)
    member __.Run(C x) = async {
      let! x = x
      return
        match x with
        | Some x -> x
        | None -> failwith "Imperative async computation expression must return value."
    }

  [<AutoOpen>]
  module Syntax =

    let imperativeAsync = ImperativeAsyncBuilder()

module Imperative =

    let async = ImperativeAsync.Syntax.imperativeAsync
