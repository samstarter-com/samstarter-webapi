using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWI.SoftStock.ServerApps.WebApplicationContracts.DocumentService;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.WebApi.Common;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SWI.SoftStock.WebApi.Controllers
{
    [ApiController]
    [Authorize(Policy = Constants.PolicyUser)]
    [Route("api/document")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService documentService;

        public DocumentController(IDocumentService documentService)
        {
            this.documentService = documentService;
        }

        [HttpPost]
        [Route("uploadId")]
        public IActionResult CreateUploadId()
        {
            var data = this.documentService.CreateUploadId();
            return this.Ok(data);
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> Upload()
        {
            if (this.Request.Form.Files.Count <= 0) return this.BadRequest();

            try
            {
                var file = this.Request.Form.Files[0];
                var uploadId = Guid.NewGuid().ToString();
                var doc = await this.ToUploadedDocumentModelAsync(file, uploadId);
                await this.documentService.SaveAsync(doc);
                return this.Ok(uploadId);
            }
            catch (Exception ex)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private async Task<UploadedDocumentModel> ToUploadedDocumentModelAsync(IFormFile file, string uploadId)
        {
            var id = Guid.Parse(uploadId);
            await using var fileStream = new MemoryStream();
            await file.CopyToAsync(fileStream);
            var fileName = Path.GetFileName(file.FileName);
            var fileData = fileStream.ToArray();
            return new UploadedDocumentModel { Name = fileName, Content = fileData, UploadId = id };
        }
    }
}
