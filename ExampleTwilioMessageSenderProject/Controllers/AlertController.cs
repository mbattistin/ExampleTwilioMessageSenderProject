using ExampleTwilioMessageSenderProject.Domain.Contract;
using ExampleTwilioMessageSenderProject.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace ExampleTwilioMessageSenderProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AlertController : ControllerBase
    {
        private readonly IAlertSenderService _alertSenderService;

        public AlertController(IAlertSenderService alertSenderService)
        {
            _alertSenderService = alertSenderService;
        }

        [HttpPost]
        public async Task<IActionResult> FireAlertaAsync([FromBody] Alert alert)
        {

            var alertResponse = await _alertSenderService.SendAlertAsync(alert);

            if (!alertResponse.Success)
                return BadRequest(alertResponse.ErrorMessage);

            return Ok(new { alertResponse.XIdMessage });
        }
    }
}