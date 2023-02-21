
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api_Azure_Dio.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ArquivosController:ControllerBase
    {
        private readonly string _connectionString;
        private readonly string _conteinerName;

        public ArquivosController(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("BlobConnectionString");
            _conteinerName = configuration.GetValue<string>("BlobContainerName");
        }

        [HttpPost("Upload")]
        public IActionResult UploadArquivo(IFormFile arquivo){
            BlobContainerClient container = new(_connectionString, _conteinerName);
            BlobClient blob = container.GetBlobClient(arquivo.FileName);
            
            using var data = arquivo.OpenReadStream();

            blob.Upload(data, new BlobUploadOptions{
                HttpHeaders = new BlobHttpHeaders{ContentType = arquivo.ContentType}
            });

            return Ok(blob.Uri.ToString());
        }


        [HttpGet("Dowload/{nomeArquivo}")]
        public IActionResult DowloadArquivo(string nomeArquivo){
            BlobContainerClient container = new(_connectionString, _conteinerName);
            BlobClient blob = container.GetBlobClient(nomeArquivo); 

            if(!blob.Exists())
                return BadRequest();

            var retorno = blob.DownloadContent();
            return File(retorno.Value.Content.ToArray(), retorno.Value.Details.ContentType, blob.Name);  
        }

        [HttpDelete("Deletar/{nomeArquivo}")]
        public IActionResult ApagarArquivo(string nomeArquivo){
            BlobContainerClient container = new(_connectionString, _conteinerName);
            BlobClient blob = container.GetBlobClient(nomeArquivo); 

            blob.DeleteIfExists();

            return NoContent();
        }

        //Listar todos os blobs 
        [HttpGet("Listar")]
        public IActionResult ListarArquivos(){
            List<BlobDto> blobDto = new List<BlobDto>(); 
            BlobContainerClient container = new(_connectionString, _conteinerName);

            foreach (var blob in container.GetBlobs())
            {
                blobDto.Add(new BlobDto{
                    Name = blob.Name,
                    Type = blob.Properties.ContentType,
                    Uri = container.Uri.AbsoluteUri + "/" + blob.Name

                });
            }

            return Ok(blobDto);
        }
    }
}            