module DecisionTree

open HelperFunctions
open Types
open Converters
open CsvFileReader
open System.Collections.Generic
open System.Linq
open FSharp.Data
open FSharp.Data.CsvExtensions



let reduce (rows:seq<CsvRow>) 
           (parents:seq<node>) 
           :list<CsvRow> = let mutable reduced = rows |> Seq.toList 
                                                      |> immutableToMutableNodeList
                           if (Seq.length parents) > 1 then
                               for i in 1 .. ((Seq.length parents)) do
                                  if not (i = (Seq.length parents)) then
                                     reduced <- (query {
                                                          for row in reduced do
                                                          where (row?((parents |> Seq.item (i-1)).content) = (parents |> Seq.item i).branch.Value)
                                                          select row
                                                       }
                                                 |> Seq.toList
                                                 |> immutableToMutableNodeList)
                           mutableToImmutableNodeList reduced



let tuplesCountPerClass (rows:seq<CsvRow>) = rows |> Seq.groupBy (fun row -> row?Class)
                                                  |> Seq.map (fun tuple -> Seq.length (snd tuple))

let Pi n s = ((float)n/(float)(rowsCount (s)))

let entropy (rows:seq<CsvRow>) = tuplesCountPerClass rows |> Seq.map (fun n -> - (Pi n rows) * log2(Pi n rows))
                                                          |> Seq.sum
                                          
let entropiesDependingOn (rows:seq<CsvRow>) 
                         (parents:seq<node>) = rows |> reduce 
                                                   <| parents
                                                    |> Seq.groupBy (fun row -> row?((parents |> Seq.item (Seq.length parents - 1)).content))
                                                    |> Seq.map (fun groupedRows -> (fst groupedRows,entropy (snd groupedRows)))
                                                    |> Seq.sortByDescending (fun e -> snd e) 

let gain (s:seq<CsvRow>) 
         (a:string) = s |> Seq.groupBy (fun row -> row.[a])
                        |> Seq.map (fun groupedRows -> ((float)(Seq.length (snd groupedRows))/(float)(rowsCount s)) * entropy (snd groupedRows))
                        |> Seq.sum
                        |> negate
                        |> add 
                       <| entropy s                    

let gainSuchAs (s:seq<CsvRow>) 
               (parents:seq<node>) 
               (b:string) 
               (c:string) = s |> reduce  
                             <| parents
                              |> Seq.groupBy (fun row -> row?((parents |> Seq.item (Seq.length parents - 1)).content))
                              |> Seq.filter (fun groupedRows -> (fst groupedRows) = b)
                              |> Seq.head
                              |> snd
                              |> gain 
                             <| c

let rec makeTree (data:CsvFile)
                 (excludedHeaders:List<string>)
                 (parents:seq<node> option)
                 : list<node> option = match parents with
                                       | None -> match data |> getCsvFileHeaders
                                                            |> Seq.except excludedHeaders
                                                            |> Seq.map (fun header -> gain (getCsvFileRows data) header)
                                                            |> Seq.max with
                                                 | 0. -> None
                                                 | _  -> let node = {
                                                                       content = data |> getCsvFileHeaders
                                                                                      |> Seq.except (excludedHeaders) 
                                                                                      |> Seq.maxBy (fun header -> gain (getCsvFileRows data) header);
                                                                       branch = None;
                                                                       children = None;
                                                                       isLeaf = false;
                                                                    };
                                                         excludedHeaders.Add node.content
                                                         let nodeChildren = makeTree data excludedHeaders (Some(Seq.singleton node))
                                                         let node' = {
                                                                        content = node.content;
                                                                        branch = node.branch;
                                                                        children = nodeChildren;
                                                                        isLeaf = false;
                                                                     };
                                                         Some ([node'])
                                       
                                       | Some(parents) -> let bestBranchEntropies = data |> getCsvFileRows
                                                                                         |> entropiesDependingOn 
                                                                                        <| parents

                                                          let children = bestBranchEntropies 
                                                                         |> Seq.map (fun bestBranch 
                                                                                       -> match (snd bestBranch) with
                                                                                          | 0. -> let node = {
                                                                                                                content = data |> getCsvFileRows
                                                                                                                               |> reduce 
                                                                                                                              <| parents
                                                                                                                               |> Seq.filter (fun row -> row?((Seq.last parents).content) = fst bestBranch)
                                                                                                                               |> Seq.map (fun row -> row?Class)        
                                                                                                                               |> Seq.head

                                                                                                                branch = Some(fst bestBranch);
                                                                                                                children = None;
                                                                                                                isLeaf = true;
                                                                                                             }
                                                                                                  node
                                                                                          | _ -> let attribut = data |> getCsvFileHeaders
                                                                                                                     |> Seq.except excludedHeaders
                                                                                                                     |> Seq.maxBy (fun attribut -> data |> getCsvFileRows 
                                                                                                                                                        |> gainSuchAs 
                                                                                                                                                       <| parents
                                                                                                                                                       <| (fst bestBranch) 
                                                                                                                                                       <| attribut)                                                                                                                                
                                                                                                 let node = {
                                                                                                               content = attribut;
                                                                                                               branch = Some(fst bestBranch);
                                                                                                               children = None;
                                                                                                               isLeaf = false;
                                                                                                            }
                                                                                                 excludedHeaders.Add node.content   
                                                                                                 node                   
                                                                                     )
                                                                         |> Seq.toList
                                                          let children' = new List<list<node> option>()
                                                          children |> Seq.iter (fun child -> if not child.isLeaf then
                                                                                                let p = Seq.append parents (Seq.singleton child)
                                                                                                (makeTree data excludedHeaders (Some(p))).Value |> Some
                                                                                                                                                |> children'.Add
                                                                                             else
                                                                                                children'.Add (None)
                                                                                )
                                                          children |> Seq.mapi (fun i child -> {
                                                                                                   content = child.content;
                                                                                                   branch = child.branch;
                                                                                                   children = children'.[i];
                                                                                                   isLeaf = child.isLeaf;
                                                                                               })
                                                                   |> Seq.toList
                                                                   |> Some