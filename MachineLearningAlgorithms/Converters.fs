module Converters
open System.Collections.Generic

let immutableToMutableNodeList<'a> (immutable:list<'a>) = let MutableList = new List<'a>()
                                                          immutable |> List.iter (fun node -> MutableList.Add node)
                                                          MutableList

let mutableToImmutableNodeList<'a> (mut:List<'a>) = let immutableList:seq<'a> = query {
                                                                                         for element in mut do
                                                                                         select element
                                                                                      }
                                                    immutableList |> Seq.toList