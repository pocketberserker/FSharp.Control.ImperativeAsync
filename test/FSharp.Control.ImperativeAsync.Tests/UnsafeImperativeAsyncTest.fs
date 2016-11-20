module FSharp.Control.Tests.UnsafeImperativeAsyncTest

open Persimmon
open UseTestNameByReflection
open FSharp.Control.ImperativeAsync.Unsafe

let zero = test {
  let res = imperativeAsync { () }
  return! trap { it(res |> Async.RunSynchronously) }
}

let ret = test {
  let res = imperativeAsync { return 0 }
  do!
    res |> Async.RunSynchronously
    |> assertEquals 0
}
