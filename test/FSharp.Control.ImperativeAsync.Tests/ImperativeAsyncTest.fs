module FSharp.Control.Tests.ImperativeAsyncTest

open Persimmon
open UseTestNameByReflection
open FSharp.Control
open FSharp.Control.ImperativeAsync

type TestBuilder with
  member this.TryWith(f, h) = try f () with e -> h e

let some x = imperativeAsync.Return(x)
let none<'T> : ImperativeAsync<'T> = imperativeAsync.Zero()

let zero = test {
  let res = imperativeAsync { () }
  do!
    res |> Async.RunSynchronously
    |> assertEquals None
}

let ret = test {
  let res = imperativeAsync { return 0 }
  do!
    res |> Async.RunSynchronously
    |> assertEquals (Some 0)
}

let retret = test {
  let res = imperativeAsync { return 10; return 20; }
  do!
    res |> Async.RunSynchronously
    |> assertEquals (Some 10)
}

let ``retFrom Async`` = test {
  let res = imperativeAsync { return! async.Return(10); return 0 }
  do!
    res |> Async.RunSynchronously
    |> assertEquals (Some 10)
}

let ``retFrom ImperativeAsync`` = parameterize {
  source [
    (some 10, Some 10)
    (none, Some 0)
  ]
  run (fun (opt, expected) -> test {
    let res = imperativeAsync { return! opt; return 0 }
    do!
      res |> Async.RunSynchronously
      |> assertEquals expected
  })
}

let letBinding = test {
  let res = imperativeAsync {
    let! a = async { return 10 }
    return a * 2 |> string
  }
  do!
    res |> Async.RunSynchronously
    |> assertEquals (Some "20")
}

let letBindings = parameterize {
  source [
    (some 10, some 5, Some "15", "some 10. some 5")
    (some 10, none, None, "some 10, none")
    // FIXME: perameterize test case does not unique name.
    (none, some 5, None, "none, some 5")
    (none, none, None, "none, none")
  ]
  run (fun (opt1, opt2, expected, _) -> test {
    let res = imperativeAsync {
      let! a = opt1
      let! b = opt2
      return a + b |> string
    }
    do!
      res |> Async.RunSynchronously
      |> assertEquals expected
  })
}

// copy from https://github.com/BasisLib/Basis.Core/blob/f48ed463699ae0235aa58623d0f46c754a6f7326/Basis.Core.Tests/TestUtils.fs
type Disposable<'T>(x: 'T) =
  let mutable f: unit -> unit = fun () -> ()
  member this.Value = x
  member this.F with set v = f <- v
  interface System.IDisposable with
    member this.Dispose() =
      f ()

let usingBinding = parameterize {
  source [
    (none, false, None)
    (some (new Disposable<ImperativeAsync<int>>(none)), true, None)
    (some (new Disposable<ImperativeAsync<int>>(some 10)), true, Some "10")
    (some (new Disposable<ImperativeAsync<int>>(some 20)), true, Some "20")
  ]
  run (fun (opt, willDisposed, expected) -> test {
    let disposed = ref false
    let res = imperativeAsync {
      use! a = opt
      a.F <- (fun () -> disposed := true)
      let! b = a.Value
      return b |> string
    }
    do!
     res |> Async.RunSynchronously
     |> assertEquals expected
    do! !disposed |> assertEquals willDisposed
  })
}

let combine = parameterize {
  source [
    (none, false, None)
    (some 11, false, Some 11)
    (some 18, true, Some 36)
  ]
  run(fun (opt, willEven, expected) -> test {
    let isEven = ref false
    let res = imperativeAsync {
      let! a = opt
      if a % 2 = 0 then
        isEven := true
        return a * 2
      return a
    }
    do!
      res |> Async.RunSynchronously
      |> assertEquals expected
    do! !isEven |> assertEquals willEven
  })
}

let tryWith = parameterize {
  source [
    ((fun () -> none), None)
    ((fun () -> some 10), Some 10)
    ((fun () -> failwith "oops!": ImperativeAsync<int>), Some -1)
  ]
  run (fun (f, expected) -> test {
    let res = imperativeAsync {
      try
        let! a = f ()
        return a
      with
        _ -> return -1
    }
    do!
      res |> Async.RunSynchronously
      |> assertEquals expected
  })
}

let tryFinally = parameterize {
  source [
    ((fun () -> none), None)
    ((fun () -> some 10), Some 10)
    ((fun () -> failwith "oops!": ImperativeAsync<int>),  None)
  ]
  run (fun (f, expected) -> test {
    let final = ref false
    try
      let res = imperativeAsync {
        try
          let! a = f ()
          return a
        finally
          final := true
      }
      do!
        res |> Async.RunSynchronously
        |> assertEquals expected
      do! assertPred !final
    with
      e ->
        do! assertPred !final
        do! assertEquals "oops!" e.Message
  })
}

let whileLoop = parameterize {
  source [
    (none, 0, None)
    (some 1, 5, Some 1)
    (some 2, 6, Some 2)
    (some 10, 10, Some -1)
  ]
  run(fun (opt, expectedCounter, expected) -> test {
    let counter = ref 0
    let res = imperativeAsync {
      let! a = opt
      while (!counter < 5) do
        counter := !counter + a
        if !counter = 10 then
          return -1
      return a
    }
    do!
      res |> Async.RunSynchronously
      |> assertEquals expected
    do! !counter |> assertEquals expectedCounter
  })
}

let forLoop = parameterize {
  source [
    (none, 0, None)
    (some 1, 5, Some 1)
    (some -1, 3, Some 0)
  ]
  run (fun (opt, expectedCounter, expected) -> test {
    let counter = ref 0
    let res = imperativeAsync {
      let! a = opt
      for i in 1..5 do
        counter := i
        if a = -1 && i = 3 then
          return 0
      return a
    }
    do!
      res |> Async.RunSynchronously
      |> assertEquals expected
    do! !counter |> assertEquals expectedCounter
  })
}
