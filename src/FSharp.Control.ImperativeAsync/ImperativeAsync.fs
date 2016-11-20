namespace FSharp.Control

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
    member __.Run(C x) = x

  [<AutoOpen>]
  module Syntax =

    let imperativeAsync = ImperativeAsyncBuilder()
  
  module Unsafe =

    type UnsafeImperativeAsyncBuilder() =
      member __.ReturnFrom(x) = imperativeAsync.ReturnFrom(x)
      member __.Return(x) = imperativeAsync.Return(x)
      member __.Source(xs: #seq<_>) = imperativeAsync.Source(xs)
      member __.Source(x: ImperativeAsync<_>) = imperativeAsync.Source(x)
      member __.Source(x: Async<_>) = imperativeAsync.Source(x)
      member __.Zero() = imperativeAsync.Zero()
      member __.Bind(x, f) = imperativeAsync.Bind(x, f)
      member __.Using(x: #IDisposable, f) = imperativeAsync.Using(x, f)
      member __.Combine(x, y) = imperativeAsync.Combine(x, y)
      member __.While(guard, y) = imperativeAsync.While(guard, y)
      member __.For(xs: #seq<_>, f) = imperativeAsync.For(xs, f)
      member __.TryFinally(x, cf) = imperativeAsync.TryFinally(x, cf)
      member __.TryWith(p, cf) = imperativeAsync.TryWith(p, cf)
      member __.Delay(f) = imperativeAsync.Delay(f)
      member __.Run(C x) = async {
        let! x = x
        return
          match x with
          | Some x -> x
          | None -> Unchecked.defaultof<'T>
      }

    let imperativeAsync = UnsafeImperativeAsyncBuilder()
