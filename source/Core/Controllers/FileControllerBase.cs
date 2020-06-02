using System;
using System.Net;
using System.Threading.Tasks;
using Core.Repo;
using Core.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Core.Controllers
{
    public class FileControllerBase : ControllerBase
    {
        protected FileRepo fileRepo;

        public FileControllerBase(FileRepo fileRepo)
        {
            this.fileRepo = fileRepo;
        }
        
        [Authorize]
        [RequestSizeLimit(Constants._2GB)]
        [RequestFormLimits(MultipartBodyLengthLimit = Constants._2GB)]
        protected async Task<Guid?> UploadFile()
        {
            var authorId = Guid.Parse(HttpContext.User.Identity.Name);
            
            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
                return null;

            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType), Constants.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);
                if (hasContentDispositionHeader)
                { 
                    if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                    {
                        var nameHtmlEncoded = WebUtility.HtmlEncode(contentDisposition.FileName.Value);

                        return await fileRepo.SaveAttachment(section, nameHtmlEncoded, authorId).ConfigureAwait(true);
                    }
                }

                section = await reader.ReadNextSectionAsync();
            }

            return null;
        }
    }
}