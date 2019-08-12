# Tagfile data extraction tool
This tool is designed to export the data contained in a tag file to a csv.

## Usage
You can use the tag file extractor tool by calling it as below all subfolders will be traversed. Output is a csv with each epoch as a row and ATTRIBUTES_TO_INCLUDE as the columns:

 `dotnet \VSS.TRex.Tools.TagfileExporter.dll FOLDER_TO_PROCESS [ATTRIBUTES_TO_INCLUDE]`

 If the optional ATTRIBUTES_TO_INCLUDE is omitted then the default attributes of DataTime, DataLeft, DataRight will be used. The generated csv will be located with the source tag file and have the same name.

e.g. 
dotnet .\VSS.TRex.Tools.TagfileExporter.dll "C:\stuff\CAT 825K 05015" DataTime DataLeft DataRight ICMDPValues

To see the list of attributes you can include in the export use the following command:
`dotnet .\VSS.TRex.Tools.TagfileExporter.dll -a`
