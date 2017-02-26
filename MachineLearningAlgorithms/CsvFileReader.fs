module CsvFileReader

open FSharp.Data
open FSharp.Data.CsvExtensions

let readCsvFile (path:string) = CsvFile.Load(path)

let getCsvFileHeaders (csvFile:CsvFile) = match csvFile.Headers with 
                                          | None -> Seq.empty                                                            
                                          | _ -> csvFile.Headers.Value |> Array.toSeq 

let getCsvFileRows (csvFile:CsvFile) = csvFile.Rows

let getRows (csvFile:CsvFile) = csvFile |> getCsvFileRows 
                                        |> Seq.map (fun csvRow -> csvRow.Columns |> Array.toSeq)



let rowsCount (rows:seq<CsvRow>) = Seq.length rows

let headersCount (csvFile:CsvFile) = getCsvFileHeaders csvFile |> Seq.length