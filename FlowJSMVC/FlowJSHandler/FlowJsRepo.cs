using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace FlowUploadFilter
{
    public interface IFlowJsRepo
    {
        FlowJsPostChunkResponse PostChunk(HttpRequest request, string folder);
        FlowJsPostChunkResponse PostChunk(HttpRequest request, string folder, FlowValidationRules validationRules);
        bool ChunkExists(string folder, HttpRequest request);
    }
    public class FlowJsRepo : IFlowJsRepo
    {
        public FlowJsPostChunkResponse PostChunk(HttpRequest request, string folder)
        {
            return PostChunkBase(request, folder, null);
        }

        public FlowJsPostChunkResponse PostChunk(HttpRequest request, string folder, FlowValidationRules validationRules)
        {
            return PostChunkBase(request, folder, validationRules);
        }

        public bool ChunkExists(string folder, HttpRequest request)
        {
            var identifier = request.Query["flowIdentifier"];
            var chunkNumber = int.Parse(request.Query["flowChunkNumber"]);
            var chunkFullPathName = GetChunkFilename(chunkNumber, identifier, folder);
            return File.Exists(Path.Combine(folder, chunkFullPathName));
        }

        private FlowJsPostChunkResponse PostChunkBase(HttpRequest request, string folder, FlowValidationRules validationRules)
        {
            var chunk = new FlowChunk();
            var requestIsSane = chunk.ParseForm(request.Form);
            if (!requestIsSane)
            {
                Console.WriteLine($"Experienced an error in the submitted form - form damaged?");
                var errResponse = new FlowJsPostChunkResponse();
                errResponse.Status = PostChunkStatus.Error;
                errResponse.ErrorMessages.Add("damaged");
            }

            List<string> errorMessages = null;
            var file = request.Form.Files[0];

            var response = new FlowJsPostChunkResponse { FileName = chunk.FileName, Size = chunk.TotalSize };

            var chunkIsValid = true;
            Console.WriteLine($"Processing validation rules");
            if (validationRules != null)
                chunkIsValid = chunk.ValidateBusinessRules(validationRules, out errorMessages);

            if (!chunkIsValid)
            {
                Console.WriteLine($"Experienced an error while validating rules {errorMessages.Aggregate((s,a)=>s+" "+a)}");
                response.Status = PostChunkStatus.Error;
                response.ErrorMessages = errorMessages;
                return response;
            }

            var chunkFullPathName = GetChunkFilename(chunk.Number, chunk.Identifier, folder);
            try
            {
                // create folder if it does not exist
                Console.WriteLine($"Opening or creating folder {folder}");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                // save file
                using (var chunkFile = File.Create(chunkFullPathName))
                {
                    Console.WriteLine($"Saving chunk file {chunkFullPathName}");
                    file.CopyTo(chunkFile);
                }
            }
            catch (Exception)
            {
                Console.WriteLine($"Error saving chunk");
                throw;
            }

            // see if we have more chunks to upload. If so, return here
            for (int i = 1, l = chunk.TotalChunks; i <= l; i++)
            {
                var chunkNameToTest = GetChunkFilename(i, chunk.Identifier, folder);
                Console.WriteLine($"Checking if chuck exists already {chunkNameToTest}");
                var exists = File.Exists(chunkNameToTest);
                if (!exists)
                {
                    Console.WriteLine($"Some chunks are missing. Sending PartlyDone response");
                    response.Status = PostChunkStatus.PartlyDone;
                    return response;
                }
            }

            // if we are here, all chunks are uploaded
            var fileAry = new List<string>();
            Console.WriteLine($"All chunks done. the full list of chunks is:");
            for (int i = 1, l = chunk.TotalChunks; i <= l; i++)
            {
                Console.WriteLine("flow-" + chunk.Identifier + "." + i);
                fileAry.Add("flow-" + chunk.Identifier + "." + i);
            }

            MultipleFilesToSingleFile(folder, fileAry, chunk.FileName);

            Console.WriteLine($"Deleting old chunks");
            for (int i = 0, l = fileAry.Count; i < l; i++)
            {
                try
                {
                    Console.WriteLine($"Deleting {fileAry[i]}");
                    File.Delete(Path.Combine(folder, fileAry[i]));
                }
                catch (Exception)
                {
                    Console.WriteLine($"Error deleting chunk file");
                }
            }

            response.Status = PostChunkStatus.Done;
            return response;

        }



        private void MultipleFilesToSingleFile(string dirPath, IEnumerable<string> fileAry, string destFile)
        {
            Console.WriteLine($"Merging multiple files into a single to save into {destFile}");
            Console.WriteLine($"Check if file exists {destFile}");
            if (File.Exists(Path.Combine(dirPath, destFile)))
            {
                Console.WriteLine($"Deleting file {destFile}");
                File.Delete(Path.Combine(dirPath, destFile));
            }
            using (var destStream = new FileStream(Path.Combine(dirPath, destFile), FileMode.Create))
            {
                foreach (string filePath in fileAry)
                {
                    Console.WriteLine($"Adding {filePath} into {destFile}");
                    using (var sourceStream = File.OpenRead(Path.Combine(dirPath, filePath)))
                        sourceStream.CopyTo(destStream); // You can pass the buffer size as second argument.
                }
                destStream.Flush();
            }
            Console.WriteLine($"Succefully merged file {destFile}");
        }

        private string GetChunkFilename(int chunkNumber, string identifier, string folder)
        {
            Console.WriteLine($"Chunk filename is {Path.Combine(folder, "flow-" + identifier + "." + chunkNumber)}");
            return Path.Combine(folder, "flow-" + identifier + "." + chunkNumber);
        }
    }
}
