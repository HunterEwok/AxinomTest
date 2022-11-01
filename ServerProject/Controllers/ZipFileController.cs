using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using ServerProject.Models;
using Microsoft.AspNetCore.Http;
using ServerProject.Common;
using Microsoft.AspNetCore.Authorization;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZipFileController : ControllerBase
    {
        private readonly ILogger<ZipFileController> _logger;

        public ZipFileController(ILogger<ZipFileController> logger)
        {
            _logger = logger;
        }

        [Authorize]
        [HttpPost]
        public ActionResult PostZipStructure([FromBody] ZipFileInfo data)
        {
            _logger.LogDebug("PostZipStructure executed");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            ActionResult actionResult;

            try
            {
                _logger.LogDebug("Zip structure data is sent to DB");

                try
                {
                    data.ContentTree = CryptHelper.DecryptAll(data.ContentTree);

                    KeyValuePair<string, bool> response = DatabaseHelper.SaveZipStructure(data.FileName, data.ContentTree).Result;

                    _logger.LogDebug("PostZipStructure completed");

                    actionResult = StatusCode(response.Value ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError, 
                        response.Key);
                }
                catch (Exception e)
                {
                    _logger.LogDebug("PostZipStructure saving to DB raised exception" + Environment.NewLine + e.Message);

                    actionResult = StatusCode(StatusCodes.Status500InternalServerError, "Error on saving to DB");
                }
            }
            catch
            {
                _logger.LogDebug("PostZipStructure handled with exception");

                actionResult = StatusCode(StatusCodes.Status400BadRequest, "Wrong ZIP file structure data");
            }

            return actionResult;
        }
    }
}
