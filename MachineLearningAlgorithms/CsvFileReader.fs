module CsvFileReader

open FSharp.Data
open FSharp.Data.CsvExtensions
open System.IO

let readCsvFile (path:string) = CsvFile.Load(path)

let getCsvFileHeaders (csvFile:CsvFile) = match csvFile.Headers with 
                                          | None -> raise (new IOException("Empty File"))                                                      
                                          | _ -> csvFile.Headers.Value |> Array.toSeq
                                                                       |> Seq.last
                                                                       |> (fun lastHeader -> 
                                                                           match lastHeader with
                                                                           | "Class" -> csvFile.Headers.Value |> Array.toSeq
                                                                           | _ -> raise (new InvalidDataException("No \"Class\" header found, the data set must have a header with the name \"Class\"")))


let getCsvFileRows (csvFile:CsvFile) = csvFile.Rows

let getRows (csvFile:CsvFile) = csvFile |> getCsvFileRows 
                                        |> Seq.map (fun csvRow -> csvRow.Columns |> Array.toSeq)



let rowsCount (rows:seq<CsvRow>) = Seq.length rows

let headersCount (csvFile:CsvFile) = getCsvFileHeaders csvFile |> Seq.length